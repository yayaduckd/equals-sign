using UnityEngine;
using System.Collections;
using TDK.PlayerSystem;
using FMODUnity;

namespace Weather
{
    [RequireComponent(typeof(Light))]
    public class LightningBehaviour : WeatherBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float intervalMin = 4f;
        [SerializeField] private float intervalMax = 12f;

        [Header("Flash")]
        [SerializeField] private Light flashLight;
        [SerializeField] private float flashIntensity = 8f;
        [SerializeField] private AnimationCurve flashCurve; // sharp peak, quick decay
        [SerializeField] private float flashDuration = 0.15f;

        [Header("Thunder")]
        [SerializeField] private float thunderDelayMin = 0.3f;
        [SerializeField] private float thunderDelayMax = 2.5f;
        [SerializeField] private EventReference thunderEvent;
        [SerializeField] private float thunderScatterRadius = 40f;  // horizontal scatter in world units
        [SerializeField] private float thunderHeight = 60f;       // how high above the player

        private float _weight = 1f;

        private void OnEnable()
        {
            StartCoroutine(StrikeLoop());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (flashLight != null)
                flashLight.intensity = 0f;
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
                    StartCoroutine(DoFlash());
                    StartCoroutine(DoThunder());
                }
            }
        }

        private IEnumerator DoFlash()
        {
            float elapsed = 0f;
            Debug.Log("Lightning flash!");

            // optional: double flash for realism
            bool doubleFlash = Random.value > 0.5f;

            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;
                flashLight.intensity = flashIntensity * flashCurve.Evaluate(t);
                yield return null;
            }

            flashLight.intensity = 0f;

            if (doubleFlash)
            {
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                elapsed = 0f;

                while (elapsed < flashDuration * 0.6f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / (flashDuration * 0.6f);
                    flashLight.intensity = flashIntensity * 0.6f * flashCurve.Evaluate(t);
                    yield return null;
                }

                flashLight.intensity = 0f;
            }
        }

        private IEnumerator DoThunder()
        {
            yield return new WaitForSeconds(Random.Range(thunderDelayMin, thunderDelayMax));
            Debug.Log("Thunder!");
            yield return new WaitForSeconds(Random.Range(thunderDelayMin, thunderDelayMax));

            // scatter position in a large radius around the player, high up in the sky
            Vector3 playerPos = Player.Instance.transform.position;
            Vector2 randomCircle = Random.insideUnitCircle * thunderScatterRadius;
            Vector3 thunderPos = playerPos + new Vector3(randomCircle.x, thunderHeight, randomCircle.y);

            FMOD.Studio.EventInstance instance = RuntimeManager.CreateInstance(thunderEvent);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(thunderPos));
            instance.start();
            instance.release(); // release immediately, FMOD keeps it alive until sound finishes
        }
    }
}