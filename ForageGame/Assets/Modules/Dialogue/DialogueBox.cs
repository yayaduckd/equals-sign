using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.Modules.Dialogue
{
    public class DialogueBox : MonoBehaviour
    {
        [SerializeField] Canvas canvas;
        [SerializeField] TextMeshProUGUI dialogueText;
        [SerializeField] AnimationCurve openCloseAnimation;
        [SerializeField] AnimationCurve newMessageAnimation;
        [SerializeField] float openCloseDuration;

        private void Start()
        {
            canvas.gameObject.SetActive(false);
            canvas.transform.localScale = Vector3.zero;
        }

        [ContextMenu("Animate In")]
        public async void AnimateIn()
        {
            canvas.gameObject.SetActive(true);
            canvas.transform.localScale = Vector3.zero;
            
            float elapsedTime = 0f;
            while (elapsedTime < openCloseDuration)
            {
                float scale = openCloseAnimation.Evaluate(elapsedTime / openCloseDuration);
                canvas.transform.localScale = new Vector3(scale, scale, scale);
                elapsedTime += Time.deltaTime;
                await System.Threading.Tasks.Task.Yield();
            }
        }

        [ContextMenu("Animate Out")]
        public async void AnimateOut()
        {
            float elapsedTime = 0f;
            while (elapsedTime < openCloseDuration)
            {
                float scale = openCloseAnimation.Evaluate(1 - (elapsedTime / openCloseDuration));
                canvas.transform.localScale = new Vector3(scale, scale, scale);
                elapsedTime += Time.deltaTime;
                await System.Threading.Tasks.Task.Yield();
            }
            canvas.transform.localScale = Vector3.zero;
            canvas.gameObject.SetActive(false);
        }

        public void SetText(string text)
        {
            dialogueText.text = text;
        }

        public async void AnimateNewMessage()
        {
            float elapsedTime = 0f;
            while (elapsedTime < newMessageAnimation[newMessageAnimation.length-1].time)
            {
                float scale = newMessageAnimation.Evaluate(elapsedTime);
                canvas.transform.localScale = scale * Vector3.one;
                elapsedTime += Time.deltaTime;
                await System.Threading.Tasks.Task.Yield();
            }
            canvas.transform.localScale = Vector3.one;
        }
    }
}