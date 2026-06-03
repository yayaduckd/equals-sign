Shader "Hidden/Posterisation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            uniform float3 _ChannelBinCounts;

            #define ColourSpace_RGB    0
            #define ColourSpace_HSL    1
            
            uniform int _ColourSpace;
            
            sampler2D _MainTex;

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            
            #include "ColourspaceConversions.hlsl"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert(uint id : SV_VertexID)
            {
                v2f o;
                o.pos = GetFullScreenTriangleVertexPosition(id);
                o.uv  = GetFullScreenTriangleTexCoord(id);
                return o;
            }
            
            float3 toNearestInPalette(float3 colour)
            {
                if (_ColourSpace == ColourSpace_HSL)
                    return PosteriseHSL(colour, _ChannelBinCounts);
                
                return PosteriseRGB(colour, _ChannelBinCounts);
            }
            
            float4 frag(v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                col = float4(toNearestInPalette(col), col.a);
                // col = float4(1,1,1,1);
                return col;
            }
            ENDCG
        }
    }
}