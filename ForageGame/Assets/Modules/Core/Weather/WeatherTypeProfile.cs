using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
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
        public string Id => gameObject.name;
        // public WeatherType weatherType;

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

        // particlesystem -> (base rate over time, base rate over distance)
        private Dictionary<ParticleSystem, (float, float)> particleSystems;

        private WeatherBehaviour[] _behaviours; //other behavior, such as lightning or lantern

        private void Awake()
        {
            volume = GetComponent<Volume>();

            particleSystems= new Dictionary<ParticleSystem, (float, float)>();

            foreach (var ps in GetComponentsInChildren<ParticleSystem>())
            {
                var em = ps.emission;
                particleSystems[ps] = (em.rateOverTime.constant, em.rateOverDistance.constant);
            }

            _behaviours = GetComponentsInChildren<WeatherBehaviour>();
        }

        void OnEnable()
        {
            //Debug.Log($"[WeatherTypeProfile: {gameObject.name}]: ensabled!");
            //enable the particles again
            foreach (var ps in particleSystems.Keys)
                ps.Play();

            //turn on the behaviors
            foreach(var behavior in _behaviours)
            {
               behavior.enabled = true; 
            }

            /// Okay so Unity is le stupid
            /// When I set a volume's weight to 0, Unity apparently just cashes that it doesn't need to apply this profile
            /// The thing is that specifically for vignette and fog it will never actually check if the weight is still 0
            /// So I have to manually wake it up again in order to have the vignette and fog again
            /// 
            /// so fun :) 
            /// ~Lars
            volume.enabled = true;
        }

        void OnDisable()
        {
            //Debug.Log($"[WeatherTypeProfile: {gameObject.name}]: disabled!");

            volume.weight = 0f; //to be sure
            volume.enabled = false;

            //turn off the particle emitters, and wait for their particles to die and stop them fully (resources)
            foreach (var ps in particleSystems.Keys)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                StartCoroutine(WaitForParticlesAndStop());
            }

            //turn off the behaviors too
            foreach(var behavior in _behaviours)
            {
               behavior.enabled = false; 
            }
        }

        private IEnumerator WaitForParticlesAndStop()
        {
            // Wait until all particles have naturally died
            bool anyAlive = true;
            while (anyAlive)
            {
                anyAlive = false;
                foreach (var ps in particleSystems.Keys)
                {
                    if (ps.particleCount > 0) { anyAlive = true; break; }
                }
                yield return null;
            }
            foreach (var ps in particleSystems.Keys)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            //Debug.Log($"[WeatherTypeProfile: {gameObject.name}]: all particles died, clearing!");
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

            foreach((ParticleSystem emitter, (float timeRate, float distanceRate)) in particleSystems)
            {
                var em = emitter.emission;
                em.rateOverTime = blend * timeRate;
                em.rateOverDistance = blend * distanceRate;
            } 

            //TODO: special behavior like the lantern or thunder
            foreach(var behavior in _behaviours)
            {
                behavior.SetBlend(blend);
            }
        }
    }
}

