using System;
using UnityEngine;
using System.Collections.Generic;

namespace Weather
{
    public enum WeatherType
    {
        None,
        DarkRain,
        AfternoonSun,
        LightRain,
        Blossom,
        DampAmbience,
        DarkCave
    }

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


        Dictionary<WeatherType, WeatherTypeProfile> profiles;

        private Camera cam;
        [SerializeField] private Light sunLight;

        public float lanternWeight;

        private void Awake()
        {
            //May only be one instance ofc
            if (Instance != null && Instance != this) Destroy(this); 
            else Instance = this; 

            cam = Camera.main;

            //build runtime dict
            profiles = new Dictionary<WeatherType, WeatherTypeProfile>();
            foreach (var entry in weatherTypeProfileMap)
                profiles[entry.type] = entry.profile;

            foreach (var prof in profiles.Values)
            {
                prof.gameObject.SetActive(false);
            }


            //TODO: this is debug
            SetWeatherType(WeatherType.Blossom);

        }
        

        //Update to the camera's position for particles to render correctly
        void LateUpdate()
        {
            transform.position = cam.transform.position;
        }


        public void SetWeatherType(WeatherType type)
        {
            SetWeatherTypeBlend(type, type, 1f); //heheheheh this is real nasty, don't tell anyone
        }


        public void SetWeatherTypeBlend(WeatherType a, WeatherType b, float blend)
        {
            if(!(profiles.TryGetValue(a, out var aProfile) && profiles.TryGetValue(b, out var bProfile)))
            {
                Debug.LogError($"[WeatherManager]: WeatherType {a} or {b} is not in dictionary");
                return;
            }

            //Debug.Log($"Blending between weather: {a} to {b} with value {blend}");

            //dynamically turn off unused profiles
            foreach (var (type, profile) in profiles)
            {
                profile.gameObject.SetActive(type == a || type == b);
            }

            aProfile.SetBlend(1f-blend);
            bProfile.SetBlend(blend);

            lanternWeight = Mathf.Lerp(aProfile.lanternIntensity, bProfile.lanternIntensity, blend);

            BlendLightingData(aProfile, bProfile, blend);



        }


        private void BlendLightingData(WeatherTypeProfile a, WeatherTypeProfile b, float t)
        {
            sunLight.intensity = Mathf.Lerp(a.sunIntensity, b.sunIntensity, t);
            sunLight.color = Color.Lerp(a.sunColor, b.sunColor, t);

            sunLight.transform.rotation = Quaternion.Slerp(
                Quaternion.Euler(a.sunRotation), 
                Quaternion.Euler(b.sunRotation), t);

            sunLight.shadowStrength = Mathf.Lerp(a.shadowStrength, b.shadowStrength, t);

            RenderSettings.skybox.Lerp(a.skyBox, b.skyBox, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(a.ambientIntensity, b.ambientIntensity, t);
            DynamicGI.UpdateEnvironment(); //actually updates the lighting
        }

    //TODO: this wont work in non-runtime
    // #if UNITY_EDITOR
    //     private void OnValidate()
    //     {
    //         SetWeatherType(WeatherType.Blossom); //TODO: this is debug, change to more natural one
    //     }
    // #endif
    }
}

