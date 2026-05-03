using System.Collections.Generic;
using Modules.Outlines;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class JFA_Outline_Main_Pass : ScriptableRenderPass
{
    public JFA_Outline_Main_Pass()
    {
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        CameraType cameraType = cameraData.camera.cameraType;
        if (cameraType != CameraType.Game && cameraType != CameraType.SceneView)
            return;
                
        RegisterPasses(renderGraph, frameData);
    }

    private void RegisterPasses(RenderGraph renderGraph, ContextContainer frameData)
    {
        OutlineObject[] outlineObjects = OutlineObject.All.ToArray();

        if (outlineObjects.Length == 0)
        {
            return;
        }

        foreach (OutlineObject outlineObject in outlineObjects)
        {
            TextureSet silhouetteTex = GetSilhouetteTexture(renderGraph, frameData, outlineObject);
            TextureHandle jfaTex = JFA_Pass.JFA(renderGraph, frameData, silhouetteTex.ColorTexture);
            Outline_pass.DrawOutline(renderGraph, frameData, jfaTex, silhouetteTex.ColorTexture, silhouetteTex.DepthTexture, outlineObject.outlineInfo);
        }
    }
    
    private TextureSet GetSilhouetteTexture(RenderGraph renderGraph, ContextContainer frameData, OutlineObject outlineObject)
    {
        List<Renderer> renderersToOutline = new List<Renderer>(outlineObject.Renderers);
        TextureSet silhouetteTexture = Silhouette_Pass.BuildSilhouette(renderGraph, frameData, renderersToOutline);
        if (!silhouetteTexture.ColorTexture.IsValid())
        {
            Debug.LogWarning($"Silhouette texture for {outlineObject.gameObject.name} is not valid. This likely means there were no renderers to outline for this object.");
        }
        return silhouetteTexture;
    }
}
