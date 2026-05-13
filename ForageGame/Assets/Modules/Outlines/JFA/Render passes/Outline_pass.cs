using Modules.Outlines;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class Outline_pass
{
    private static ShaderMaterial OutlineMaterial = new ShaderMaterial("Hidden/OutlineFromJFA");

    private class PassData
    {
        public Material OutlineMaterial;
        public MaterialPropertyBlock mpb;
        public TextureHandle JFATexture;
        public TextureHandle SilhouetteTexture;
        public TextureHandle depthTexture;
    }
    
    public static void DrawOutline(RenderGraph renderGraph, ContextContainer frameData,
        TextureHandle JFATex, TextureHandle SilhouetteTex, TextureHandle depthTex, OutlineInfo outlineInfo)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        
        MaterialPropertyBlock mpb = new MaterialPropertyBlock(); 
        mpb.SetFloat("_OutlineWidth", outlineInfo.outlineWidth);
        mpb.SetColor("_OutlineColor", outlineInfo.outlineColor);
        mpb.SetFloat("_DoWobbleBool", outlineInfo.doWobble ? 1f : 0f);
        mpb.SetFloat("_WobbleNoiseScale", outlineInfo.wobbleScale);
        mpb.SetFloat("_WobbleNoiseSpeed", outlineInfo.wobbleSpeed);
        mpb.SetFloat("_WobbleMaxIndentFactor", outlineInfo.wobbleMaxIndentFactor);
        mpb.SetFloat("_WobbleNoisePersistence", outlineInfo.wobblePersistence);
        mpb.SetFloat("_WobbleNoiseLacunarity", outlineInfo.wobbleLacunarity);
        mpb.SetInt("_WobbleNoiseOctaves", outlineInfo.wobbleOctaves);
        
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Outline_Pass", out var passData))
        {
            passData.OutlineMaterial = OutlineMaterial.GetMaterial();
            passData.mpb = mpb;
            passData.JFATexture = JFATex;
            passData.SilhouetteTexture = SilhouetteTex;
            passData.depthTexture = depthTex;
            
            TextureHandle camera = resourceData.activeColorTexture;
            TextureHandle depth =  resourceData.activeDepthTexture;
            builder.SetRenderAttachment(camera, 0, AccessFlags.ReadWrite);
            builder.SetRenderAttachmentDepth(depth, AccessFlags.ReadWrite);
            
            builder.UseTexture(passData.JFATexture, AccessFlags.Read);
            builder.UseTexture(passData.SilhouetteTexture, AccessFlags.Read);
            builder.UseTexture(passData.depthTexture, AccessFlags.Read);
            builder.UseTexture(frameData.Get<UniversalResourceData>().cameraDepthTexture, AccessFlags.Read);

            builder.SetRenderFunc((PassData passData, RasterGraphContext context) =>
            {
                passData.mpb.SetTexture("_JFATex", passData.JFATexture);
                passData.mpb.SetTexture("_SilhouetteTex", passData.SilhouetteTexture);
                passData.mpb.SetTexture("_DepthTex", passData.depthTexture);
                
                context.cmd.DrawProcedural(Matrix4x4.identity, passData.OutlineMaterial, 0,
                    MeshTopology.Triangles, 3, 1, passData.mpb);
            });
        }
    }

}
