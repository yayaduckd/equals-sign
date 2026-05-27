Shader "Hidden/JFA_Init"
{
    Properties
    {
        _SilhouetteTex("Silhouette Texture", 2D) = "black" {}
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

            TEXTURE2D(_SilhouetteTex);
            SAMPLER(sampler_SilhouetteTex);

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
                float v = SAMPLE_TEXTURE2D(_SilhouetteTex, sampler_SilhouetteTex, i.uv).r;
                if (v > 0.0)
                    return float4(i.uv, 0, 0);
                else
                    return float4(-1, -1, 0, 0);
            }
            ENDHLSL
        }
    }
}
