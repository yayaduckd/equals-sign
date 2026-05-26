#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

void ReconstructWorldPos_float(
    float2 uv,
    out float3 worldPos)
{
    // Sample the raw depth buffer (0-1 range)
    float rawDepth = SampleSceneDepth(uv);

    // Reconstruct NDC
    float2 ndcXY = uv * 2.0 - 1.0;

    // In Unity, NDC Y is flipped
    #if UNITY_UV_STARTS_AT_TOP
        ndcXY.y = -ndcXY.y;
    #endif

    float4 clipPos = float4(ndcXY, rawDepth, 1.0);

    // Unproject to world space
    float4 worldPos4 = mul(UNITY_MATRIX_I_VP, clipPos);
    worldPos = worldPos4.xyz / worldPos4.w;
}

void ReconstructWorldPos_half(
    half2 uv,
    out half3 worldPos)
{
    float rawDepth = SampleSceneDepth(uv);
    float2 ndcXY = (float2)uv * 2.0 - 1.0;
    #if UNITY_UV_STARTS_AT_TOP
        ndcXY.y = -ndcXY.y;
    #endif
    float4 clipPos = float4(ndcXY, rawDepth, 1.0);
    float4 worldPos4 = mul(UNITY_MATRIX_I_VP, clipPos);
    worldPos = (half3)(worldPos4.xyz / worldPos4.w);
}