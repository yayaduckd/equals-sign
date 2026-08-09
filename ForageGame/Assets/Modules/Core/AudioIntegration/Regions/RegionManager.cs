using UnityEngine;

// RegionBlender.cs
using System.Collections.Generic;
using System.Linq;
using System;
using TDK.PlayerSystem;
using AudioIntegration;
using Weather;


public class RegionManager : MonoBehaviour
{
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private float maxBlendDistance = 20f;
    [SerializeField] private LayerMask zoneLayer;

    [SerializeField] private Player player;

    [SerializeField] public WeatherTypeProfile defaultWeather;

    [SerializeField] public float currentDustiness = 0f;

    private Dictionary<Region, float> _lastRegionInfluences = new();
    
    public static RegionManager Instance { get; private set; }

    private float _timer;

    private readonly HashSet<Region> _seenRegions = new();
 
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
        var regionInfluences = EvaluateRegionBlend(player.transform.position);

        //only apply the blending if the result is different
        if (regionInfluences.OrderBy(kv => kv.Key.GetInstanceID()).SequenceEqual(_lastRegionInfluences.OrderBy(kv => kv.Key.GetInstanceID()))) 
        {
            Debug.LogError("[RegionManager] EvaluateRegionBlend returned no influences on startup, check the player spawn position!");
            //return;
        }
        //turn on and off the profiles, to disable the world particles in them when unused
        foreach (var region in regionInfluences.Keys)
                region.enabled = true;
        
        foreach (var region in _lastRegionInfluences.Keys)
                region.enabled = !regionInfluences.ContainsKey(region);

        _lastRegionInfluences = regionInfluences;

        //make the weather snap on startup, no transition
        WeatherManager.Instance.SetRegionInfluencesInstant(ToWeatherInfluences(regionInfluences));
        AmbienceManager.Instance.SetRegionInfluences(ToAmbienceInfluences(regionInfluences));
    }

    //TODO: this timer causes choppyness on weather blending... smoothen it out or just take the performance hit
    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < updateInterval) return;
        _timer = 0f;

        var regionInfluences = EvaluateRegionBlend(player.transform.position);
        //only apply the blending if the result is different
        if (regionInfluences.OrderBy(kv => kv.Key.GetInstanceID()).SequenceEqual(_lastRegionInfluences.OrderBy(kv => kv.Key.GetInstanceID()))) 
        {
            // Debug.Log("[RegionManager] No change in region blend, skipping application!");
            return;
        }
        // else
        // {
        //     foreach (var region in regionInfluences.Keys)
        //     {
        //         Debug.Log($"[RegionManager] Supposed influence region: {region.name}, Weight: {regionInfluences[region]}");
        //     }
        // }

        //turn on and off the profiles, to disable the world particles in them when unused
        foreach (var region in regionInfluences.Keys)
                region.enabled = true;
        
        foreach (var region in _lastRegionInfluences.Keys)
                if (region != null) region.enabled = regionInfluences.ContainsKey(region); //this is a check for the sleeping case, which unloads the world

        _lastRegionInfluences = regionInfluences;

    
        //apply
        currentDustiness = regionInfluences.Sum(kv => kv.Key.dustiness * kv.Value);
        WeatherManager.Instance.SetRegionInfluences(ToWeatherInfluences(regionInfluences));
        AmbienceManager.Instance.SetRegionInfluences(ToAmbienceInfluences(regionInfluences));
    }

    private Dictionary<Region, float> EvaluateRegionBlend(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, maxBlendDistance, zoneLayer);

        //Regions already sampled
        _seenRegions.Clear();

        var influences = new Dictionary<Region, float>();


        foreach (Collider hit in hits)
        {
            var region = hit.GetComponentInParent<Region>();
            //for multi-collider zones, only sample once
            if(_seenRegions.Add(region))
            {

                var weight = region.Sample(position);
                if (weight <= 0f) continue;

                //weather
                if (influences.TryGetValue(region, out float existing))
                {
                    // Debug.Log($"[RegionManager]: duplicate weatherProfile detected: {weatherProfile}");
                    influences[region] = existing + weight;
                }
                else
                    influences[region] = weight;
            }
        }

        //Normalize the weights from 0-1
        float total = influences.Values.Sum();


        // Normalize, but not higher than the actual influence is (i.e., edge case stuff, should never actually matter)
        foreach (var r in influences.Keys.ToList()) // i.e. to prevent the 'enumeration may not complete' whining
        {
            influences[r] = Mathf.Min(influences[r] / total, influences[r]);
        }
        
        return influences;
    }

    private Dictionary<string, float> ToWeatherInfluences(Dictionary<Region, float> influences)
    {
        var weather = new Dictionary<string, float>();

        foreach (var (region, weight) in influences)
        {
            if (weather.TryGetValue(region.weatherTypeProfile.Id, out float existing))
                weather[region.weatherTypeProfile.Id] = existing + weight;
            else
                weather[region.weatherTypeProfile.Id] = weight;
        }
        //fill the weather with default weather if required
        //Important: this is not done for audio
        float total = weather.Values.Sum();
        if (total < 1f)
        {
            if (weather.TryGetValue(defaultWeather.Id, out float existing)) //do not override if the default weather is already present
                weather[defaultWeather.Id] = existing + (1f-total);
            else
                weather[defaultWeather.Id] = 1f-total;
        }

        return weather;
    }

    private Dictionary<FMODUnity.EventReference, float> ToAmbienceInfluences(Dictionary<Region, float> influences)
    {
        var ambience = new Dictionary<FMODUnity.EventReference, float>();

        foreach (var (region, weight) in influences)
        {
            if(region.ambienceEvent.IsNull)
            {
                Debug.Log($"[RegionManager]: Region has no ambience event assigned: {region}. Skipping!");
            }
            else if (ambience.TryGetValue(region.ambienceEvent, out float existing))
                ambience[region.ambienceEvent] = existing + weight;
            else
                ambience[region.ambienceEvent] = weight;
        }

        return ambience;
    }
}
