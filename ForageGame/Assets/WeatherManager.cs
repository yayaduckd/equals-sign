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
                profile.enabled = false;
                //profile.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            //update lighting data if needed
            if (Mathf.Abs(sunLight.intensity - targetSunIntensity) > 0.01f || Quaternion.Angle(sunLight.transform.rotation, Quaternion.Euler(targetSunRotation)) > 0.01f)
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

        /// <summary>
        /// Instant version that snaps the weather
        /// Can give jarring transitions, so only use on startup
        /// </summary>
        /// <param name="w"></param>
        public void SetWeatherTypeInstant(WeatherTypeProfile w)
        {
            SetWeatherType(w);
            SnapWeatherToTarget();
        }

        public void SetWeatherType(WeatherTypeProfile w)
        {
            if(!profiles.TryGetValue(w.Id, out var profile))
            {
                Debug.LogError($"[WeatherManager]: WeatherTypeProfile {w} or is not in dictionary");
                return;
            }

            foreach (var (type, p) in profiles)
            {
                //p.gameObject.SetActive(p.Id == profile.Id);
                p.enabled = p.Id == profile.Id;
            }

            profile.SetBlend(1f);

            lanternWeight = profile.lanternIntensity;

            //lighting
            targetSunIntensity = profile.sunIntensity;
            targetSunColor = profile.sunColor;
            targetSunRotation = profile.sunRotation;
            targetShadowStrength = profile.shadowStrength;
            targetAmbientColor = profile.ambientColor;

            RenderSettings.skybox = profile.skyBox;
        }

        /// <summary>
        /// As it says, snaps the actual weather to the target values.
        /// Mostly to be used on startup or maybe cutscenes
        /// </summary>
        private void SnapWeatherToTarget()
        {
            sunLight.intensity = targetSunIntensity;
            sunLight.color = targetSunColor;
            sunLight.transform.rotation = Quaternion.Euler(targetSunRotation);
            sunLight.shadowStrength = targetShadowStrength;
            RenderSettings.ambientLight = targetAmbientColor;
        }

        public void SetRegionInfluencesInstant(Dictionary<string, float> influences)
        {
            SetRegionInfluences(influences);
            SnapWeatherToTarget();
        }

        public void SetRegionInfluences(Dictionary<string, float> influences)
        {   
            //process regions to sum up the same weathertypes
            // var weatherTypeProfiles = new Dictionary<string, float>();
            // foreach (var (region, weight) in influences)
            // {
            //     if (weatherTypeProfiles.TryGetValue(region.weatherTypeProfile.Id, out float existing))
            //         weatherTypeProfiles[region.weatherTypeProfile.Id] = existing + weight;
            //     else
            //         weatherTypeProfiles[region.weatherTypeProfile.Id] = weight;
            // }

            //dynamically turn on and off (un)used profiles
            foreach (var (type, profile) in profiles)
                //profile.gameObject.SetActive(weatherTypeProfiles.ContainsKey(type));
                profile.enabled = influences.ContainsKey(type);

            foreach (var (type, weight) in influences)
                profiles[type].SetBlend(weight);

            BlendLightingData(influences);
            //TODO: lantern is not enabled

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

            //TODO: Skyboxes are never really visible and are no longer used for ambient lighting, remove?
            Material skybox = RenderSettings.skybox;
            skybox.SetFloat("_AtmosphereThickness", atmosphereThickness);
            skybox.SetColor("_SkyTint", skyTint);
            skybox.SetColor("_GroundColor", groundTint);
            skybox.SetFloat("_Exposure", exposure);
        }
    }
}

