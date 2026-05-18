using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PosterisationRenderFeature : ScriptableRendererFeature
{
    PosterisationPass _pass;
    
    public RenderPassEvent injectionPoints;

    public Palette palette;
    
    public override void Create()
    {
        _pass = new PosterisationPass(palette);
        _pass.renderPassEvent = injectionPoints;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_pass);
    }
}
