using UnityEngine;

// RegionBlender.cs
using System.Collections.Generic;
using System.Linq;
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

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < updateInterval) return;
        _timer = 0f;
        EvaluateBlend(player.transform.position);
    }

    public void EvaluateBlend(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, maxBlendDistance, zoneLayer);

        if (hits.Length == 0) return;

        // Build weighted list
        var influences = new List<(Region region, float weight)>(hits.Length);
        float total = 0f;

        foreach (Collider hit in hits)
        {
            var (region, weight) = hit.GetComponent<RegionZone>().Sample(position);
            if (weight <= 0f) continue;
            influences.Add((region, weight));
            total += weight;
        }

        if (influences.Count == 0 || total <= 0f) return;

        // Normalize
        var blendTargets = new List<(Region region, float weight)>(influences.Count);
        foreach (var (region, weight) in influences)
            blendTargets.Add((region, weight / total));

        WeatherManager.Instance.SetRegionInfluences(blendTargets);
        AmbienceManager.Instance.SetRegionInfluences(blendTargets);
    }
}
