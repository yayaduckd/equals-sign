using Modules.Outlines;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Experimental.Rendering;

public class JFA_Pass
{
    private static ShaderMaterial _JFAInitMaterial = new ShaderMaterial("Hidden/JFA_Init");
    private static ShaderMaterial _JFAStepMaterial = new ShaderMaterial("Hidden/JFA_Step");
    
    public static TextureHandle JFA(RenderGraph renderGraph, ContextContainer frameData, TextureHandle silhouetteTexture)
    {
        TextureHandle initializedTex = UVPositionsPrepass(renderGraph, frameData, silhouetteTexture);

        TextureDesc descA = initializedTex.GetDescriptor(renderGraph);
        TextureDesc descB = initializedTex.GetDescriptor(renderGraph);
        descA.name = "JFA_A";
        descB.name = "JFA_B";
        TextureHandle JFA_A = renderGraph.CreateTexture(descA);
        TextureHandle JFA_B = renderGraph.CreateTexture(descB);
        
        CopyInitToStep(renderGraph, initializedTex, JFA_A);

        Vector2 screenSize = new Vector2(descA.width, descA.height);
        int N_steps = (int)Mathf.Ceil(Mathf.Log(Mathf.Max(screenSize.x, screenSize.y), 2)) + 1; // +1 to ensure we cover the last pixel in cases where screen size is not a power of 2.
        TextureHandle output = JFA_A;
        
        for (int i = 0; i < N_steps; i++)
        {
            int stepsize = (int)Mathf.Pow(2, N_steps - 1 - i);
            if (i % 2 == 0)
            {
                JFA_Step(renderGraph, frameData, JFA_A, JFA_B, stepsize);
                output = JFA_B;
            }
            else
            {
                JFA_Step(renderGraph, frameData, JFA_B, JFA_A, stepsize);
                output = JFA_A;
            }        
        }
        return output;
    }

    class PassData
    {
        public TextureHandle src;
        public TextureHandle target;
        public Material mat;
        public MaterialPropertyBlock mpb;
    }

    private static TextureHandle UVPositionsPrepass(RenderGraph renderGraph, ContextContainer frameData, TextureHandle silhouetteTexture)
    {
        TextureDesc desc = silhouetteTexture.GetDescriptor(renderGraph);
        desc.name = "JFA_Init_Texture";
        desc.colorFormat = GraphicsFormat.R16G16_SFloat;
        TextureHandle output = renderGraph.CreateTexture(desc);

        Fullscreen_with_material_pass.RenderPass(renderGraph, silhouetteTexture, output, _JFAInitMaterial.GetMaterial(), "JFA_UV_Positions_Prepass", "_SilhouetteTex");

        return output;
    }
    
    private static void CopyInitToStep(RenderGraph renderGraph, TextureHandle initializedTex, TextureHandle JFA_A)
    {
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("JFA_Copy_Init_To_Step", out var passData))
        {
            passData.src = initializedTex;
            passData.target = JFA_A;

            builder.UseTexture(passData.src, AccessFlags.Read);
            builder.SetRenderAttachment(passData.target, 0, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                Blitter.BlitTexture(ctx.cmd, data.src, new Vector4(1, 1, 0, 0), 0, false);
            });
        }
    }

    private static void JFA_Step(RenderGraph renderGraph, ContextContainer frameData, TextureHandle inputTexture, TextureHandle outputTexture, int stepSize)
    {
        using (var builder = renderGraph.AddRasterRenderPass<PassData>($"JFA_Step_{stepSize}", out var passData))
        {
            passData.src = inputTexture;
            passData.mat = _JFAStepMaterial.GetMaterial();
            passData.target = outputTexture;
            
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            passData.mpb = mpb;
            passData.mpb.SetFloat("_StepSize", stepSize);

            builder.UseTexture(passData.src, AccessFlags.Read);
            builder.SetRenderAttachment(passData.target, 0, AccessFlags.Write);
            
            
            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                // TextureHandle can only be resolved while inside an executing RenderGraph pass.
                data.mpb.SetTexture("_InputTex", data.src);
                ctx.cmd.DrawProcedural(Matrix4x4.identity, data.mat, 0,
                    MeshTopology.Triangles, 3, 1, data.mpb);
            });
        }
    }
}