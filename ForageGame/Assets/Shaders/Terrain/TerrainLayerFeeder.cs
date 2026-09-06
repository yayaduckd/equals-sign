using UnityEngine;

// Attach this to the same GameObject as your Terrain component.
// It reads the terrain's alphamap (control) textures and TerrainLayer data,
// then pushes them onto a material under custom property names so a single
// Shader Graph pass can blend all 8 layers at once (bypassing URP's
// automatic 4-layers-per-draw-call splitting, which is keyed off Unity's
// reserved property names like _Control/_Splat0-3).
[ExecuteAlways]
[RequireComponent(typeof(Terrain))]
public class TerrainLayerFeeder : MonoBehaviour
{
    [Tooltip("Material using the custom 8-layer Shader Graph. Leave empty to use the Terrain's current Material.")]
    public Material targetMaterial;

    [Tooltip("Also feed normal map textures (_LayerNormal0.._LayerNormal7).")]
    public bool includeNormalMaps = true;

    Terrain _terrain;
    static Texture2D _flatNormal;

    // A 1x1 flat tangent-space normal (0,0,1) used when a layer has no normal map assigned.
    static Texture2D FlatNormal
    {
        get
        {
            if (_flatNormal == null)
            {
                _flatNormal = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                _flatNormal.SetPixel(0, 0, new Color(0.5f, 0.5f, 1f, 1f));
                _flatNormal.Apply();
            }
            return _flatNormal;
        }
    }

    void OnEnable()
    {
        _terrain = GetComponent<Terrain>();
        TerrainCallbacks.textureChanged += OnTerrainTextureChanged;
        Refresh();
    }

    void OnDisable()
    {
        TerrainCallbacks.textureChanged -= OnTerrainTextureChanged;
    }

    void OnTerrainTextureChanged(Terrain t, string textureName, RectInt tile, bool synched)
    {
        if (t == _terrain) Refresh();
    }

    // Call this manually after changing layers via script/tooling if you need
    // an immediate update outside the normal painting callback.
    [ContextMenu("Refresh Now")]
    public void Refresh()
    {
        if (_terrain == null) _terrain = GetComponent<Terrain>();
        if (_terrain == null) return;

        TerrainData terrainData = _terrain.terrainData;
        if (terrainData == null) return;

        Material mat = targetMaterial != null ? targetMaterial : _terrain.materialTemplate;
        if (mat == null) return;

        // --- Control (alpha) maps ---
        // alphamapTextures[0] holds RGBA weights for layers 0-3.
        // alphamapTextures[1] (if it exists) holds RGBA weights for layers 4-7.
        Texture2D[] alphamaps = terrainData.alphamapTextures;
        mat.SetTexture("_Ctrl0", alphamaps.Length > 0 ? alphamaps[0] : Texture2D.blackTexture);
        mat.SetTexture("_Ctrl1", alphamaps.Length > 1 ? alphamaps[1] : Texture2D.blackTexture);

        // --- Per-layer diffuse / normal / tiling ---
        TerrainLayer[] layers = terrainData.terrainLayers;
        for (int i = 0; i < 8; i++)
        {
            string diffuseProp = "_Layer" + i;
            string normalProp = "_LayerNormal" + i;
            string stProp = "_Layer" + i + "_ST";

            if (i < layers.Length && layers[i] != null)
            {
                TerrainLayer layer = layers[i];

                mat.SetTexture(diffuseProp,
                    layer.diffuseTexture != null ? layer.diffuseTexture : Texture2D.whiteTexture);

                if (includeNormalMaps)
                    mat.SetTexture(normalProp,
                        layer.normalMapTexture != null ? layer.normalMapTexture : FlatNormal);

                // Same tiling math Unity's own terrain shaders use:
                // scale = terrain world size / layer tile size, offset = tile offset / tile size.
                Vector2 tileSize = layer.tileSize;
                Vector2 tileOffset = layer.tileOffset;

                float sx = tileSize.x > 0.0001f ? terrainData.size.x / tileSize.x : 1f;
                float sy = tileSize.y > 0.0001f ? terrainData.size.z / tileSize.y : 1f;
                float ox = tileSize.x > 0.0001f ? tileOffset.x / tileSize.x : 0f;
                float oy = tileSize.y > 0.0001f ? tileOffset.y / tileSize.y : 0f;

                mat.SetVector(stProp, new Vector4(sx, sy, ox, oy));
            }
            else
            {
                // Empty slot (fewer than 8 layers painted): its control-map weight
                // will be ~0 anyway, but feed harmless fallbacks just in case.
                mat.SetTexture(diffuseProp, Texture2D.blackTexture);
                if (includeNormalMaps) mat.SetTexture(normalProp, FlatNormal);
                mat.SetVector(stProp, new Vector4(1, 1, 0, 0));
            }
        }
    }

    void Update()
    {
        // Keep syncing while editing in-Editor (e.g. if you swap layers via script/tooling).
        // Cheap (just SetTexture/SetVector calls), so fine to run every editor frame.
        // Not needed in Play mode / builds; painting callback handles normal use.
        if (!Application.isPlaying) Refresh();
    }
}
