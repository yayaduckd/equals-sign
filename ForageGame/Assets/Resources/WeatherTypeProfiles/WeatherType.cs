using UnityEngine;

namespace Weather
{
    [CreateAssetMenu(fileName = "WeatherType", menuName = "Weather/WeatherType")]
    public class WeatherType : ScriptableObject
    {
        public string displayName;
    }
}
