using System.Collections.Generic;
using UnityEngine;

namespace Weather
{
    [RequireComponent(typeof(Light))]
    public class WeatherManager : MonoBehaviour
    {

        [System.Serializable]
        public struct WeatherTypeProfileEntry
        {
            public WeatherType type;
            public WeatherTypeProfile profile;
        }

        [SerializeField] private List<WeatherTypeProfileEntry> weatherTypeProfileMap;

        public static WeatherManager Instance { get; private set; }
        public enum WeatherType
        {
            None,
            DarkRain,
            AfternoonSun,
            LightRain,
            Flowers,
            DampAmbience,
            DarkCave
        }

        //TODO: do I even want to track this, or have it just be responsibility of the map design?
        [SerializeField] private WeatherType environment = WeatherType.AfternoonSun;
        private WeatherType previousEnvironment = WeatherType.None;

        Dictionary<WeatherType, WeatherTypeProfile> profiles;

        private Camera cam;
        private Light sunLight;

        private void Awake()
        {
            //May only be one instance ofc
            if (Instance != null && Instance != this) Destroy(this); 
            else Instance = this; 

            cam = Camera.main;
            sunLight = GetComponent<Light>();

            //build runtime dict
            profiles = new Dictionary<WeatherType, WeatherTypeProfile>();
            foreach (var entry in weatherTypeProfileMap)
                profiles[entry.type] = entry.profile;

            foreach (var prof in profiles.Values)
            {
                prof.gameObject.SetActive(false);
            }

            SetActiveEnvironment();

        }
        

        //Update to the camera's position for particles to render correctly
        void LateUpdate()
        {
            transform.position = cam.transform.position;
        }


        public void SetWeatherType(WeatherType type)
        {
            SetWeatherTypeBlend(type, type, 0f); //heheheheh
        }


        public void SetWeatherTypeBlend(WeatherType a, WeatherType b, float blend)
        {
            if(!(profiles.TryGetValue(a, out var aProfile) && profiles.TryGetValue(b, out var bProfile)))
            {
                Debug.LogError($"[WeatherManager]: WeatherType {a} or {b} is not in dictionary");
                return;
            }

            //dynamically turn off unused profiles
            foreach (var (type, profile) in profiles)
            {
                profile.gameObject.SetActive(type == a || type == b);
            }

            aProfile.SetBlend(blend);
            bProfile.SetBlend(1f-blend);

            BlendLightingData(aProfile, bProfile, blend);



        }


        private void BlendLightingData(WeatherTypeProfile a, WeatherTypeProfile b, float t)
        {
            sunLight.intensity = Mathf.Lerp(a.sunIntensity, b.sunIntensity, t);
            sunLight.color = Color.Lerp(a.sunColor, b.sunColor, t);

            transform.rotation = Quaternion.Slerp(
                Quaternion.Euler(a.sunRotation), 
                Quaternion.Euler(b.sunRotation), t);

            sunLight.shadowStrength = Mathf.Lerp(a.shadowStrength, b.shadowStrength, t);

            //TODO: do later
            // ambientIntensity = Mathf.Lerp(a.ambientIntensity, b.ambientIntensity, t),
        }






        public void SetActiveEnvironment()
        {
            if (environment == previousEnvironment) return;

            // GameObject oldObject = EnvironmentObjects[previousEnvironment];
            // GameObject newObject = EnvironmentObjects[environment];

            // previousEnvironment = environment;

            // newObject.SetActive(true);
            // oldObject.SetActive(false);
        }

    #if UNITY_EDITOR
        private void OnValidate()
        {
            SetActiveEnvironment();
        }
    #endif
    }
}

