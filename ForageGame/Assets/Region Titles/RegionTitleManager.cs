using DG.Tweening;
using TMPro;
using UnityEngine;

namespace TDK.RegionTitles
{
    public class RegionTitleManager : MonoBehaviour
    {
        [SerializeField] private TypewriterTextbox WelcomeText;
        [SerializeField] private TypewriterTextbox RegionNameText;
        private float lastTriggerTime = 0f;

        public static RegionTitleManager Instance { get; private set; }
        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            Instance = this;
        }

        private void Start()
        {
            WelcomeText.SetMessage("Welcome to...");
            gameObject.SetActive(false);
        }

        public void TriggerRegionTitle(string titleText)
        {

            if (!gameObject.activeSelf && lastTriggerTime + 15 < Time.time) // 15 sec time delay
            {
                RegionNameText.SetMessage(titleText);
                ShowRegionTitle();
            }
            lastTriggerTime = Time.time;
        }

        [ContextMenu("Show region title")]
        private async void ShowRegionTitle()
        {
            gameObject.SetActive(true);
            WelcomeText.textbox.text = "";
            RegionNameText.textbox.text = "";
            await transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).From(0).AsyncWaitForCompletion();
            await WelcomeText.TypeText();
            await System.Threading.Tasks.Task.Delay(500);
            await RegionNameText.TypeText();
            await System.Threading.Tasks.Task.Delay(2000);
            await transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).AsyncWaitForCompletion();
            gameObject.SetActive(false);
        }
    }
}