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
    // Bias into positive range before casting to uint
    // Large prime offsets avoid symmetry at zero
    uint x = (uint)(cell.x + 100003);
    uint y = (uint)(cell.y + 100003);
    uint z = (uint)(cell.z + 100003);

    uint h = _PerlinHash(_PerlinHash(_PerlinHash(x) ^ y) ^ z);
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


// ================================================================
//  SimplexNoise.hlsl
//  float simplexNoise3(float3 p) → roughly [-1, 1]
// ================================================================

static const int _SPerm[512] = {
    151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,
    140,36,103,30,69,142,8,99,37,240,21,10,23,190,6,148,
    247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,
    57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,
    74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,
    60,211,133,230,220,105,92,41,55,46,245,40,244,102,143,54,
    65,25,63,161,1,216,80,73,209,76,132,187,208,89,18,169,
    200,196,135,130,116,188,159,86,164,100,109,198,173,186,3,64,
    52,217,226,250,124,123,5,202,38,147,118,126,255,82,85,212,
    207,206,59,227,47,16,58,17,182,189,28,42,223,183,170,213,
    119,248,152,2,44,154,163,70,221,153,101,155,167,43,172,9,
    129,22,39,253,19,98,108,110,79,113,224,232,178,185,112,104,
    218,246,97,228,251,34,242,193,238,210,144,12,191,179,162,241,
    81,51,145,235,249,14,239,107,49,192,214,31,181,199,106,157,
    184,84,204,176,115,121,50,45,127,4,150,254,138,236,205,93,
    222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
    151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,
    140,36,103,30,69,142,8,99,37,240,21,10,23,190,6,148,
    247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,
    57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,
    74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,
    60,211,133,230,220,105,92,41,55,46,245,40,244,102,143,54,
    65,25,63,161,1,216,80,73,209,76,132,187,208,89,18,169,
    200,196,135,130,116,188,159,86,164,100,109,198,173,186,3,64,
    52,217,226,250,124,123,5,202,38,147,118,126,255,82,85,212,
    207,206,59,227,47,16,58,17,182,189,28,42,223,183,170,213,
    119,248,152,2,44,154,163,70,221,153,101,155,167,43,172,9,
    129,22,39,253,19,98,108,110,79,113,224,232,178,185,112,104,
    218,246,97,228,251,34,242,193,238,210,144,12,191,179,162,241,
    81,51,145,235,249,14,239,107,49,192,214,31,181,199,106,157,
    184,84,204,176,115,121,50,45,127,4,150,254,138,236,205,93,
    222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
};

// Gradient lookup from permutation hash
float3 _SplxGrad(int hash)
{
    // 12 edge midpoints of a cube — no axis-aligned vectors
    static const float3 _SplxGrad3[12] =
    {
        float3( 1, 1, 0), float3(-1, 1, 0), float3( 1,-1, 0), float3(-1,-1, 0),
        float3( 1, 0, 1), float3(-1, 0, 1), float3( 1, 0,-1), float3(-1, 0,-1),
        float3( 0, 1, 1), float3( 0,-1, 1), float3( 0, 1,-1), float3( 0,-1,-1),
    };
    return _SplxGrad3[hash % 12];
}

int _SPerm12(int i) { return _SPerm[i & 255] % 12; }

// ----------------------------------------------------------------
//  simplexNoise3 — 3-D simplex noise
//  Returns roughly [-1, 1]
// ----------------------------------------------------------------
float simplexNoise3(float3 p)
{
    // Skew input space into simplex cell
    static const float F3 = 1.0 / 3.0;
    static const float G3 = 1.0 / 6.0;

    float s  = (p.x + p.y + p.z) * F3;
    int3  i  = (int3) floor(p + s);
    float t  = (float)(i.x + i.y + i.z) * G3;

    // Unskew back to get cell origin
    float3 X0 = (float3)i - t;
    float3 x0 = p - X0;

    // Determine which simplex we're in (rank ordering of x0 components)
    int3 i1, i2;
    if (x0.x >= x0.y)
    {
        if      (x0.y >= x0.z) { i1 = int3(1,0,0); i2 = int3(1,1,0); }
        else if (x0.x >= x0.z) { i1 = int3(1,0,0); i2 = int3(1,0,1); }
        else                    { i1 = int3(0,0,1); i2 = int3(1,0,1); }
    }
    else
    {
        if      (x0.y <  x0.z) { i1 = int3(0,0,1); i2 = int3(0,1,1); }
        else if (x0.x <  x0.z) { i1 = int3(0,1,0); i2 = int3(0,1,1); }
        else                    { i1 = int3(0,1,0); i2 = int3(1,1,0); }
    }

    // Offsets for corners 1, 2, 3
    float3 x1 = x0 - (float3)i1 + G3;
    float3 x2 = x0 - (float3)i2 + 2.0 * G3;
    float3 x3 = x0 - 1.0        + 3.0 * G3;

    // Permutation hashes for all 4 corners
    int3 ii = i & 255;
    int g0 = _SPerm[ii.x +          _SPerm[ii.y +          _SPerm[ii.z         ]]] % 12;
    int g1 = _SPerm[ii.x + i1.x +   _SPerm[ii.y + i1.y +   _SPerm[ii.z + i1.z  ]]] % 12;
    int g2 = _SPerm[ii.x + i2.x +   _SPerm[ii.y + i2.y +   _SPerm[ii.z + i2.z  ]]] % 12;
    int g3 = _SPerm[ii.x + 1 +      _SPerm[ii.y + 1 +       _SPerm[ii.z + 1     ]]] % 12;

    // Compute contributions from each corner
    // Radial falloff: max(0, 0.6 - |x|^2)^4
    float4 t0 = max(0, float4(
        0.6 - dot(x0, x0),
        0.6 - dot(x1, x1),
        0.6 - dot(x2, x2),
        0.6 - dot(x3, x3)
    ));
    t0 = t0 * t0;
    t0 = t0 * t0;

    float4 contrib = t0 * float4(
        dot(_SplxGrad(g0), x0),
        dot(_SplxGrad(g1), x1),
        dot(_SplxGrad(g2), x2),
        dot(_SplxGrad(g3), x3)
    );

    // Scale to [-1, 1]
    return 32.0 * (contrib.x + contrib.y + contrib.z + contrib.w);
}


float FBM(float3 p,
int    octaves    = 6,
float  lacunarity = 2.0,
float  gain       = 0.5)
{
    float amplitude = 0.5;
    float frequency = 1.0;
    float value     = 0.0;

    [unroll(8)]                  // optional: unroll if octaves is a compile-time constant
    for (int i = 0; i < octaves; ++i)
    {
        value     += amplitude * simplexNoise3(p * frequency);
        frequency *= lacunarity;
        amplitude *= gain;
    }

    return value;
}