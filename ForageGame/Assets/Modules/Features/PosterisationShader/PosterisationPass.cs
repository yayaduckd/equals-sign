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
    }

    private void RegisterPass(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        
        TextureHandle source = resourceData.activeColorTexture;

        TextureDesc desc = renderGraph.GetTextureDesc(source);
        desc.name = "TempColor";

        TextureHandle temp = renderGraph.CreateTexture(desc);
        renderGraph.AddBlitPass(source, temp, new Vector2(1, 1));
        
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("PosterisationPass", out var passData))
        {
            TextureHandle camera = resourceData.activeColorTexture;
         
            passData.PosterisationMaterial = posterisationMaterial.GetMaterial();
            passData.mpb = mpb;
            passData.channelBinCounts = channelBinCounts;
            passData.colourSpace = colourSpace;
            
            builder.SetRenderAttachment(camera, 0, AccessFlags.ReadWrite);

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

    
}