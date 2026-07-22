using UnityEngine;

/// <summary>
/// Updates the TerrainTexture detector so it uses the correct terrain for the current island
/// </summary>
[RequireComponent(typeof(MeshCollider))]
public class IslandTerrainZone : MonoBehaviour
{
    [SerializeField] private Terrain islandTerrain;

    //The collider should only be detecting the player's layer anyways, but this is just to be sure
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<SurfaceTypeDetector>(out var detector))
            detector.SetActiveTerrain(islandTerrain);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<SurfaceTypeDetector>(out var detector))
            detector.ClearActiveTerrain(islandTerrain);
    }
}
