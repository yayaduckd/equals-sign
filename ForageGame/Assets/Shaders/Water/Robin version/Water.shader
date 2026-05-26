Shader "URP/Water"
{
    Properties
    {
        _DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)
        _DepthMaxDistance("Depth Maximum Distance", Float) = 1
        
        _WavesThreshold("Waves Threshold", Float) = 0.5
        _WavesNoiseScale("Waves Noise Scale", Float) = 0.1
        _WavesSpeed("Waves Speed", Float) = 0.5
        
        _FoamDistance("Foam Distance", Float) = 0.4
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP includes only — no UnityCG.cginc
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Assets/Shaders/Water/Robin version/noise.hlsl"
            
            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;
            float _DepthMaxDistance;
            
            float _WavesThreshold;
            float _WavesNoiseScale;
            float _WavesSpeed;

            float _FoamDistance;
            
            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 screenPosition : TEXCOORD0;
                float3 worldPosition : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.screenPosition = ComputeScreenPos(OUT.positionHCS);
                OUT.worldPosition = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Get screen UV (perspective divide)
                float2 screenUV = IN.screenPosition.xy / IN.screenPosition.w;
                
                // Sample and linearize depth
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneDepthEye = LinearEyeDepth(rawDepth, _ZBufferParams);

                // Depth difference between scene and water surface
                float depthDifference = sceneDepthEye - IN.screenPosition.w; 

                // Remap to 0-1 over _DepthMaxDistance
                float depthFactor = saturate(depthDifference / _DepthMaxDistance);

                // Blend between shallow and deep colors
                float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, depthFactor);

                float2 worldNoiseUV = IN.worldPosition.xz * _WavesNoiseScale + float2(_Time.y * _WavesSpeed, _Time.y * _WavesSpeed * 0.7);
                float4 surfaceNoiseSample = FBM(float3(worldNoiseUV,0), 4, 2, 0.5);
                
                float foamDepthDifference01 = saturate(depthDifference / _FoamDistance);
                float surfaceNoiseCutoff = foamDepthDifference01 * _WavesThreshold;
                
                float4 surfaceNoise = surfaceNoiseSample > surfaceNoiseCutoff ? 1 : 0;
                
                // return surfaceNoiseSample;
                return waterColor + surfaceNoise;
            }
            ENDHLSL
        }
    }
}