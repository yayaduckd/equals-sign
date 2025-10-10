using UnityEngine;
using TMPro;

public class ModifySpriteAsset : MonoBehaviour
{
    [SerializeField]TMP_SpriteAsset spriteAsset;

    [SerializeField] float scale = 1f;
    [SerializeField] float bearingX = 0f;
    [SerializeField] float bearingY = 0f;
    [SerializeField] float centeringConstant = 15;

    [ContextMenu("Center")]
    public void Center()
    {
        foreach(var sprite in spriteAsset.spriteGlyphTable)
        {
            var metrics = sprite.metrics;
            metrics.horizontalBearingY = metrics.height / 2f + centeringConstant;
            sprite.metrics = metrics;
        }
    }

    [ContextMenu("Set Scale")]
    public void SetScale()
    {
        foreach (var sprite in spriteAsset.spriteGlyphTable)
        {
            sprite.scale = scale;
        }
    }

    [ContextMenu("Set Bearing")]
    public void SetBearingX()
    {
        foreach (var sprite in spriteAsset.spriteGlyphTable)
        {
            var metrics = sprite.metrics;
            metrics.horizontalBearingX = bearingX;
            sprite.metrics = metrics;
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(spriteAsset);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }

    [ContextMenu("Set Bearing Y")]
    public void SetBearingY()
    {
        foreach (var sprite in spriteAsset.spriteGlyphTable)
        {
            var metrics = sprite.metrics;
            metrics.horizontalBearingY = bearingY;
            sprite.metrics = metrics;
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(spriteAsset);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }

}
