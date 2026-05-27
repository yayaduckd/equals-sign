using UnityEngine;
using System.IO;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Project.Menus;
using System.Threading.Tasks;
using Project.SceneLoading;

[RequireComponent(typeof(Animator))]
public class ImageCutsceneController : MonoBehaviour
{
    // ------------ Cutscenes ------------

    [SerializeField] private Animator _animator;

    [SerializeField] private float _introDuration = 1f;
    [SerializeField] private float _outroDuration = 1f;

    void OnValidate()
    {
        _animator = GetComponent<Animator>();
    }

    public async Task PlayIntroSequence()
    {
        _animator.SetTrigger("Intro");
        await Task.Delay(Mathf.CeilToInt(_introDuration * 100));
    }

    public async Task PlayOutroSequence()
    {
        _animator.SetTrigger("Outro");
        await Task.Delay(Mathf.CeilToInt(_outroDuration * 100));
    }
}