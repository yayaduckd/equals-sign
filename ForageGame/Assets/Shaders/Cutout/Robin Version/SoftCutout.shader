Shader "UnityLibrary/URP/Effects/SoftSeethroughCircle_Lit"
{
    Properties
    {
        [MainTexture] _MainTex ("Base Texture", 2D) = "white" {}
        [MainColor]   _BaseColor ("Base Color Tint", Color) = (1, 1, 1, 1)
        
        _Radius ("Cutout Radius", Float) = 1.5
        _Softness ("Edge Softness", Float) = 0.5
        _PlayerPosition ("Player Position (World)", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        // "LightMode" = "UniversalForward" tells URP to pass lighting data to this pass
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Required URP framework files
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Material Textures & Properties
            Texture2D _MainTex;
            SamplerState sampler_MainTex; // Required for URP texture sampling

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
                float  _Radius;
                float  _Softness;
            CBUFFER_END

            // Global variable passed from C# script
            float3 _PlayerPosition;

            struct appdata 
            { 
                float4 vertex : POSITION; 
                float3 normal : NORMAL;   // Required for lighting!
                float2 uv     : TEXCOORD0; 
            };

            struct v2f     
            { 
                float4 pos      : SV_POSITION; 
                float3 worldPos : TEXCOORD0; 
                float3 normal   : TEXCOORD1; // Passed to frag shader
                float2 uv       : TEXTURE_UV;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                
                // Transform the local normal direction into world space
                o.normal = TransformObjectToWorldNormal(v.normal);
                
                // Scale and offset UV coordinates based on Material settings
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float distPointToLine(float3 p, float3 linePointA, float3 linePointB)
            {
                float3 lineDir = linePointB - linePointA;
                float3 pointToA = p - linePointA;

                float t = dot(pointToA, lineDir) / dot(lineDir, lineDir);
                t = clamp(t, 0.0, 1.0); 

                float3 projection = linePointA + t * lineDir;
                return length(p - projection);
            }

            float4 frag(v2f i) : SV_Target
            {
                // 1. Calculate Cutout Mask
                float dist = distPointToLine(i.worldPos, _PlayerPosition, _WorldSpaceCameraPos);
                float mask = smoothstep(_Radius - _Softness, _Radius, dist);

                // 2. Sample the Texture and apply color tint
                float4 texColor = _MainTex.Sample(sampler_MainTex, i.uv) * _BaseColor;

                // 3. Initialize Lighting
                // Normalize the world normal to avoid interpolation artifacts
                float3 worldNormal = normalize(i.normal); 
                
                // Get the primary directional light data from URP
                Light mainLight = GetMainLight(); 
                
                // Calculate basic NdotL (Diffuse light factor)
                // Half-Lambert formulation ensures dark sides aren't pitch black
                float ndotl = dot(worldNormal, mainLight.direction) * 0.5 + 0.5;
                
                // Combine light color and light attenuation (shadows/falloff)
                float3 diffuseLight = mainLight.color * (ndotl * mainLight.distanceAttenuation);
                
                // Add ambient/environment light so shadowed areas aren't flat
                float3 ambientLight = SampleSH(worldNormal);
                float3 finalLighting = diffuseLight + ambientLight;

                // 4. Apply Lighting to Texture Color
                float3 litColor = texColor.rgb * finalLighting;

                // 5. Output with cutout mask driving transparency
                return float4(litColor, texColor.a * mask);
            }
            ENDHLSL
        }
    }
}