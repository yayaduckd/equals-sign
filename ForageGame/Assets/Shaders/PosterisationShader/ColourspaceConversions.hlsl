#ifndef COLOURSPACE_CONVERSIONS_INCLUDED
#define COLOURSPACE_CONVERSIONS_INCLUDED

//https://css-tricks.com/converting-color-spaces-in-javascript/

// Assuming RGB [0,1]
float3 RGBtoHSL(float3 rgb)
{
    float r = rgb.r, g = rgb.g, b = rgb.b;
    
    float cmin = min(min(r,g),b);
    float cmax = max(max(r,g),b);
    float delta = cmax - cmin;
    
    float h = 0;
    float s = 0;
    float l = 0;
    
    // Calculate hue
    // No difference
    if (delta == 0)
        h = 0;
    // Red is max
    else if (cmax == r)
        h = fmod((g - b) / delta, 6.0);
    // Green is max
    else if (cmax == g)
        h = (b - r) / delta + 2;
    // Blue is max
    else
        h = (r - g) / delta + 4;

    h = h * 60;
    
    // Make negative hues positive behind 360°
    if (h < 0)
        h += 360;
    
    // Calculate lightness
    l = (cmax + cmin) / 2;

    // Calculate saturation
    s = delta == 0 ? 0 : delta / (1 - abs(2 * l - 1));
    
    h = h / 360;
    
    return float3(h,s,l);
}

float3 HSLtoRGB(float3 hsl)
{
    float h = hsl.r, s = hsl.g, l = hsl.b;
    h = h * 360;
    
    float c = (1 - abs(2 * l - 1)) * s;
    float x = c * (1 - abs((h / 60) % 2 - 1));
    float m = l - c/2;
    
    float r,g,b = float3(0,0,0);
    
    if (0 <= h && h < 60) {
        r = c; g = x; b = 0;  
    } else if (60 <= h && h < 120) {
        r = x; g = c; b = 0;
    } else if (120 <= h && h < 180) {
        r = 0; g = c; b = x;
    } else if (180 <= h && h < 240) {
        r = 0; g = x; b = c;
    } else if (240 <= h && h < 300) {
        r = x; g = 0; b = c;
    } else if (300 <= h && h < 360) {
        r = c; g = 0; b = x;
    }
    
    r = r + m;
    g = g + m;
    b = b + m;
    
    return float3(r,g,b);
}

// Returns a scale factor representing the HDR energy of the pixel.
// Dividing the pixel by this gives a normalised SDR colour safe for
// colour space conversion. Multiply the output by this to restore HDR.
float ExtractHDRScale(float3 linearRGB)
{
    // Use luminance rather than max-channel so the scale is
    // perceptually meaningful and stable across colour spaces.
    return max(dot(linearRGB, float3(0.2126, 0.7152, 0.0722)), 1.0);
    // max(..., 1.0) means SDR pixels (luminance <= 1) are unaffected:
    // their scale is 1.0, so normalisation is a no-op.
}

float3 PosteriseRGB(float3 rgb, float3 binCounts)
{
    // RGB needs no normalisation — just quantise each channel.
    // Clamp to [0,1] so HDR pixels get clamped rather than corrupted.
    return floor(saturate(rgb) * binCounts) / binCounts;
}

float3 PosteriseHSL(float3 linearRGB, float3 binCounts)
{
    float scale = ExtractHDRScale(linearRGB);
    float3 sdr  = linearRGB / scale;         // guaranteed [0, 1]

    float3 hsl      = RGBtoHSL(sdr);
    float3 quantised = floor(hsl * binCounts) / binCounts;
    float3 rgb      = HSLtoRGB(quantised);

    return rgb * scale;                      // restore HDR energy
}

// Add future colour spaces here, following the same pattern:
// float3 PosteriseHSV(float3 linearRGB, float3 binCounts) { ... }
// float3 PosteriseLab(float3 linearRGB, float3 binCounts) { ... }

#endif