#ifndef TERRAIN_LAYER_BLEND_INCLUDED
#define TERRAIN_LAYER_BLEND_INCLUDED

// Blends 4 terrain layers using one RGBA control map. Call this once for
// layers 0-3 (using _Ctrl0) and once for layers 4-7 (using _Ctrl1), then
// combine the two results with CombineTerrainGroups_float below.
//
// Weight is returned un-normalized (raw sum of the 4 control channels) so
// the final combine step can normalize correctly across all 8 layers at once,
// even if this group alone doesn't sum to 1.
void BlendTerrainGroup_float(
    UnityTexture2D Control, UnitySamplerState ControlSampler, float2 ControlUV,
    UnityTexture2D Diff0, float4 ST0, UnityTexture2D Norm0,
    UnityTexture2D Diff1, float4 ST1, UnityTexture2D Norm1,
    UnityTexture2D Diff2, float4 ST2, UnityTexture2D Norm2,
    UnityTexture2D Diff3, float4 ST3, UnityTexture2D Norm3,
    UnitySamplerState LayerSampler, float2 BaseUV,
    out float3 Color, out float3 Normal, out float Weight)
{
    float4 ctrl = SAMPLE_TEXTURE2D(Control.tex, ControlSampler.samplerstate, ControlUV);
    Weight = ctrl.r + ctrl.g + ctrl.b + ctrl.a;

    float2 uv0 = BaseUV * ST0.xy + ST0.zw;
    float2 uv1 = BaseUV * ST1.xy + ST1.zw;
    float2 uv2 = BaseUV * ST2.xy + ST2.zw;
    float2 uv3 = BaseUV * ST3.xy + ST3.zw;

    float3 c0 = SAMPLE_TEXTURE2D(Diff0.tex, LayerSampler.samplerstate, uv0).rgb;
    float3 c1 = SAMPLE_TEXTURE2D(Diff1.tex, LayerSampler.samplerstate, uv1).rgb;
    float3 c2 = SAMPLE_TEXTURE2D(Diff2.tex, LayerSampler.samplerstate, uv2).rgb;
    float3 c3 = SAMPLE_TEXTURE2D(Diff3.tex, LayerSampler.samplerstate, uv3).rgb;

    Color = ctrl.r * c0 + ctrl.g * c1 + ctrl.b * c2 + ctrl.a * c3;

    float3 n0 = UnpackNormal(SAMPLE_TEXTURE2D(Norm0.tex, LayerSampler.samplerstate, uv0));
    float3 n1 = UnpackNormal(SAMPLE_TEXTURE2D(Norm1.tex, LayerSampler.samplerstate, uv1));
    float3 n2 = UnpackNormal(SAMPLE_TEXTURE2D(Norm2.tex, LayerSampler.samplerstate, uv2));
    float3 n3 = UnpackNormal(SAMPLE_TEXTURE2D(Norm3.tex, LayerSampler.samplerstate, uv3));

    Normal = ctrl.r * n0 + ctrl.g * n1 + ctrl.b * n2 + ctrl.a * n3;
}

// Combines the two 4-layer groups into the final 8-layer result, normalizing
// by the true combined weight and re-normalizing the blended normal vector.
void CombineTerrainGroups_float(
    float3 ColorA, float3 NormalA, float WeightA,
    float3 ColorB, float3 NormalB, float WeightB,
    out float3 Color, out float3 Normal)
{
    float total = max(WeightA + WeightB, 1e-5);
    Color = (ColorA + ColorB) / total;

    float3 n = NormalA + NormalB;
    Normal = (dot(n, n) > 1e-8) ? normalize(n) : float3(0, 0, 1);
}

#endif
