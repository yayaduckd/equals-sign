using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;
using Modules.Outlines;
using UnityEngine.Experimental.Rendering;

public class Silhouette_Pass
{
    class PassData
    {
        public Material SilhouetteMaterial;
        public List<Renderer> RenderersToOutline;
    }

    private static ShaderMaterial silhouetteMaterial = new ShaderMaterial("Hidden/Silhouette");
    
    public static TextureSet BuildSilhouette(RenderGraph renderGraph, ContextContainer frameData, List<Renderer> renderersToOutline)
    {
        var resourceData = frameData.Get<UniversalResourceData>();
        var source = resourceData.activeColorTexture;
        var desc = renderGraph.GetTextureDesc(source);
        desc.colorFormat = GraphicsFormat.R16_UNorm;
        desc.depthBufferBits = 0;
        desc.msaaSamples = MSAASamples.None;
        desc.name = "_SilhouetteMask";
        
        TextureDesc depthDesc = desc;
        depthDesc.name = "_SilhouetteMaskDepth";
        depthDesc.colorFormat = GraphicsFormat.R32_SFloat;
        
        TextureSet output = new TextureSet();
        output.ColorTexture = TextureHandle.nullHandle;
        output.DepthTexture = TextureHandle.nullHandle;

        if(renderersToOutline.Count == 0)
        {
            return output;
        }

        TextureHandle outputTexture = renderGraph.CreateTexture(desc);
        TextureHandle depthTexture = renderGraph.CreateTexture(depthDesc);
        
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Silhouette_Pass", out var passData))
        {
            passData.RenderersToOutline = renderersToOutline;
            passData.SilhouetteMaterial = silhouetteMaterial.GetMaterial();

            builder.SetRenderAttachment(outputTexture, 0, AccessFlags.Write);
            builder.SetRenderAttachment(depthTexture, 1, AccessFlags.Write);

            builder.SetRenderFunc((PassData passData, RasterGraphContext context) =>
            {
                var cmd = context.cmd;

                cmd.ClearRenderTarget(RTClearFlags.All, Color.black, 1f, 0);

                foreach (var renderer in passData.RenderersToOutline)
                {
                    cmd.DrawRenderer(renderer, passData.SilhouetteMaterial, 0, 0);
                }
            });
        }
        
        output.ColorTexture = outputTexture;
        output.DepthTexture = depthTexture;
        return output;
    }

}