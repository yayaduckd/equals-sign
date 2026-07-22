using UnityEngine;
using System.Collections.Generic;


public enum SurfaceType { Sand, Grass, Dirt, Rock, Snow, Wood } //DO NOT EDIT, THIS CORRESPONDS WITH FMOD STUFF ~Lars

[System.Serializable]
public struct TerrainMapEntry
{
    public TerrainLayer layer;
    public SurfaceType type;
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
