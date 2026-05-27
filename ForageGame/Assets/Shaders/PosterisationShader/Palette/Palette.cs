
using System.Collections.Generic;
using UnityEngine;

public abstract class Palette : ScriptableObject
{
    public List<Color> colours;

    abstract public void UpdatePalette();

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdatePalette();
    }
#endif
}
