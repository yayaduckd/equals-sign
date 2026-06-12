using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class TerrainTextureDetector : MonoBehaviour
{
    private Terrain terrain;
    private TerrainData terrainData;
    private Vector3 terrainPosition;

    [SerializeField] private TerrainTypeLayerMap terrainTypeLayerMap;
    private Dictionary<TerrainLayer, TerrainType> terrainTypeLayerDict; //dict version of the SO above


    private void Awake()
    {
        terrainTypeLayerDict = new Dictionary<TerrainLayer, TerrainType>();
        foreach (var e in terrainTypeLayerMap.entries)
            if (e.layer != null)
                terrainTypeLayerDict[e.layer] = e.type;
    }

    public void SetActiveTerrain(Terrain t)
    {
        terrain = t;
        terrainData = t.terrainData;
        terrainPosition = t.transform.position;
        Debug.Log($"[TerrainTextureDetector]: Player entered island terrain zone: {t.terrainData}");
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
        Debug.Log($"[TerrainTextureDetector]: Player left island terrain zone: {t.terrainData}");
    }

    public TerrainType GetTerrainType()
    {
        if (terrain == null || terrainData.terrainLayers.Length == 0)
        {
            // Debug disabled by Tim; WAY TO MANY ERRORS, PLEASE STOP!!!
            Debug.Log($"[TerrainTextureDetector]: No terrain active or terrain has no layers, defaulting to Grass!");
            return TerrainType.Grass;
        }
        int textureIndex;
        try
        {
            textureIndex = GetDominantTextureIndex(transform.position);
        }
        catch(Exception e)
        {
            Debug.Log($"[TerrainTextureDetector]: caught error {e}. defaulting to grass terrain!");
            return TerrainType.Grass;
        }
        //int textureIndex = GetDominantTextureIndex(transform.position);
        string textureName = terrainData.terrainLayers[textureIndex].diffuseTexture.name;
        Debug.Log($"Walking on: {textureName} of TerrainType: {GetLayerTerrainType(terrainData.terrainLayers[textureIndex])}");
        return GetLayerTerrainType(terrainData.terrainLayers[textureIndex]);
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

    private TerrainType GetLayerTerrainType(TerrainLayer layer)
    {
        if (terrainTypeLayerDict != null && terrainTypeLayerDict.TryGetValue(layer, out var type))
            return type;
        Debug.Log("Terrain Type not present in TerrainMap, falling back to Grass!");
        return TerrainType.Grass;
    }
}
