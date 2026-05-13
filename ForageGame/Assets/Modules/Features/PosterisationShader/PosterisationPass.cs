using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class PosterisationPass : ScriptableRenderPass
{
    private List<Color> palette;

    public PosterisationPass(List<Color> palette)
    {
        this.palette = palette; 
    }
    
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        CameraType cameraType = cameraData.camera.cameraType;
        if (cameraType != CameraType.Game && cameraType != CameraType.SceneView)
            return;
        
        RegisterPass(renderGraph, frameData);
    }

    private void RegisterPass(RenderGraph renderGraph, ContextContainer frameData)
    {
        
    }

    
}