using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMProOutline : MonoBehaviour
{
    TextMeshProUGUI _textView;
    [SerializeField] float outlineWidth = 1.0f;
    [SerializeField] Color outlineColor = Color.white;
    private void Awake()
    {
        SetOutline();
    }

    [ContextMenu("SetOutline")]
    private void SetOutline()
    {
        if(_textView == null) _textView = GetComponent<TextMeshProUGUI>();

        // 1. Get a copy of the material (Creates a new instance for this object)
        // If you use 'fontSharedMaterial', you will change it for EVERY text using this font!
        Material mat = _textView.fontMaterial;

        // 2. Enable the Outline keyword (Required for some shaders to switch modes)
        mat.EnableKeyword("OUTLINE_ON");

        // 3. Set the Shader properties
        // Width is usually between 0.0 and 1.0
        mat.SetFloat("_OutlineWidth", outlineWidth); 
        mat.SetColor("_OutlineColor", outlineColor);

        // 4. IMPORTANT: Force TMP to update its mesh boundaries
        // Without this, the outline might get "clipped" or cut off at the edges
        _textView.UpdateMeshPadding();
    }

#if  UNITY_EDITOR
    private void OnValidate()
    {
        SetOutline();
    }
#endif
}
