using UnityEngine;

/// <summary>
/// One Region in the game world, for audio and weather purposes.
/// uses meshcolliders / promesh builder
/// IMPORTANT: these must be convex
/// 
/// ~Lars
/// </summary>
[RequireComponent(typeof(MeshCollider))]
public class RegionZone : MonoBehaviour
{
    public Region region;
    public float blendDistance = 20f;

    private MeshCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<MeshCollider>();
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
        Vector3 closest = _collider.ClosestPoint(worldPos);
        bool isInside = closest == worldPos;

        float weight = isInside
            ? 1f
            : Mathf.Clamp01(1f - Vector3.Distance(worldPos, closest) / blendDistance);

        return (region, weight);
    }
}
