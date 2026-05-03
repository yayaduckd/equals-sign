using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Modules.Outlines
{
    public class Fullscreen_with_material_pass
    {
        class PassData
        {
            public TextureHandle src;
            public TextureHandle target;
            public Material mat;
            public MaterialPropertyBlock mpb;
        }
        
        public static void RenderPass(RenderGraph renderGraph, TextureHandle inputTexture, TextureHandle outputTexture, Material mat, string passName, string materialTextureName = "_MainTex", MaterialPropertyBlock mpb = null)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                if (mpb == null)
                {
                    mpb = new MaterialPropertyBlock(); 
                }

                passData.mpb = mpb;
                passData.src = inputTexture;
                passData.mat = mat;
                passData.target = outputTexture;

                builder.UseTexture(passData.src, AccessFlags.Read);
                builder.SetRenderAttachment(passData.target, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    data.mpb.SetTexture(materialTextureName, data.src);
                    ctx.cmd.DrawProcedural(Matrix4x4.identity, data.mat, 0,
                        MeshTopology.Triangles, 3, 1, mpb);
                });
            }
        }
    }
}