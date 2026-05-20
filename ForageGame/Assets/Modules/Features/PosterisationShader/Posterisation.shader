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
            #define ColourSpace_CIELAB 2
            
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
            
            float3 convertToColourSpace(float3 color)
            {
                if (_ColourSpace == ColourSpace_RGB)
                    return color;
                else if (_ColourSpace == ColourSpace_HSL)
                    return RGBtoHSL(color);
                // else if (_ColourSpace == ColourSpace_CIELAB)
                //     return RGBtoCIELAB(color);
                else
                    return color; // Default to RGB if an invalid colour space is provided
            }
            
            float3 convertBackToRGB(float3 color)
            {
                if (_ColourSpace == ColourSpace_RGB)
                    return color;
                else if (_ColourSpace == ColourSpace_HSL)
                    return HSLtoRGB(color);
                // else if (_ColourSpace == ColourSpace_CIELAB)
                //     return CIELABtoRGB(color);
                else
                    return color; // Default to RGB if an invalid colour space is provided
            }
            
            float3 toNearestInPalette(float3 colour)
            {
                float3 colourInSpace = convertToColourSpace(colour);
                
                float3 rounded = floor(colourInSpace * _ChannelBinCounts) / _ChannelBinCounts;
                
                float3 output = convertBackToRGB(rounded);
                
                return output;
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