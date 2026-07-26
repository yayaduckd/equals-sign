using UnityEngine;
using System.Collections.Generic;


public enum SurfaceType { Sand, Grass, Gravel, Rock, Water, Wood } //DO NOT EDIT, THIS CORRESPONDS WITH FMOD STUFF ~Lars

[System.Serializable]
public struct SurfaceTypeEntry
{
    public SurfaceType type;
    [Tooltip("Color of dust particle for this surface, leave alpha 0 for no dust")]
    public Color dustColor;
}

[System.Serializable]
public struct TerrainMapEntry
{
    public TerrainLayer layer;
    public SurfaceTypeEntry surface;
}

/// <summary>
/// This is just so we can have a serializable dictionary
/// ~Lars
/// </summary>
[CreateAssetMenu(fileName = "TerrainTypeLayerMap", menuName = "AudioStuff/TerrainTypeLayerMap")]
public class TerrainTypeLayerMap : ScriptableObject
{
    public TerrainMapEntry[] entries;
}
