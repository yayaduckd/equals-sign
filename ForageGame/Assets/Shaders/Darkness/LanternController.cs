using DG.Tweening;
using DG.Tweening.Core;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Weather;

public class LanternController : MonoBehaviour
{
    [SerializeField] private Light _light;

    [SerializeField] private Color mutedColor = Color.red;
    [SerializeField] private Color BrightColor = Color.yellow;

    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 5f;

    [SerializeField][Range(0f, 1f)] private float lanternStrength = 0.5f;

    [SerializeField][Range(0f, 10f)] private float flickerSpeed = 1f;

    [SerializeField] FBM1D fbm = new FBM1D(FBM1D.NoiseFunctionType.Sin, 4, 1.97f, 0.43f);


    private float weight = 0f;

    private void Update()
    {
        weight = WeatherManager.Instance.lanternWeight; //weathermanager decides relative lantern strength
        if(weight > 0f)
        {
            lanternStrength = fbm.Eval01(Time.time * flickerSpeed);
            SetLanternVisuals();
        }
        else _light.intensity = 0f;
    }

    [ContextMenu("Set Lantern Visuals")]
    private void SetLanternVisuals()
    {
        _light.color = Color.Lerp(mutedColor, BrightColor, lanternStrength);
        _light.intensity = Mathf.Lerp(minIntensity*weight, maxIntensity*weight, lanternStrength);
    }

}
