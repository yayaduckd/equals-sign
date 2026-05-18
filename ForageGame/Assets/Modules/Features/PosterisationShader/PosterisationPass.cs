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
    private List<Vector4> paletteUV;

    public PosterisationPass(Palette palette)
    {
        paletteUV = palette.colours.Select(c => (Vector4)c).ToList();
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
        public List<Vector4> paletteUV;
        public TextureHandle tempCameraCopy;
    }

    private void RegisterPass(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        
        TextureHandle temp = copySourceToTemp(renderGraph, frameData, resourceData.activeColorTexture);
        
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("PosterisationPass", out var passData))
        {
            TextureHandle camera = resourceData.activeColorTexture;
         
            passData.PosterisationMaterial = posterisationMaterial.GetMaterial();
            passData.mpb = mpb;
            passData.paletteUV = paletteUV;
            passData.tempCameraCopy = temp;

            builder.UseTexture(temp, AccessFlags.Read);
            builder.SetRenderAttachment(camera, 0, AccessFlags.Write);

            builder.SetRenderFunc((PassData passData, RasterGraphContext context) =>
            {
                passData.mpb.SetFloat("_PaletteSize", passData.paletteUV.Count);
                passData.mpb.SetTexture("_MainTex", passData.tempCameraCopy);
                passData.PosterisationMaterial.SetVectorArray("_Palette", passData.paletteUV);
                
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