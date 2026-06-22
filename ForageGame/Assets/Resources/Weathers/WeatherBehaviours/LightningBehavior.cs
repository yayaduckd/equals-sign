using UnityEngine;
using System.Collections;
using TDK.PlayerSystem;
using FMODUnity;
using AudioIntegration;

namespace Weather
{
    [RequireComponent(typeof(Light))]
    public class LightningBehaviour : WeatherBehaviour, IManagedAudioSource
    {
        [Header("Timing")]
        [SerializeField] private float intervalMin = 6f;
        [SerializeField] private float intervalMax = 12f;

        [Header("Flash")]
        [SerializeField] private Light flashLight;
        [SerializeField] private float flashIntensity = 8f;
        [SerializeField] private AnimationCurve flashCurve; // sharp peak, quick decay
        [SerializeField] private float flashDuration = 0.15f;

        [Header("Near Strike")]
        [SerializeField] private float nearRadiusMin = 15f;
        [SerializeField] private float nearRadiusMax = 50f;

        [Header("Far Strike")]
        [SerializeField] private float farRadiusMin = 80f;
        [SerializeField] private float farRadiusMax = 200f;
        /// <summary>
        /// How much to reduce flash intensity for far strikes. 0 = no reduction, 1 = fully dark.
        /// </summary>
        [SerializeField] [Range(0f, 1f)] private float farIntensityFalloff = 0.65f;

        [Header("Thunder")]
        [SerializeField] private float thunderDelayMin = 0.3f;

        [SerializeField] private float thunderDelayMid = 1.5f;
        [SerializeField] private float thunderDelayMax = 2.5f;
        [SerializeField] private EventReference thunderEvent;
        [SerializeField] private float thunderHeight = 60f;       // how high above the player

        [SerializeField] private string thunderTypeParam = "ThunderType";

        FMOD.Studio.EventInstance _instance;

        private float _weight = 1f;

        private struct LightningStrike
        {
            public Vector3 worldPosition;  // origin point shared by both light and sound
            public bool isFar;
            public float normalizedDistance; // 0 = closest possible, 1 = furthest possible
        }

        private void OnEnable()
        {
            StartCoroutine(StrikeLoop());
            AudioManager.Instance.Register(this);
        }
        private void OnDisable()
        {
            StopAllCoroutines();
            if (flashLight != null)
                flashLight.intensity = 0f;
            AudioManager.Instance.Unregister(this);
        }

        public override void SetBlend(float value)
        {
            _weight = value;
        }

        private IEnumerator StrikeLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(intervalMin, intervalMax));
 
                if (_weight > 0.05f)
                {
                    LightningStrike strike = GenerateStrike();
                    StartCoroutine(DoFlash(strike));
                    StartCoroutine(DoThunder(strike));
                }
            }
        }

        /// <summary>
        /// Determines whether this is a near or far strike, picks a random origin point
        /// in the appropriate radius ring, rotates the directional light to face the player
        /// from that origin, and returns the shared strike descriptor.
        /// </summary>
        private LightningStrike GenerateStrike()
        {
            bool isFar = Random.value > 0.5f;
 
            float radius = isFar
                ? Random.Range(farRadiusMin, farRadiusMax)
                : Random.Range(nearRadiusMin, nearRadiusMax);
 
            Vector3 playerPos = Player.Instance.transform.position;
 
            // Place the strike at a random angle around the player at the chosen radius
            Vector2 circle = Random.insideUnitCircle.normalized * radius;
            Vector3 strikePos = playerPos + new Vector3(circle.x, thunderHeight, circle.y);
 
            // Rotate the directional light so it appears to shine from the strike toward the player.
            // Close strikes will be steep (nearly vertical); far ones will be shallow (near horizontal).
            Vector3 lightDir = (playerPos - strikePos).normalized;
            flashLight.transform.rotation = Quaternion.LookRotation(lightDir);
 
            float normalizedDist = Mathf.InverseLerp(nearRadiusMin, farRadiusMax, radius);

            flashLight.shadowStrength = isFar? .3f: .7f;
            // Debug.Log($"new strike generated: pos: {strikePos}, isFar: {isFar}");
            return new LightningStrike
            {
                worldPosition = strikePos,
                isFar = isFar,
                normalizedDistance = normalizedDist
            };
        }

        private IEnumerator DoFlash(LightningStrike strike)
        {
            // Far strikes appear dimmer
            float distanceFactor = 1f - (strike.normalizedDistance * farIntensityFalloff);
            float peakIntensity  = flashIntensity * distanceFactor * _weight;
 
            bool doubleFlash = Random.value > 0.5f;
 
            yield return RunFlash(peakIntensity, flashDuration);
 
            if (doubleFlash)
            {
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                yield return RunFlash(peakIntensity * 0.6f, flashDuration * 0.6f);
            }
        }

        /// <summary>
        /// Animates the flash light from 0 to peak and back over <paramref name="duration"/> seconds
        /// using the assigned flashCurve, then resets intensity to 0.
        /// </summary>
        private IEnumerator RunFlash(float peakIntensity, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                flashLight.intensity = peakIntensity * flashCurve.Evaluate(elapsed / duration);
                yield return null;
            }
            flashLight.intensity = 0f;
        }


        private IEnumerator DoThunder(LightningStrike strike)
        {
            // Delay between the flash and the thunder clap (simulates distance)
            if(strike.isFar) yield return new WaitForSeconds(Random.Range(thunderDelayMid, thunderDelayMax));
            else yield return new WaitForSeconds(Random.Range(thunderDelayMin, thunderDelayMid));
 
            _instance = RuntimeManager.CreateInstance(thunderEvent);
 
            // Place the 3D audio at the same world position the light came from
            _instance.set3DAttributes(RuntimeUtils.To3DAttributes(strike.worldPosition));
 
            // Drive the FMOD parameter so the event picks a near or far sample
            _instance.setParameterByName(thunderTypeParam, strike.isFar ? 1f : 0f);
 
            _instance.start();
            _instance.release(); // FMOD keeps the instance alive until the one-shot finishes
        }

        /// <summary>
        /// Inherited from IManagedAudioSource
        /// </summary>
        public void StopAndRelease()
        {
            StopAllCoroutines();
            if(_instance.isValid()) //will only do something if it hasn't been stopped yet
            {
                _instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); 
                _instance.release(); //just to be sure
            }
            
        }
    }
}