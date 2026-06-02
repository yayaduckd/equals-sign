using UnityEngine;
using System.Collections;

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
        [SerializeField] private FMODUnity.EventReference thunderEvent;

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
            //RuntimeManager.PlayOneShot(thunderEvent);
        }
    }
}