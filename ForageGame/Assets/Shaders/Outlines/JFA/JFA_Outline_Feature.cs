using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class JFA_Outline_Feature : ScriptableRendererFeature
{
    JFA_Outline_Main_Pass _pass;
    
    public RenderPassEvent injectionPoint = RenderPassEvent.BeforeRenderingPostProcessing;
    

    public override void Create()
    {
        _pass = new JFA_Outline_Main_Pass();
        _pass.renderPassEvent = injectionPoint;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_pass);
    }
}
