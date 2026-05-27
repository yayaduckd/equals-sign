
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColourListPalette", menuName = "Palette/Colour List Palette")]
class ColourListPalette : Palette
{
    private List<Color> _colors = new List<Color>();
    
    public override void UpdatePalette()
    {
        return;
    }
}
