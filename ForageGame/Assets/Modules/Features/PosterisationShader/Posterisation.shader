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
            
            uniform float4 _Palette[64];
            uniform float _PaletteSize;
            sampler2D _MainTex;

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

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
            
            
            float4 toNearestInPalette(float4 colour)
            {
                float minDistance = 1e10;
                float4 nearestColour = float4(0, 0, 0, 1);
                for (int i = 0; i < _PaletteSize; i++)
                {
                    float4 paletteColour = _Palette[i];
                    float dist = distance(colour.rgb, paletteColour.rgb);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        nearestColour = paletteColour;
                    }
                }
                return nearestColour;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                // col = toNearestInPalette(col);
                return col;
            }
            ENDCG
        }
    }
}