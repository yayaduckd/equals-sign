using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public static class ThresholdTexture
{
    class PassData
    {
        public TextureHandle src;
        public TextureHandle target;
        public Material mat;
        public float threshold;
    }


    static Material ThresholdMat;

    private static Material thresholdMat;
    static Material GetMaterial()
    {
        if (thresholdMat != null) return thresholdMat;

        var shader = Shader.Find("Hidden/ThresholdWhite");
        if (shader == null)
        {
            Debug.LogError("Could not find shader Hidden/ThresholdWhite");
            return null;
        }

        thresholdMat = CoreUtils.CreateEngineMaterial(shader);
        return thresholdMat;
    }


    public static TextureHandle Threshold(
        TextureHandle textureToThreshold,
        float threshold,
        RenderGraph renderGraph
        )
    {
        TextureDesc desc = textureToThreshold.GetDescriptor(renderGraph);
        desc.name = "ThresholdTextureOutput";
        desc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;

        TextureHandle output = renderGraph.CreateTexture(desc);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("ThresholdTexturePass", out var passData))
        {
            passData.src = textureToThreshold;
            passData.mat = GetMaterial();
            passData.target = output;
            passData.threshold = threshold;
            Debug.Log(passData.mat.name);

            builder.UseTexture(passData.src, AccessFlags.Read);
            builder.SetRenderAttachment(passData.target, 0, AccessFlags.Write);

            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                Debug.Log("Executing ThresholdTexturePass");
                data.mat.SetTexture("_SourceTex", data.src);
                data.mat.SetFloat("_Threshold", data.threshold);
                ctx.cmd.DrawProcedural(Matrix4x4.identity, data.mat, 0,
                                       MeshTopology.Triangles, 3, 1);
            });
        }

        return output;
    }
}