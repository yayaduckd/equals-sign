using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// One type of weather, holds lighting data to be blended by the shared WeatherManager,
/// and manages children particle emitters and possible extra functionality, like lightning or the lantern.
/// 
/// ~Lars
/// </summary>
[RequireComponent(typeof(Volume))]
public class WeatherType : MonoBehaviour
{
    [Header("Lighting Data")]
    public float sunIntensity;
    public Color sunColor;
    public Quaternion sunAngle;
    public float shadowStrength;

    public Material skyBox; //yeah this is gonna be f u n

    private Volume volume;

    private void Awake()
    {
        volume = GetComponent<Volume>();
    }
    

    /// <summary>
    /// Called by the WeatherManager, to have this figure out how to set its particle effects and possible extra stuff 
    /// (i.e., lantern or thunder)
    /// 
    /// lighting data is just polled by the WeatherManager
    /// </summary>
    /// <param name="val"></param>
    public void SetBlend(float val)
    {
        volume.weight = val;
    }
}
