Shader "Hidden/OutlineFromJFA"
{
    Properties
    {
        _JFATex ("Texture", 2D) = "white" {}
        _SilhouetteTex ("Silhouette Texture", 2D) = "black" {}
        _DepthTex ("Depth Texture", 2D) = "white" {}
        
        _OutlineWidth ("Outline Width", Float) = 1.0
        _OutlineColor ("Outline Color", Color) = (0.5, 0.8, 1, 1)
        
        _DoWobbleBool ("Do Wobble", Float) = 0.0
        _WobbleNoiseScale ("Wobble Noise Scale", Float) = 0.2
        _WobbleNoiseSpeed ("Wobble Noise Speed", Float) = 0.5
        _WobbleMaxIndentFactor ("Wobble Noise Max Indent Factor", Float) = 0.5
        _WobbleNoiseLacunarity ("Wobble Noise Lacunarity", Float) = 2.0
        _WobbleNoisePersistence ("Wobble Noise Persistence", Float) = 0.5
        _WobbleNoiseOctaves("Wobble Noise Octaves", Int) = 5
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite On ZTest LEqual

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "PerlinNoise3D.hlsl"
            
            TEXTURE2D(_JFATex);
            SAMPLER(sampler_JFATex);
            TEXTURE2D(_SilhouetteTex);
            SAMPLER(sampler_SilhouetteTex);
            TEXTURE2D(_DepthTex);
            SAMPLER(sampler_DepthTex);
    
            float4 _OutlineColor;
            float _OutlineWidth;
            float4 _ScreenParams; // x = width, y = height, z = 1/width, w = 1/height
            
            float4 _Time;
            float _DoWobbleBool;
            float _WobbleNoiseScale;
            float _WobbleNoiseSpeed;
            float _WobbleMaxIndentFactor;
            float _WobbleNoiseLacunarity;
            float _WobbleNoisePersistence;
            int _WobbleNoiseOctaves;
            
            struct Varyings { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert(uint id : SV_VertexID)
            {
                Varyings o;
                o.pos = GetFullScreenTriangleVertexPosition(id);
                o.uv  = GetFullScreenTriangleTexCoord(id);
                return o;
            }
            
            struct FragOutput
            {
                float4 color : SV_Target;
                float depth : SV_Depth;
            };
            
            // ----- UTILITY FUNCTIONS -----
            
            float SampleSeedDepth(float2 pixel_uv, float2 seed_uv)
            {
                float2 texelSize = _ScreenParams.zw; // zw = 1/width, 1/height
                float maxDepth = 0.0;
                
                for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    float2 offset = float2(x, y);
                    float2 pos = seed_uv * _ScreenParams.xy + offset;
                    float depth = LOAD_TEXTURE2D(_DepthTex, pos).r;
                    maxDepth = max(maxDepth, depth);
                }
                
                return maxDepth;
            }
            
            float DistanceToSeed(float2 pixel_uv, float2 seed_uv)
            {
                // Convert to pixel space to fix aspect ratio and get correct fwidth scale
                float2 pixelPos = pixel_uv * _ScreenParams.xy;
                float2 seedPixelPos = seed_uv * _ScreenParams.xy;
                float distanceToSeed = length(pixelPos - seedPixelPos);
                return distanceToSeed;
            }
            
            float2 GetSeedUV(float2 pixel_uv)
            {
                return SAMPLE_TEXTURE2D(_JFATex, sampler_JFATex, pixel_uv).rg;
            }

            float noise(float2 p, float t)
            {
                float value = 0.0;
                float amplitude = 1.0;
                float frequency = 1.0;
                
                for (int i = 0; i < _WobbleNoiseOctaves; i++)
                {
                    value += amplitude * perlin3(float3(p * frequency, t * frequency));
                    amplitude *= _WobbleNoisePersistence;
                    frequency *= _WobbleNoiseLacunarity;
                }
                
                
                float maxValue = (1.0 - pow(_WobbleNoisePersistence, _WobbleNoiseOctaves)) 
                     / (1.0 - _WobbleNoisePersistence);
                
                value = value / maxValue; // Normalize to [0, 1]
                
                return value;
            }
            
            // ----- OUTLINE EFFECTS -----
            
            float SilhouetteMask(float2 pixel_uv)
            {
                float silhouetteAlpha = SAMPLE_TEXTURE2D(_SilhouetteTex, sampler_SilhouetteTex, pixel_uv).r;
                return (1.0 - silhouetteAlpha);
            }
            
            float OutlineMask(float outlineWidth, float distanceToSeed)
            {
                return smoothstep(outlineWidth, outlineWidth - fwidth(distanceToSeed), distanceToSeed);
            }
            
            float ApplyWobbleToOutlineWidth(float outlineWidth, float2 seed_uv)
            {
                if (_DoWobbleBool < 0.5) return 1.0; // No wobble
                
                float noiseValue = noise(seed_uv / _WobbleNoiseScale, _Time.y * _WobbleNoiseSpeed);
                float wobble = lerp(0, _WobbleMaxIndentFactor * outlineWidth, noiseValue);
                
                float newWidth = outlineWidth - wobble;
                return newWidth;
            }
            
            FragOutput Frag(Varyings i)
            {
                FragOutput o;
                
                float2 pixel_uv = i.uv;
                float2 seed_uv = GetSeedUV(pixel_uv);
                
                float distanceToSeed = DistanceToSeed(pixel_uv, seed_uv);

                float outlineWidth = _OutlineWidth;
                
                if (_DoWobbleBool >= 0.5)
                {
                    outlineWidth = ApplyWobbleToOutlineWidth(outlineWidth, seed_uv);
                    float derivative = fwidth(distanceToSeed);
                    // outlineWidth = smoothstep(outlineWidth, outlineWidth - derivative, derivative) * outlineWidth;
                }
                
                float outlineAlpha = 1;
                outlineAlpha = outlineAlpha * OutlineMask(outlineWidth, distanceToSeed);
                outlineAlpha = outlineAlpha * SilhouetteMask(pixel_uv);
                
                if (outlineAlpha <= 0.0)
                {
                    discard;
                    // o.color = noise(pixel_uv / _WobbleNoiseScale, _Time.y * _WobbleNoiseSpeed);
                    // o.depth = 1.0;
                    // return o;
                }
                
                o.color = float4(_OutlineColor.rgb, _OutlineColor.a * outlineAlpha);                
                float seedDepth = SampleSeedDepth(i.uv, seed_uv);
                
                o.depth = seedDepth;
                
                // o.color = float4(seedDepth, seedDepth, seedDepth, 1);
                // o.color = float4(noise(pixel_uv * _WobbleNoiseScale + _Time.y * _WobbleNoiseSpeed).xxx, 1);
                return o;

            }
            ENDHLSL
        }
    }
}