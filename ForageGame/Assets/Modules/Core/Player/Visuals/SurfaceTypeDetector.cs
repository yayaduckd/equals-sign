using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class SurfaceTypeDetector : MonoBehaviour
{
    [Header("Raycast settings")]
    [SerializeField] private LayerMask detectionMask; // Terrain + Obstacle + Water layers
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private float rayLength = 1.5f;
    [SerializeField] private float rayStartHeight = 0.5f;

    private RaycastHit[] hitBuffer = new RaycastHit[8];
    private Terrain terrain;
    private TerrainData terrainData;
    private Vector3 terrainPosition;

    [Header("Current terrain layer map")]
    [SerializeField] private TerrainTypeLayerMap surfaceTypeLayerMap;
    private Dictionary<TerrainLayer, SurfaceTypeEntry> surfaceTypeLayerDict; //dict version of the SO above

    [SerializeField] private SurfaceTypeEntry defaultSurfaceTypeEntry = new SurfaceTypeEntry {type = SurfaceType.Grass, dustColor = Color.clear};


    private void Awake()
    {
        surfaceTypeLayerDict = new Dictionary<TerrainLayer, SurfaceTypeEntry>();
        foreach (var e in surfaceTypeLayerMap.entries)
            if (e.layer != null)
                surfaceTypeLayerDict[e.layer] = e.surface;
    }

    public void SetActiveTerrain(Terrain t)
    {
        terrain = t;
        terrainData = t.terrainData;
        terrainPosition = t.transform.position;
        Debug.Log($"[SurfaceTypeDetector]: Player entered island terrain zone: {t.terrainData}");
    }

    ///IMPORTANT: in between islands, this means terrain will be null
    ///I don't want to have the awkwardness of defaulting to the pond floor,
    /// Since it doesn't actually have terrain to walk on (it shouldn't)
    /// I catch the null reference anyway, just know that it does so
    /// ~Lars
    public void ClearActiveTerrain(Terrain t)
    {
        terrain = null;
        terrainData = null;
        //terrainPosition = null; //does not need to be cleared
        Debug.Log($"[SurfaceTypeDetector]: Player left island terrain zone: {t.terrainData}");
    }

    public SurfaceTypeEntry GetSurfaceType()
    {
        Vector3 origin = transform.position + Vector3.up * rayStartHeight;
        int hitCount = Physics.RaycastNonAlloc(
            origin, Vector3.down, hitBuffer, rayLength + rayStartHeight, detectionMask);

        if (hitCount == 0) 
        {
            Debug.LogError($"[SurfaceTypeDetector]: No raycast hits detected under the player, defaulting to Grass!)");
            return defaultSurfaceTypeEntry;
        }

        // Sort hits by distance, closest first
        Array.Sort(hitBuffer, 0, hitCount, 
            Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance)));

        RaycastHit closest = hitBuffer[0];
        int hitLayer = closest.collider.gameObject.layer;

        if (IsInLayerMask(hitLayer, obstacleLayer))
        {
            // Debug.Log($"[SurfaceTypeDetector]: Detected an obstacle...");
            var tag = closest.collider.GetComponentInParent<ObstacleSurfaceType>();
            if (tag)
            {
                // Debug.Log($"[SurfaceTypeDetector]: ...With SurfaceType: {tag.SurfaceType}");
                return new SurfaceTypeEntry{type = tag.SurfaceType, dustColor = Color.clear}; //TODO: obstacle dust color?
            }

            // Debug.Log($"[SurfaceTypeDetector]: ...but no SurfaceType tag found, defaulting to Wood!");
            return new SurfaceTypeEntry{type = SurfaceType.Wood, dustColor = Color.clear};
        }
        else if (IsInLayerMask(hitLayer, waterLayer))
        {
            // Debug.Log($"[SurfaceTypeDetector]: Detected a water layer");
            return new SurfaceTypeEntry{type = SurfaceType.Water, dustColor = Color.clear}; //dust is disabled for water anyways
        }
        else if (IsInLayerMask(hitLayer, terrainLayer))
        {
            // Debug.Log($"[SurfaceTypeDetector]: Detected the terrain layer, checking texture type...");
            return GetTerrainSurfaceType();
        }
        else
        {
            Debug.LogError($"[SurfaceTypeDetector]: No surface detected under the player, defaulting to Grass!)");
            return defaultSurfaceTypeEntry;
        }
    }

    //awesome utility function Unity doesn't provide for some reason
    private bool IsInLayerMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;


    #region Terrain Texture Detection


    private SurfaceTypeEntry GetTerrainSurfaceType()
    {
        if (terrain == null || terrainData.terrainLayers.Length == 0)
        {
            // Debug disabled by Tim; WAY TO MANY ERRORS, PLEASE STOP!!!
            Debug.Log($"[SurfaceTypeDetector]: No terrain active or terrain has no layers, defaulting to Grass!");
            return defaultSurfaceTypeEntry;
        }
        int textureIndex;
        try
        {
            textureIndex = GetDominantTextureIndex(transform.position);
        }
        catch(Exception e)
        {
            Debug.Log($"[SurfaceTypeDetector]: caught error {e}. defaulting to grass terrain!");
            return defaultSurfaceTypeEntry;
        }
        //int textureIndex = GetDominantTextureIndex(transform.position);
        string textureName = terrainData.terrainLayers[textureIndex].diffuseTexture.name;
        // Debug.Log($"Walking on: {textureName} of SurfaceType: {GetLayerSurfaceType(terrainData.terrainLayers[textureIndex])}");
        return GetLayerSurfaceType(terrainData.terrainLayers[textureIndex]);
    }
    private int GetDominantTextureIndex(Vector3 worldPos)
    {
        float[] mix = GetTextureMix(worldPos);
        int dominantIndex = 0;
        float maxWeight = 0f;

        for (int i = 0; i < mix.Length; i++)
        {
            if (mix[i] > maxWeight)
            {
                maxWeight = mix[i];
                dominantIndex = i;
            }
        }

        return dominantIndex;
    }

    //Special thanks to mr. Claude for this one
    private float[] GetTextureMix(Vector3 worldPos)
    {
        //Convert world position to terrain-local coordinates (0..1 range)
        float normX = (worldPos.x - terrainPosition.x) / terrainData.size.x;
        float normZ = (worldPos.z - terrainPosition.z) / terrainData.size.z;

        //Convert to alphamap coordinates
        int mapX = Mathf.RoundToInt(normX * (terrainData.alphamapWidth - 1));
        int mapZ = Mathf.RoundToInt(normZ * (terrainData.alphamapHeight - 1));

        //alphamaps[z, x, layerIndex] — note the z/x order!
        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        float[] mix = new float[splatmapData.GetUpperBound(2) + 1];
        for (int i = 0; i < mix.Length; i++)
            mix[i] = splatmapData[0, 0, i];

        return mix;
    }

    private SurfaceTypeEntry GetLayerSurfaceType(TerrainLayer layer)
    {
        if (surfaceTypeLayerDict != null && surfaceTypeLayerDict.TryGetValue(layer, out var entry))
            return entry;
        Debug.Log("Terrain Type not present in TerrainMap, falling back to Grass with no dust!");
        return defaultSurfaceTypeEntry;
    }

    #endregion
}
