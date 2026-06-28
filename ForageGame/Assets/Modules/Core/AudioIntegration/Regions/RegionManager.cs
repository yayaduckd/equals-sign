using UnityEngine;

// RegionBlender.cs
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using TDK.PlayerSystem;
using AudioIntegration;
using Weather;


public class RegionManager : MonoBehaviour
{
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private float maxBlendDistance = 20f;
    [SerializeField] private LayerMask zoneLayer;

    [SerializeField] private Player player;

    [SerializeField] public Region defaultRegion;

    private Dictionary<string, float> _lastWeatherInfluences = new();
    
    public static RegionManager Instance { get; private set; }

    private float _timer;

    private readonly HashSet<RegionZone> _seenRegions = new();

    private void Awake() 
    { 
        //May only be one instance ofc
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }


    private void Start()
    {
        var (weatherInfluences, ambienceInfluences) = EvaluateBlend(player.transform.position);

        //only apply the blending if the result is different
        if (weatherInfluences.OrderBy(kv => kv.Key).SequenceEqual(_lastWeatherInfluences.OrderBy(kv => kv.Key))) 
        {
            Debug.LogError("[RegionManager] No change in weather blend on startup? player is in a weird spot!");
            return;
        }
        _lastWeatherInfluences = weatherInfluences;

        //make the weather snap on startup, no transition
        WeatherManager.Instance.SetRegionInfluencesInstant(weatherInfluences);
        AmbienceManager.Instance.SetRegionInfluences(ambienceInfluences);
    }

    //TODO: this timer causes choppyness on weather blending... smoothen it out or just take the performance hit
    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < updateInterval) return;
        _timer = 0f;
        var (weatherInfluences, ambienceInfluences) = EvaluateBlend(player.transform.position);

        //only apply the blending if the result is different
        if (weatherInfluences.OrderBy(kv => kv.Key).SequenceEqual(_lastWeatherInfluences.OrderBy(kv => kv.Key))) 
        {
            Debug.Log("[RegionManager] No change in region blend, skipping application!");
            return;
        }
        _lastWeatherInfluences = weatherInfluences;

        WeatherManager.Instance.SetRegionInfluencesInstant(weatherInfluences);
        AmbienceManager.Instance.SetRegionInfluences(ambienceInfluences);
    }

    private (Dictionary<string, float>, Dictionary<FMODUnity.EventReference, float>) EvaluateBlend(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, maxBlendDistance, zoneLayer);

        _seenRegions.Clear();

        //if (hits.Length == 0) return new Dictionary<Region, float>();

        // Build weighted list
        var influences = new Dictionary<Region, float>();

        var weather = new Dictionary<string, float>();
        var ambience = new Dictionary<FMODUnity.EventReference, float>();

        //zones already sampled
        // var seenZones = new HashSet<RegionZone>();

        foreach (Collider hit in hits)
        {
            var region = hit.GetComponentInParent<RegionZone>();
            //for multi-collider zones, only sample once
            if(_seenRegions.Add(region))
            {
                var (r, weight) = region.Sample(position);

                var (weatherProfile, ambienceEvent) = (r.weatherTypeProfile.Id, r.ambienceEvent);
                if (weight <= 0f) continue;

                //weather
                if (weather.TryGetValue(weatherProfile, out float existing))
                {
                    Debug.Log($"[RegionManager]: duplicate weatherProfile detected: {weatherProfile}");
                    weather[weatherProfile] = existing + weight;
                }
                else
                    weather[weatherProfile] = weight;


                //ambience
                if(ambienceEvent.IsNull) //for regions without ambience, for some reason
                {
                    Debug.Log($"[RegionManager]: Region has no ambience event assigned: {r}. Skipping!");
                }
                else if (ambience.TryGetValue(ambienceEvent, out existing))
                {
                    Debug.Log($"[RegionManager]: duplicate ambience detected: {ambienceEvent}");
                    ambience[ambienceEvent] = existing + weight;
                }
                else
                    ambience[ambienceEvent] = weight;
            }
        }

        //Normalize the weights from 0-1
        float total = weather.Values.Sum();


        // Normalize, but not higher than the actual influence is (i.e., edge case stuff, should never actually matter)
        foreach (var p in weather.Keys.ToList()) // i.e. to prevent the 'enumeration may not complete' whining
        {
            weather[p] = Mathf.Min(weather[p] / total, weather[p]);
        }

        foreach (var e in ambience.Keys.ToList()) // i.e. to prevent the 'enumeration may not complete' whining
        {
            ambience[e] = Mathf.Min(ambience[e] / total, ambience[e]);
        }
        
        //fill the weather with default weather if required
        //Important: this is not done for audio
        total = weather.Values.Sum();
        if (total < 1f)
        {
            if (weather.TryGetValue(defaultRegion.weatherTypeProfile.Id, out float existing)) //do not override if the default weather is already present
                weather[defaultRegion.weatherTypeProfile.Id] = existing + (1f-total);
            else
                weather[defaultRegion.weatherTypeProfile.Id] = 1f-total;
            //Debug.Log($"[RegionManager] influences do not sum to 1, filling with default Region: {1f-total}!");
        }

        return (weather, ambience);
    }
}
