#ifndef PERLIN_NOISE_HLSL
#define PERLIN_NOISE_HLSL
#endif

// ================================================================
//  PerlinNoise.hlsl
//  Provides:
//    float  perlin3(float3 p)                        → [-1,  1]
//    float  perlinAbs3(float3 p)                     → [ 0,  1]  (billowy)
//    float  fbmPerlin3(float3 p, int oct, float per) → [-1,  1]
//    float  fbmPerlinAbs3(...)                       → [ 0,  1]  (turbulence)
// ================================================================

// ----------------------------------------------------------------
//  Internal — hash & gradient
// ----------------------------------------------------------------

static const float3 _PerlinGrad3[16] =
{
    float3( 1, 1, 0), float3(-1, 1, 0), float3( 1,-1, 0), float3(-1,-1, 0),
    float3( 1, 0, 1), float3(-1, 0, 1), float3( 1, 0,-1), float3(-1, 0,-1),
    float3( 0, 1, 1), float3( 0,-1, 1), float3( 0, 1,-1), float3( 0,-1,-1),
    float3( 1, 1, 0), float3(-1, 1, 0), float3( 0,-1, 1), float3( 0, 1,-1), // wrap-pad to 16
};

uint _PerlinHash(uint n)
{
    n = (n << 13u) ^ n;
    n = n * (n * n * 15731u + 789221u) + 1376312589u;
    return n;
}

float3 _PerlinGradient(int3 cell)
{
    uint h = _PerlinHash(_PerlinHash(_PerlinHash((uint)cell.x)
                                             ^ (uint)cell.y)
                                             ^ (uint)cell.z);
    return _PerlinGrad3[h & 15u];
}

// Quintic fade — 6t^5 - 15t^4 + 10t^3  (zero first AND second derivative at 0,1)
float3 _PerlinFade(float3 t)
{
    return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
}

// ----------------------------------------------------------------
//  perlin3 — classic gradient noise
//  Returns [0, 1]
// ----------------------------------------------------------------

float perlin3(float3 p)
{
    int3   i  = (int3) floor(p);
    float3 f  = frac(p);
    float3 u  = _PerlinFade(f);

    // Gradients at the 8 cube corners
    float g000 = dot(_PerlinGradient(i + int3(0,0,0)), f - float3(0,0,0));
    float g100 = dot(_PerlinGradient(i + int3(1,0,0)), f - float3(1,0,0));
    float g010 = dot(_PerlinGradient(i + int3(0,1,0)), f - float3(0,1,0));
    float g110 = dot(_PerlinGradient(i + int3(1,1,0)), f - float3(1,1,0));
    float g001 = dot(_PerlinGradient(i + int3(0,0,1)), f - float3(0,0,1));
    float g101 = dot(_PerlinGradient(i + int3(1,0,1)), f - float3(1,0,1));
    float g011 = dot(_PerlinGradient(i + int3(0,1,1)), f - float3(0,1,1));
    float g111 = dot(_PerlinGradient(i + int3(1,1,1)), f - float3(1,1,1));

    // Trilinear interpolation with fade weights
    return 0.5 * (1 + lerp(
        lerp(lerp(g000, g100, u.x), lerp(g010, g110, u.x), u.y),
        lerp(lerp(g001, g101, u.x), lerp(g011, g111, u.x), u.y),
        u.z));
}