#ifndef CUTOUT_MATH_INCLUDED
#define CUTOUT_MATH_INCLUDED

void ApplyCutoutMask_float(
    float3 PixelWorldPos,
    float3 PlayerPos,
    float3 CameraPos,
    float Radius,
    float Softness,
    out float AlphaMask)
{
    float3 lineDir = CameraPos - PlayerPos;
    float3 pointToA = PixelWorldPos - PlayerPos;

    float t = dot(pointToA, lineDir) / dot(lineDir, lineDir);
    t = clamp(t, 0.0, 1.0);

    float3 projection = PlayerPos + t * lineDir;
    float dist = length(PixelWorldPos - projection);

    // Outputs 0 inside the circle, 1 outside
    AlphaMask = smoothstep(Radius - Softness, Radius, dist);
}

#endif