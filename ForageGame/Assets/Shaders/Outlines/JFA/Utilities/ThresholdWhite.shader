Shader "Hidden/ThresholdWhite"
{
    Properties {
        _Threshold("Threshold", Float) = 0.0001
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

            TEXTURE2D(_SourceTex);
            SAMPLER(sampler_SourceTex);
            float _Threshold;

            struct Varyings { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert(uint id : SV_VertexID)
            {
                Varyings o;
                float2 uv = float2((id == 2) ? 2.0 : 0.0, (id == 1) ? 2.0 : 0.0);
                o.uv  = uv;
                o.pos = float4(uv * float2(2, -2) + float2(-1, 1), 0, 1);
                return o;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, i.uv);
                float sum = color.r + color.g + color.b;
                float bw = sum > _Threshold ? 1.0 : 0.0;
                return float4(bw, bw, bw, 1.0);
            }
            ENDHLSL
        }
    }
}