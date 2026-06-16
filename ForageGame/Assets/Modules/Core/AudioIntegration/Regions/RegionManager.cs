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

    private Dictionary<Region, float> _lastInfluences = new();
    
    public static RegionManager Instance { get; private set; }

    private float _timer;

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
        EvaluateBlend(player.transform.position);
    }

    //TODO: this timer causes choppyness on weather blending... smoothen it out or just take the performance hit
    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < updateInterval) return;
        _timer = 0f;
        EvaluateBlend(player.transform.position);
    }

    private void EvaluateBlend(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, maxBlendDistance, zoneLayer);

        if (hits.Length == 0) return;

        // Build weighted list
        var influences = new Dictionary<Region, float>();

        foreach (Collider hit in hits)
        {
            var (region, weight) = hit.GetComponent<RegionZone>().Sample(position);
            if (weight <= 0f) continue;

            if (influences.TryGetValue(region, out float existing)) //convex regions have multiple colliders, but should not take up more power in the blending
                influences[region] = Mathf.Max(existing, weight); // take highest for split convex zones
            else
                influences[region] = weight;
        }

        // Total computed after deduplication so split convex zones don't inflate it
        float total = influences.Values.Sum();

        if (influences.Count == 0 || total <= 0f) return;

        // Normalize, but not higher than the actual influence is (i.e., edge case stuff, should never actually matter)
        var blendTargets = new Dictionary<Region, float>(influences.Count);
        foreach (var (region, weight) in influences)
            blendTargets[region] = Mathf.Min(weight / total, weight);


        //only apply the blending if the result is different
        if (blendTargets.OrderBy(kv => kv.Key.GetInstanceID()).SequenceEqual(_lastInfluences.OrderBy(kv => kv.Key.GetInstanceID()))) 
        {
            // Debug.Log("[RegionManager] No change in region blend, skipping application!");
            return;
        }
        _lastInfluences = blendTargets;

        WeatherManager.Instance.SetRegionInfluences(blendTargets);
        AmbienceManager.Instance.SetRegionInfluences(blendTargets);
    }
}
