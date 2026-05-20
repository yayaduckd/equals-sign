using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using System.Linq;
using Modules.Outlines;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class PosterisationPass : ScriptableRenderPass
{
    private Vector3 channelBinCounts;
    private ColourSpace colourSpace;

    public enum ColourSpace
    {
        RGB,
        HSL,
        CIELAB
    }

    public PosterisationPass(Vector3 channelBinCounts, ColourSpace colourSpace)
    {
        this.colourSpace = colourSpace;
        this.channelBinCounts = channelBinCounts;
    }
    
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        CameraType cameraType = cameraData.camera.cameraType;
        if (cameraType != CameraType.Game && cameraType != CameraType.SceneView)
            return;
        
        RegisterPass(renderGraph, frameData);
    }

    private static ShaderMaterial posterisationMaterial = new ShaderMaterial("Hidden/Posterisation");
    
    class PassData
    {
        public MaterialPropertyBlock mpb;
        public Material PosterisationMaterial;
        public Vector3 channelBinCounts;
        public ColourSpace colourSpace;
        public TextureHandle tempCameraCopy;
    }

    private void RegisterPass(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        
        TextureHandle temp = copySourceToTemp(renderGraph, frameData, resourceData.activeColorTexture);
        
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("PosterisationPass", out var passData))
        {
            passData.tempCameraCopy = temp;
            passData.PosterisationMaterial = posterisationMaterial.GetMaterial();
            passData.mpb = mpb;
            passData.channelBinCounts = channelBinCounts;
            passData.colourSpace = colourSpace;
            
            builder.UseTexture(temp, AccessFlags.Read);
            builder.SetRenderAttachment(camera, 0, AccessFlags.Write);

            builder.SetRenderFunc((PassData passData, RasterGraphContext context) =>
            {
                passData.mpb.SetVector("_ChannelBinCounts", passData.channelBinCounts);
                passData.mpb.SetTexture("_MainTex", resourceData.activeColorTexture);
                passData.mpb.SetInt("_ColourSpace", (int)passData.colourSpace);       
                
                context.cmd.DrawProcedural(Matrix4x4.identity, passData.PosterisationMaterial, 0,
                    MeshTopology.Triangles, 3, 1, passData.mpb);
            });
        }
    }


    class BlitPassData
    {
        public TextureHandle source;
        public TextureHandle dest;
    }
    private TextureHandle copySourceToTemp(RenderGraph renderGraph, ContextContainer frameData, TextureHandle source)
    {
        // Create a temp texture matching source
        TextureHandle temp = renderGraph.CreateTexture(
            renderGraph.GetTextureDesc(source) // copies desc from source
        );

        using (var builder = renderGraph.AddRasterRenderPass<BlitPassData>("Blit camera to temp", out var passData))
        {

            builder.UseTexture(source, AccessFlags.Read);
            builder.SetRenderAttachment(temp, 0, AccessFlags.Write);

            passData.source = source;
            passData.dest = temp;

            builder.SetRenderFunc((BlitPassData data, RasterGraphContext ctx) =>
            {
                Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
            });


        }
        
        return temp;
    }
}