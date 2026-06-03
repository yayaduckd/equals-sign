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

float3 PosteriseRGB(float3 rgb, float3 binCounts)
{
    // RGB needs no normalisation — just quantise each channel.
    // Clamp to [0,1] so HDR pixels get clamped rather than corrupted.
    return floor(saturate(rgb) * binCounts) / binCounts;
}

float3 PosteriseHSL(float3 linearRGB, float3 binCounts)
{
    // Convert to HSL using a normalised colour.
    float maxChannel = max(max(linearRGB.r, linearRGB.g), linearRGB.b);
    float3 chromaRGB = linearRGB / max(maxChannel, 1e-5);

    float3 hsl = RGBtoHSL(chromaRGB);

    // Quantise hue and saturation normally.
    hsl.r = floor(hsl.r * binCounts.x) / binCounts.x;
    hsl.g = floor(hsl.g * binCounts.y) / binCounts.y;

    // Quantise brightness in log space.
    float lum = dot(linearRGB, float3(0.2126, 0.7152, 0.0722));

    float logLum = log2(1.0 + lum);

    logLum =
        floor(logLum * binCounts.z)
        / binCounts.z;

    float quantLum = exp2(logLum) - 1.0;

    // Reconstruct colour.
    float3 rgb = HSLtoRGB(hsl);

    float rgbLum = max(
        dot(rgb, float3(0.2126, 0.7152, 0.0722)),
        1e-5
    );

    return rgb * (quantLum / rgbLum);
}

// Add future colour spaces here, following the same pattern:
// float3 PosteriseHSV(float3 linearRGB, float3 binCounts) { ... }
// float3 PosteriseLab(float3 linearRGB, float3 binCounts) { ... }

#endif