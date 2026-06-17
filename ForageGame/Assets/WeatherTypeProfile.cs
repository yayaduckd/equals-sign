using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;


namespace Weather
{
    /// <summary>
    /// One type of weather, holds lighting data to be blended by the shared WeatherManager,
    /// and manages children particle emitters and possible extra functionality, like lightning or the lantern.
    /// 
    /// ~Lars
    /// </summary>
    [RequireComponent(typeof(Volume))]
    public class WeatherTypeProfile : MonoBehaviour
    {
        public WeatherType weatherType;
        
        [Header("Lighting Data")]
        public float sunIntensity;
        public Color sunColor;
        public Vector3 sunRotation;
        public float shadowStrength;

        public Material skyBox; //yeah this is gonna be f u n

        ///Is now what drives ambient lighting, not the skybox.
        ///Using the skybox is very heavy for blending purposes
        ///So, we use HDR colors to be able to use intensity.
        /// These are about log_2 (x) compared to the previous multipliers
        /// ~Lars
        [ColorUsage(true, true)]
        public Color ambientColor;

        public float lanternIntensity;

        private Volume volume;

        private Dictionary<ParticleSystem, float> particleEmitters;

        private WeatherBehaviour[] _behaviours; //other behavior, such as lightning or lantern

        private void Awake()
        {
            volume = GetComponent<Volume>();

            particleEmitters= new Dictionary<ParticleSystem, float>();

            foreach (var emitter in GetComponentsInChildren<ParticleSystem>())
            {
                particleEmitters[emitter] = emitter.emission.rateOverTime.constant;
            }

            _behaviours = GetComponentsInChildren<WeatherBehaviour>();
        }
        

        /// <summary>
        /// Called by the WeatherManager, to have this figure out how to set its particle effects and possible extra stuff 
        /// (i.e., lantern or thunder)
        /// 
        /// lighting data is just polled by the WeatherManager
        /// </summary>
        /// <param name="val"></param>
        public void SetBlend(float blend)
        {
            volume.weight = blend;

            foreach((ParticleSystem emitter, float rate) in particleEmitters)
            {
                var em = emitter.emission;
                em.rateOverTime = blend * rate;
            } 

            //TODO: special behavior like the lantern or thunder
            foreach(var behavior in _behaviours)
            {
                behavior.SetBlend(blend);
            }
        }
    }
}

