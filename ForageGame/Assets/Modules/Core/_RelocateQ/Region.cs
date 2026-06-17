using UnityEngine;
using FMODUnity;
using Weather;

/// <summary>
/// One subregion in the world
/// Associates the ambience event and the weather type
/// 
/// ~Lars
/// </summary>
[CreateAssetMenu(fileName = "Region", menuName = "Region")]
public class Region : ScriptableObject
{
    public EventReference ambienceEvent;
    public WeatherTypeProfile weatherTypeProfile;

}
