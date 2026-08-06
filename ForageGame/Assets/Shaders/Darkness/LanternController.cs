using DG.Tweening;
using DG.Tweening.Core;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Weather;

public class PlayerLanternController : MonoBehaviour
{
    [SerializeField] private Light _light;
    [SerializeField] private Renderer glassRenderer;

    [SerializeField] private Color mutedColor = Color.red;
    [SerializeField] private Color BrightColor = Color.yellow;

    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 5f;

    [SerializeField][Range(0f, 1f)] private float lanternStrength01 = 0.5f;

    [SerializeField][Range(0f, 10f)] private float flickerSpeed = 1f;
    [SerializeField][Range(0f, .4f)] private float flickerScale = .2f;

    [SerializeField] FBM1D fbm = new FBM1D(FBM1D.NoiseFunctionType.Sin, 4, 1.97f, 0.43f);

    private MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

    private static readonly int FlickerID = Shader.PropertyToID("_Flicker");


    private float weight = 0f;

    private void Update()
    {
        weight = WeatherManager.Instance.lanternIntensity; //weathermanager decides relative lantern strength
        if(weight > 0f)
        {
            lanternStrength01 = fbm.Eval01(Time.time * flickerSpeed);
            SetLanternVisuals();
        }
        else 
        {
            _light.intensity = 0f;
        }
    }

    [ContextMenu("Set Lantern Visuals")]
    private void SetLanternVisuals()
    {
        _light.color = Color.Lerp(mutedColor, BrightColor, lanternStrength01);
        _light.intensity = Mathf.Lerp(minIntensity*weight, maxIntensity*weight, lanternStrength01);

        glassRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(FlickerID, (lanternStrength01 + 1f)*flickerScale);
        glassRenderer.SetPropertyBlock(propertyBlock);
    }

}
