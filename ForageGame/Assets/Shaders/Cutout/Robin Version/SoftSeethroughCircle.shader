Shader "UnityLibrary/URP/Effects/SoftSeethroughCircle"
{

    Properties
    {
        _Radius ("Radius", Float) = 0.4
        _Softness ("Edge Softness", Float) = 0.05
        _PlayerPosition ("Player Position (World)", Vector) = (0, 0, 0)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

        // Multiply only the destination alpha
        Blend SrcAlpha OneMinusSrcAlpha

        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float  _Radius;
            float  _Softness;

            float3 _PlayerPosition;

            struct appdata 
            { 
                float4 vertex : POSITION; 
                float2 uv : TEXCOORD0; 
            };

            struct v2f     
            { 
                float4 pos : SV_POSITION; 
                float3 worldPos : TEXCOORD0; // Passed to frag shader
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                // Calculate world position here, in the vertex shader
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                return o;
            }

            float distPointToLine(float3 pointPos, float3 linePointA, float3 linePointB)
            {
                float3 lineDir = linePointB - linePointA;
				float3 pointToA = pointPos - linePointA;

				// project pointToA onto lineDir
				float t = dot(pointToA, lineDir) / dot(lineDir, lineDir);
                t = clamp(t, 0.0, 1.0); // clamp to segment

				float3 projection = linePointA + t * lineDir;

				// distance from point to projection
				return length(pointPos - projection);
             }


            float4 frag(v2f i) : SV_Target
            {
                // we need to get the distance from the pixel's world space position to the line from the player to the camera
                // https://mathworld.wolfram.com/Point-LineDistance3-Dimensional.html
                float3 pixelWorldPos = i.worldPos;

                // project the pixel position onto the player-camera line
                float dist = distPointToLine(pixelWorldPos, _PlayerPosition, _WorldSpaceCameraPos);

                // 0 inside circle, 1 outside
                float mask = smoothstep(_Radius - _Softness, _Radius, dist);

                return float4(1, 1, 1, mask);
            }

            ENDHLSL
        }
    }
}