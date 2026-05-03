Shader "Hidden/Silhouette"
{
    Properties
    {
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "SilhouetteID"

            ZWrite On
            ZTest Always
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = OUT.positionHCS.xy * 0.5 + 0.5;
                return OUT;
            }

            struct FragOutput
            {
                float4 color : SV_Target0;
                float depth : SV_Target1;
            };
            
            FragOutput frag(Varyings i)
            {
                FragOutput o;
                o.color = half4(1, 0, 0, 1);
                o.depth = i.positionHCS.z;
                return o;
            }
            ENDHLSL
        }
    }
}