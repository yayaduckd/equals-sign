using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class RegionTitle : MonoBehaviour
{
    [SerializeField] private TypewriterTextbox WelcomeText;
    [SerializeField] private TypewriterTextbox RegionNameText;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    [ContextMenu("Show region title")]
    public async void ShowRegionTitle()
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
