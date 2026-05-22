
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuantizedRGBPalette", menuName = "Palette/Quantized RGB Palette")]
class QuantizedRGBPalette : Palette
{
    public Vector3 ColourChannelBins = new Vector3(4, 4, 4);

    public override void UpdatePalette()
    {
        List<Color> colors = new List<Color>();
        for (int r = 0; r < ColourChannelBins.x; r++)
        {
            for (int g = 0; g < ColourChannelBins.y; g++)
            {
                for (int b = 0; b < ColourChannelBins.z; b++)
                {
                    colors.Add(new Color(r / ColourChannelBins.x, g / ColourChannelBins.y, b / ColourChannelBins.z));
                }
            }
        }
        colours = colors;
    }
}