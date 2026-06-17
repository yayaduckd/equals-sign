using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Weather
{
    // public enum WeatherType
    // {
    //     None,
    //     DarkRain,
    //     AfternoonSun,
    //     LightRain,
    //     Blossom,
    //     DampAmbience,
    //     DarkCave,
    //     Thunder,
    //     Clear,
    //     Morning,
    //     ShadedForest,
    //     Overcast
    // }

    public class WeatherManager : MonoBehaviour
    {

        // [System.Serializable]
        // public struct WeatherTypeProfileEntry
        // {
        //     public WeatherType type;
        //     public WeatherTypeProfile profile;
        // }

        // [SerializeField] private List<WeatherTypeProfileEntry> weatherTypeProfileMap;

        public static WeatherManager Instance { get; private set; }


        /// <summary>
        /// This looks stupid, BUT
        /// this is of type (prefab) -> (instance)
        /// Okay yes it is still stupid
        /// ~Lars
        /// </summary>
        Dictionary<string, WeatherTypeProfile> profiles;

        private Camera cam;
        [SerializeField] private Light sunLight;

        public float lanternWeight;

        public string test;
        
        [SerializeField] private float blendSpeed = 0.3f;

        private void Awake()
        {
            //May only be one instance ofc
            if (Instance != null && Instance != this) Destroy(this); 
            else Instance = this; 
 
            cam = Camera.main;

            //build runtime dict
            profiles = new Dictionary<string, WeatherTypeProfile>();
            // foreach (var entry in weatherTypeProfileMap)
            //     profiles[entry.type] = entry.profile;

            foreach (var profile in GetComponentsInChildren<WeatherTypeProfile>(true))
            {
                Debug.Log($"What the fuck am I doing?: {profile}");
                if (profiles.TryGetValue(profile.Id, out var e)) //do not override if the default weather is already present
                    Debug.LogError($"[WeatherManager]: duplicate weather type profile entry: {profile.Id}");
                else
                    profiles[profile.Id] = profile;
            }

            foreach (var prof in profiles.Values)
            {
                prof.gameObject.SetActive(false);
            }




        }
        
        void Start()
        {
            //TODO: this is debug, and should be overwritten frame 1
            //SetWeatherType(RegionManager.Instance.defaultRegion.weatherType);
        }

        //Update to the camera's position for particles to render correctly
        void LateUpdate()
        {
            transform.position = cam.transform.position;
        }


        public void SetWeatherType(WeatherTypeProfile profile)
        {
            SetWeatherTypeBlend(profile, profile, 1f); //heheheheh this is real nasty, don't tell anyone
        }


        public void SetWeatherTypeBlend(WeatherTypeProfile a, WeatherTypeProfile b, float blend)
        {
            if(!(profiles.TryGetValue(a.Id, out var aProfile) && profiles.TryGetValue(b.Id, out var bProfile)))
            {
                Debug.LogError($"[WeatherManager]: WeatherType {a} or {b} is not in dictionary");
                return;
            }

            //Debug.Log($"Blending between weather: {a} to {b} with value {blend}");

            //dynamically turn off unused profiles
            foreach (var (type, profile) in profiles)
            {
                profile.gameObject.SetActive(type == a.Id || type == b.Id);
            }

            aProfile.SetBlend(1f-blend);
            bProfile.SetBlend(blend);

            lanternWeight = Mathf.Lerp(aProfile.lanternIntensity, bProfile.lanternIntensity, blend);

            BlendLightingData(aProfile, bProfile, blend);



        }

        public void SetRegionInfluences(Dictionary<Region, float> influences)
        {   
            //process regions to sum up the same weathertypes
            var weatherTypeProfiles = new Dictionary<string, float>();
            foreach (var (region, weight) in influences)
            {
                if (weatherTypeProfiles.TryGetValue(region.weatherTypeProfile.Id, out float existing))
                    weatherTypeProfiles[region.weatherTypeProfile.Id] = existing + weight;
                else
                    weatherTypeProfiles[region.weatherTypeProfile.Id] = weight;
            }

            //dynamically turn on and off (un)used profiles
            foreach (var (type, profile) in profiles)
                profile.gameObject.SetActive(weatherTypeProfiles.ContainsKey(type));

            foreach (var (type, weight) in weatherTypeProfiles)
                profiles[type].SetBlend(weight);

            BlendLightingData(weatherTypeProfiles);
            //TODO: lantern is not enabled

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
            DynamicGI.UpdateEnvironment(); //actually updates the lighting
        }


        //TODO: clean up
        private void BlendLightingData(Dictionary<string, float> influences)
        {
            float sunIntensity = 0f;
            Color sunColor = Color.black;
            float shadowStrength = 0f;
            Vector3 sunRotation = Vector3.zero;

            // //skybox
            float atmosphereThickness = 0f;
            Color skyTint = Color.black;
            Color groundTint = Color.black;
            float exposure = 0f;

            Color ambientColor = Color.black; // 0,0,0,0 — neutral starting point for summation


            foreach (var (id, weight) in influences)
            {
                if(!profiles.TryGetValue(id, out var profile))
                {
                    Debug.LogError($"[WeatherManager]: WeatherTypeProfile {id} is not in dictionary");
                    return;
                }

                sunIntensity     += profile.sunIntensity * weight;
                sunColor         += profile.sunColor * weight;
                shadowStrength   += profile.shadowStrength * weight;
                sunRotation      += profile.sunRotation * weight;

                ambientColor += profile.ambientColor * weight;

                atmosphereThickness += profile.skyBox.GetFloat("_AtmosphereThickness") * weight;
                skyTint             += (Color)(profile.skyBox.GetColor("_SkyTint")) * weight;
                groundTint          += (Color)(profile.skyBox.GetColor("_GroundColor")) * weight;
                exposure            += profile.skyBox.GetFloat("_Exposure") * weight;
            }

            //apply, lerped
            sunLight.intensity = Mathf.Lerp(sunLight.intensity, sunIntensity, blendSpeed);
            sunLight.color = Color.Lerp(sunLight.color, sunColor, blendSpeed);

            sunLight.transform.rotation = Quaternion.Slerp(
                sunLight.transform.rotation, 
                Quaternion.Euler(sunRotation), blendSpeed);

            sunLight.shadowStrength = Mathf.Lerp(sunLight.shadowStrength, shadowStrength, blendSpeed);


            // sunLight.intensity       = sunIntensity;
            // sunLight.color           = sunColor;
            // sunLight.shadowStrength  = shadowStrength;

            // sunLight.transform.rotation = Quaternion.Euler(sunRotation);

            //funny name for something NOT called that in the editor
            RenderSettings.ambientLight = ambientColor;
            Material skybox = RenderSettings.skybox;
            skybox.SetFloat("_AtmosphereThickness", atmosphereThickness);
            skybox.SetColor("_SkyTint", skyTint);
            skybox.SetColor("_GroundColor", groundTint);
            skybox.SetFloat("_Exposure", exposure);

            //no longer required
            // DynamicGI.UpdateEnvironment();
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

