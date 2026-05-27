Shader "Hidden/JFA_Step"
{
    Properties
    {
        _InputTex("Input Texture", 2D) = "black" {}
        _StepSize("Step Size", Float) = 1.0
    }
    SubShader
    {
        ZWrite Off ZTest Always Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            TEXTURE2D(_InputTex);
            SAMPLER(sampler_InputTex);

            float4 _ScreenParams;
            float _StepSize;
            
            struct Varyings { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
            
            
            bool is_valid(float2 uv)
            {
            	// Uncolored pixels have UV (-1, -1), so we can check if the UV is valid by checking if it's greater than or equal to 0
                return uv.x >= 0.0001 && uv.y >= 0.0001; 
            }
            
            float2 jfa(float2 p_uv)
            {
            	// The JFA step is to sample the seed texture at the current pixel and return the UV of the seed if it exists, otherwise return (-1, -1)
                // We store the UV of the seed in the red and green channels of the seed texture, so we can sample it directly
                // Call the current point "P" and the neighbour point "Q".
                int2 resolution = _ScreenParams.xy;
            	
				int2 p_pos = clamp(floor(p_uv * resolution), int2(0, 0), resolution - 1);
				float2 p_seed_uv = LOAD_TEXTURE2D(_InputTex, p_pos).rg;
            	float2 p_seed_pos = p_seed_uv * resolution; 

            	float2 output_uv = p_seed_uv; // Default to the current pixel's seed UV, which may be (-1, -1) if it's uncolored
            	
            	float2 best_candiate_seed_uv = p_seed_uv; // Start with the current pixel's seed UV as the best candidate
				float2 best_candidate_seed_pos = p_seed_pos;
                
            	for (int x = -1; x <= 1; x++)
				{
					for (int y = -1; y <= 1; y++)
					{
						if (x == 0 && y == 0) continue; // Skip the current pixel
						
						int2 pixel_offset = int2(x, y) * _StepSize; 
						
                        int2 q_pos = p_pos + pixel_offset;
						if (any(q_pos < int2(0, 0)) || any(q_pos >= resolution.xy)) continue; // Skip out-of-bounds pixels
						float2 q_seed_uv = LOAD_TEXTURE2D(_InputTex, q_pos).rg;
						float2 q_seed_pos = q_seed_uv * resolution;
						
						if (is_valid(q_seed_uv)) 
						{
							if(is_valid(best_candiate_seed_uv))
                            {
								float best_dist = distance(p_pos, best_candidate_seed_pos);
								float q_dist = distance(p_pos, q_seed_pos);

								if (q_dist < best_dist)
								{
									output_uv = q_seed_uv; 
									best_candiate_seed_uv = q_seed_uv;
									best_candidate_seed_pos = q_seed_pos;
								}
							}
							else
							{
								output_uv = q_seed_uv;
								best_candiate_seed_uv = q_seed_uv;
								best_candidate_seed_pos = q_seed_pos;
                            }
						}
					}
                }
            	
            	return output_uv;
            }
            
            Varyings vert(uint id : SV_VertexID)
            {
                Varyings o;
                o.pos = GetFullScreenTriangleVertexPosition(id);
                o.uv  = GetFullScreenTriangleTexCoord(id);
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float2 p_uv = jfa(i.uv);
            	return float4(p_uv, 0, 0);
            }


            ENDHLSL
        }
    }
}
