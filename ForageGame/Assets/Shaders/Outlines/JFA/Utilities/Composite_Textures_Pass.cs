using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Modules.Outlines
{
    public class Composite_Textures_Pass
    {
        private static Material _CompositeMat;

        static Material CompositeMat
        {
            get
            {
                const string shaderName = "Hidden/CompositeTextures";

                if (_CompositeMat != null) return _CompositeMat;

                var shader = Shader.Find(shaderName);
                if (shader == null)
                {
                    Debug.LogError($"Could not find shader {shaderName}");
                    return null;
                }

                _CompositeMat = CoreUtils.CreateEngineMaterial(shader);
                return _CompositeMat;
            }

        }

        private class PassData
        {
            public Material mat;
            public TextureHandle bgTex;
            public TextureHandle overlayTex;
            public TextureHandle target;
        }

    private static TextureHandle Composite(RenderGraph renderGraph, ContextContainer frameData, TextureHandle bgTex,
            TextureHandle overlayTex)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            TextureHandle camera = frameData.Get<UniversalResourceData>().activeColorTexture;
            TextureDesc desc = camera.GetDescriptor(renderGraph);
            desc.name = "Composite Pass Target";
            TextureHandle target = renderGraph.CreateTexture(desc);
            
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Composite_Pass", out var passData))
            {
                passData.bgTex = bgTex;
                passData.overlayTex = overlayTex;
                passData.target = target;
                passData.mat = CompositeMat;
                
                
                builder.UseTexture(passData.bgTex, AccessFlags.Read);
                builder.UseTexture(passData.overlayTex, AccessFlags.Read);
                builder.SetRenderAttachment(passData.target, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    data.mat.SetTexture("_BackgroundTex", data.bgTex);
                    data.mat.SetTexture("_OverlayTex", data.overlayTex);
                    ctx.cmd.DrawProcedural(Matrix4x4.identity, data.mat, 0,
                        MeshTopology.Triangles, 3, 1, mpb);
                });
            }

            return target;
        }
    }
}