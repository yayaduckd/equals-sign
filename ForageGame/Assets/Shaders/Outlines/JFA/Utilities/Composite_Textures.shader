Shader "Hidden/CompositeTextures"
{
    Properties
    {
        _BackgroundTex("Background Texture", 2D) = "white" {}
        _OverlayTex("Foreground Texture", 2D) = "white" {}
    }
    SubShader
    {
        ZWrite Off ZTest Always Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            TEXTURE2D(_BackgroundTex);
            SAMPLER(sampler_BackgroundTex);
            TEXTURE2D(_OverlayTex);
            SAMPLER(sampler_OverlayTex);

            struct Varyings { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert(uint id : SV_VertexID)
            {
                Varyings o;
                o.pos = GetFullScreenTriangleVertexPosition(id);
                o.uv  = GetFullScreenTriangleTexCoord(id);
                return o;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float4 bgColor = SAMPLE_TEXTURE2D(_BackgroundTex, sampler_BackgroundTex, i.uv);
                float4 overlayColor = SAMPLE_TEXTURE2D(_OverlayTex, sampler_OverlayTex, i.uv);
                // Alpha blending: outColor = overlayColor * overlayAlpha + bgColor * (1 - overlayAlpha)
                float overlayAlpha = overlayColor.a;
                float4 outColor = overlayColor * overlayAlpha + bgColor * (1.0 - overlayAlpha);
                return outColor;
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}