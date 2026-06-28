using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using Weather;

/// <summary>
/// One Region in the game world, for audio and weather purposes.
/// uses meshcolliders / promesh builder
/// IMPORTANT: these must be convex
/// 
/// ~Lars
/// </summary>
//[RequireComponent(typeof(MeshCollider))]
public class Region : MonoBehaviour
{
    public float blendDistance = 20f;

    [SerializeField] public EventReference ambienceEvent;
    [SerializeField] public WeatherTypeProfile weatherTypeProfile;
    
    private MeshCollider[] _colliders;

    private void Awake()
    {
        _colliders = GetComponentsInChildren<MeshCollider>();

        if(_colliders.Length == 0) Debug.LogError($"[Region: {gameObject.name}]: Has no colliders attached in children");
        foreach (MeshCollider col in _colliders)
        {
            col.GetComponent<MeshRenderer>().enabled = false;
        }
        enabled = false;
        //GetComponent<MeshRenderer>().enabled = false; //turn off in play mode
    }

    void OnEnable()
        {
            Debug.Log($"[Region: {gameObject.name}]: enabled!");
            //enable the particles again
            // foreach (var ps in particleSystems.Keys)
            //     ps.Play();

            // //turn on the behaviors
            // foreach(var behavior in _behaviours)
            // {
            //    behavior.enabled = true; 
            // }
        }

        void OnDisable()
        {
            Debug.Log($"[Region: {gameObject.name}]: disabled!");

            // volume.weight = 0f; //to be sure

            // //turn off the particle emitters, and wait for their particles to die and stop them fully (resources)
            // foreach (var ps in particleSystems.Keys)
            // {
            //     ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            //     StartCoroutine(WaitForParticlesAndStop());
            // }

            // //turn off the behaviors too
            // foreach(var behavior in _behaviours)
            // {
            //    behavior.enabled = false; 
            // }
        }

    /// <summary>
    /// Compute the influence of this region on the input position.
    /// this position should logically be the player.
    /// 
    /// returns 1f if the position is inside
    /// 
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public float Sample(Vector3 worldPos)
    {
        Vector3 closest;
        var weight = 0f;
        
        foreach (var col in _colliders)
        {
            closest = col.ClosestPoint(worldPos);
            //early exit if we are fully inside any of the colliders
            if(closest == worldPos)
            {
                return 1f;
            }
            else weight = Mathf.Max(weight,  Mathf.Clamp01(1f - Vector3.Distance(worldPos, closest) / blendDistance));
        }

        return weight;
    }
}
