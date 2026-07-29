using UnityEngine;

public class WorldLanternController : MonoBehaviour
{
    [SerializeField] private Light pointLight;
    [SerializeField] private Renderer glassRenderer;

    [SerializeField] private float baseLightIntensity = 5.0f;
    [SerializeField] private float baseFlicker = 1.0f;
    [SerializeField][Range(0f, 10f)] private float flickerSpeed = 1f;
    [SerializeField][Range(0f, .4f)] private float flickerScale = .2f;

    [SerializeField] FBM1D fbm = new FBM1D(FBM1D.NoiseFunctionType.Sin, 4, 1.97f, 0.43f);

    [SerializeField] private float flickerOffset; //make sure not all lanterns flicker in sync

    private MaterialPropertyBlock propertyBlock;

    private static readonly int FlickerID = Shader.PropertyToID("_Flicker");

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        flickerOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        float flicker = baseFlicker + (fbm.Eval01((Time.time + flickerOffset) * flickerSpeed)- 0.5f) * flickerScale;
        // However you calculate your flicker.
        // float flicker = baseFlicker + Mathf.Sin((Time.time + flickerOffset) * flickerSpeed) * flickerScale;

        pointLight.intensity = baseLightIntensity * flicker;

        glassRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(FlickerID, flicker);
        glassRenderer.SetPropertyBlock(propertyBlock);
    }
}