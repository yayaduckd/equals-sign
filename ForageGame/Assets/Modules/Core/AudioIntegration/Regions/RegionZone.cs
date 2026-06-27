using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One Region in the game world, for audio and weather purposes.
/// uses meshcolliders / promesh builder
/// IMPORTANT: these must be convex
/// 
/// ~Lars
/// </summary>
//[RequireComponent(typeof(MeshCollider))]
public class RegionZone : MonoBehaviour
{
    public Region region;
    public float blendDistance = 20f;

    private MeshCollider[] _colliders;

    private void Awake()
    {
        _colliders = GetComponentsInChildren<MeshCollider>();

        if(_colliders.Length == 0) Debug.LogError($"[RegionZone: {gameObject.name}]: Has no colliders attached in children");
        foreach (MeshCollider col in _colliders)
        {
            col.GetComponent<MeshRenderer>().enabled = false;
        }
        //GetComponent<MeshRenderer>().enabled = false; //turn off in play mode
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
    public (Region region, float weight) Sample(Vector3 worldPos)
    {
        Vector3 closest = worldPos;
        
        foreach (var col in _colliders)
        {
            closest = col.ClosestPoint(worldPos);
            //early exit if we are fully inside any of the colliders
            if(closest == worldPos)
            {
                return (region, 1f);
            }
        }

        return (region, Mathf.Clamp01(1f - Vector3.Distance(worldPos, closest) / blendDistance));
    }
}
