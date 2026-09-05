using System.Collections.Generic;
using System.Collections;
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

    [SerializeField] public float dustiness = 0f;

    private MeshCollider[] _colliders;

    [SerializeField] private List<ParticleSystem> regionParticles;

    private Coroutine _waitForParticlesCoroutine;

    private void Awake()
    {
        _colliders = GetComponentsInChildren<MeshCollider>();

        if (_colliders.Length == 0) Debug.LogError($"[Region: {gameObject.name}]: Has no colliders attached in children");
        foreach (MeshCollider col in _colliders)
        {
            col.GetComponent<MeshRenderer>().enabled = false;
        }

        regionParticles = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
        foreach (var ps in regionParticles)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); //to hopefully prevent the coroutine from dragging on too long
        }
        enabled = false;
        //GetComponent<MeshRenderer>().enabled = false; //turn off in play mode
    }

    void OnEnable()
    {
        Debug.Log($"[Region: {gameObject.name}]: enabled!");

        //if the waiting coroutine is running, stop it
        if (_waitForParticlesCoroutine != null)
        {
            StopCoroutine(_waitForParticlesCoroutine);
            _waitForParticlesCoroutine = null;
        }

        //enable the particles again
        foreach (var ps in regionParticles)
        {
            // Debug.Log($"[Region: {gameObject.name}]: enabling particle systems: {ps.gameObject.name}");
            ps.Play();
        }
    }

    void OnDisable()
    {
        // Debug.Log($"[Region: {gameObject.name}]: disabled!"); // TIM SAYS: shhhhhh - quite down buddy

        //turn off the particle emitters, and wait for their particles to die and stop them fully (resources)
        bool anyPlaying = false;
        foreach (var ps in regionParticles)
        {
            if (ps.isPlaying)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                anyPlaying = true;
            }
        }
        if (anyPlaying) _waitForParticlesCoroutine = StartCoroutine(WaitForParticlesAndStop());
    }
    private IEnumerator WaitForParticlesAndStop()
    {
        // Wait until all particles have naturally died
        bool anyAlive = true;
        while (anyAlive)
        {
            anyAlive = false;
            foreach (var ps in regionParticles)
            {
                if (ps.particleCount > 0) { anyAlive = true; break; }
            }
            yield return null;
        }
        foreach (var ps in regionParticles)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        _waitForParticlesCoroutine = null;

        // Debug.Log($"[Region: {gameObject.name}]: all particles died, clearing!");
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
            if (closest == worldPos)
            {
                return 1f;
            }
            else weight = Mathf.Max(weight, Mathf.Clamp01(1f - Vector3.Distance(worldPos, closest) / blendDistance));
        }

        return weight;
    }
}
