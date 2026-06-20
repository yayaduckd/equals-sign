using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Weather
{

    public class WeatherManager : MonoBehaviour
    {


        public static WeatherManager Instance { get; private set; }


        /// <summary>
        /// This is populated automatically on Awake()
        /// the string values come from WeatherTypeProfile's 'id' field
        /// Which is what allows editor references to work here without the stupid enum
        /// ~Lars
        /// </summary>
        Dictionary<string, WeatherTypeProfile> profiles;

        private Camera cam;
        [SerializeField] private Light sunLight;

        public float lanternWeight;
        
        //How fast the lighting data blends / lerps, should be pretty slow
        [SerializeField] private float blendSpeed = 0.03f;

        [Header("Target Lighting Data")]
        [SerializeField] private float targetSunIntensity;
        [SerializeField] private Color targetSunColor;
        [SerializeField] private Vector3 targetSunRotation;
        [SerializeField] private float targetShadowStrength;

        [ColorUsage(true, true)]
        [SerializeField] private Color targetAmbientColor;

        private void Awake()
        {
            //May only be one instance ofc
            if (Instance != null && Instance != this) Destroy(this); 
            else Instance = this; 
 
            cam = Camera.main;

            //build runtime dict
            profiles = new Dictionary<string, WeatherTypeProfile>();

            foreach (var profile in GetComponentsInChildren<WeatherTypeProfile>(true))
            {
                // Debug.Log($"What the fuck am I doing?: {profile}");
                if (profiles.TryGetValue(profile.Id, out var e)) //do not override if the default weather is already present
                    Debug.LogError($"[WeatherManager]: duplicate weather type profile entry: {profile.Id}");
                else
                    profiles[profile.Id] = profile;
                
                //turn everything off by default
                profile.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            //update lighting data if needed
            if (Mathf.Abs(sunLight.intensity - targetSunIntensity) > 0.01f)
            {
                sunLight.intensity = Mathf.Lerp(sunLight.intensity, targetSunIntensity, blendSpeed);
                sunLight.color = Color.Lerp(sunLight.color, targetSunColor, blendSpeed);

                sunLight.transform.rotation = Quaternion.Slerp(
                    sunLight.transform.rotation, 
                    Quaternion.Euler(targetSunRotation), blendSpeed);

                sunLight.shadowStrength = Mathf.Lerp(sunLight.shadowStrength, targetShadowStrength, blendSpeed);
                RenderSettings.ambientLight = targetAmbientColor;
            }
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

        public void SetWeatherTypeInstant(WeatherTypeProfile w)
        {
            if(!profiles.TryGetValue(w.Id, out var profile))
            {
                Debug.LogError($"[WeatherManager]: WeatherTypeProfile {w} or is not in dictionary");
                return;
            }

            foreach (var (type, p) in profiles)
            {
                p.gameObject.SetActive(p.Id == profile.Id);
            }

            profile.SetBlend(1f);

            lanternWeight = profile.lanternIntensity;

            //lighting
            sunLight.intensity = profile.sunIntensity;
            targetSunIntensity = profile.sunIntensity;
            sunLight.color = profile.sunColor;
            targetSunColor = profile.sunColor;
            sunLight.transform.rotation = Quaternion.Euler(profile.sunRotation);
            targetSunRotation = profile.sunRotation;
            sunLight.shadowStrength = profile.shadowStrength;
            targetShadowStrength = profile.shadowStrength;
            RenderSettings.ambientLight = profile.ambientColor;
            targetAmbientColor = profile.ambientColor;

            RenderSettings.skybox = profile.skyBox;

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
            //these are applied lerped in update(), since they are jarring otherwise
            targetSunIntensity = 0f;
            targetSunColor = Color.black;
            targetShadowStrength = 0f;
            targetSunRotation = Vector3.zero;
            targetAmbientColor = Color.black; // 0,0,0,0 — neutral starting point for summation

            //skybox, applied immediately and may be removed later
            float atmosphereThickness = 0f;
            Color skyTint = Color.black;
            Color groundTint = Color.black;
            float exposure = 0f;


            foreach (var (id, weight) in influences)
            {
                if(!profiles.TryGetValue(id, out var profile))
                {
                    Debug.LogError($"[WeatherManager]: WeatherTypeProfile {id} is not in dictionary");
                    return;
                }

                targetSunIntensity     += profile.sunIntensity * weight;
                targetSunColor         += profile.sunColor * weight;
                targetShadowStrength   += profile.shadowStrength * weight;
                targetSunRotation      += profile.sunRotation * weight;

                targetAmbientColor += profile.ambientColor * weight;

                atmosphereThickness += profile.skyBox.GetFloat("_AtmosphereThickness") * weight;
                skyTint             += (Color)(profile.skyBox.GetColor("_SkyTint")) * weight;
                groundTint          += (Color)(profile.skyBox.GetColor("_GroundColor")) * weight;
                exposure            += profile.skyBox.GetFloat("_Exposure") * weight;
            }

            //apply, lerped
            // sunLight.intensity = Mathf.Lerp(sunLight.intensity, sunIntensity, blendSpeed);
            // sunLight.color = Color.Lerp(sunLight.color, sunColor, blendSpeed);

            // sunLight.transform.rotation = Quaternion.Slerp(
            //     sunLight.transform.rotation, 
            //     Quaternion.Euler(sunRotation), blendSpeed);

            // sunLight.shadowStrength = Mathf.Lerp(sunLight.shadowStrength, shadowStrength, blendSpeed);


            // sunLight.intensity       = sunIntensity;
            // sunLight.color           = sunColor;
            // sunLight.shadowStrength  = shadowStrength;

            // sunLight.transform.rotation = Quaternion.Euler(sunRotation);

            //funny name for something NOT called that in the editor
            //RenderSettings.ambientLight = ambientColor;


            Material skybox = RenderSettings.skybox;
            skybox.SetFloat("_AtmosphereThickness", atmosphereThickness);
            skybox.SetColor("_SkyTint", skyTint);
            skybox.SetColor("_GroundColor", groundTint);
            skybox.SetFloat("_Exposure", exposure);

            //no longer required
            // DynamicGI.UpdateEnvironment();
        }
    }
}

