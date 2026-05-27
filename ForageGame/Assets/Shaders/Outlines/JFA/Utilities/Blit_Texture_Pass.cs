using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class Blit_Texture_Pass
{
    class PassData
    {
        public TextureHandle src;
        public TextureHandle target;
    }

    public static void BlitToScreen(TextureHandle textureToBlit, RenderGraph renderGraph, ContextContainer frameData)
    {
        TextureHandle camera = frameData.Get<UniversalResourceData>().activeColorTexture;
        
        Blit_Texture_Pass.BlitFromTo(textureToBlit, camera, renderGraph, frameData);
    }
    
    public static void BlitFromTo(TextureHandle from, TextureHandle to, RenderGraph renderGraph, ContextContainer frameData, string PassName = "Blit Texture")
    {
        var resourceData = frameData.Get<UniversalResourceData>();

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(PassName, out var passData))
        {
            passData.src = from;
            passData.target = to;

            builder.UseTexture(passData.src, AccessFlags.Read);
            builder.SetRenderAttachment(passData.target, 0, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                Blitter.BlitTexture(ctx.cmd, data.src, new Vector4(1, 1, 0, 0), 0, false);
            });
        }
    }
}
