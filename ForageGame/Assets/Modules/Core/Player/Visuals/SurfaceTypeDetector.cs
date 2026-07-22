using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class SurfaceTypeDetector : MonoBehaviour
{
    private Terrain terrain;
    private TerrainData terrainData;
    private Vector3 terrainPosition;

    [SerializeField] private TerrainTypeLayerMap surfaceTypeLayerMap;
    private Dictionary<TerrainLayer, SurfaceType> surfaceTypeLayerDict; //dict version of the SO above


    private void Awake()
    {
        surfaceTypeLayerDict = new Dictionary<TerrainLayer, SurfaceType>();
        foreach (var e in surfaceTypeLayerMap.entries)
            if (e.layer != null)
                surfaceTypeLayerDict[e.layer] = e.type;
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

    public SurfaceType GetSurfaceType()
    {
        if (terrain == null || terrainData.terrainLayers.Length == 0)
        {
            // Debug disabled by Tim; WAY TO MANY ERRORS, PLEASE STOP!!!
            Debug.Log($"[SurfaceTypeDetector]: No terrain active or terrain has no layers, defaulting to Grass!");
            return SurfaceType.Grass;
        }
        int textureIndex;
        try
        {
            textureIndex = GetDominantTextureIndex(transform.position);
        }
        catch(Exception e)
        {
            Debug.Log($"[SurfaceTypeDetector]: caught error {e}. defaulting to grass terrain!");
            return SurfaceType.Grass;
        }
        //int textureIndex = GetDominantTextureIndex(transform.position);
        string textureName = terrainData.terrainLayers[textureIndex].diffuseTexture.name;
        Debug.Log($"Walking on: {textureName} of SurfaceType: {GetLayerSurfaceType(terrainData.terrainLayers[textureIndex])}");
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

    private SurfaceType GetLayerSurfaceType(TerrainLayer layer)
    {
        if (surfaceTypeLayerDict != null && surfaceTypeLayerDict.TryGetValue(layer, out var type))
            return type;
        Debug.Log("Terrain Type not present in TerrainMap, falling back to Grass!");
        return SurfaceType.Grass;
    }
}
