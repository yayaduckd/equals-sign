using System;
using System.Collections.Generic;
using DG.Tweening;
using Modules.Outlines;
using UnityEngine;

[ExecuteAlways]
public class OutlineObject : MonoBehaviour
{
    public static readonly List<OutlineObject> All = new();
    [SerializeField] private GameObject _visuals = null;
    Renderer[] m_Renderers;
    public Renderer[] Renderers => m_Renderers;

    public OutlineInfo outlineInfo = new();
    private OutlineInfo baseOutlineInfo;
    Sequence _seq;

    const float animationDuration = 0.15f;

    private readonly Color _pulseColor = new Color32(170, 227, 159, 255);

    void OnValidate()
    {
        if (_visuals == null) _visuals = gameObject;
        m_Renderers = _visuals.GetComponentsInChildren<Renderer>();
    }

    void OnEnable()
    {
        if (_visuals == null) _visuals = gameObject;
        m_Renderers = _visuals.GetComponentsInChildren<Renderer>();
        All.Add(this);
    }

    private void Start()
    {
        baseOutlineInfo = outlineInfo.Copy();
    }

    void OnDisable() => All.Remove(this);

    void OnDestroy()
    {
        All.Remove(this);
        _seq?.Kill();
    }

    public void AnimateIn()
    {
        _seq?.Kill();
        this.enabled = true;
        _seq = DOTween.Sequence()
            .Append(DOTween.To(() => outlineInfo.outlineWidth, x => outlineInfo.outlineWidth = x, baseOutlineInfo.outlineWidth, animationDuration)).SetEase(Ease.OutBack)
            .Play();
    }

    public void AnimateOut()
    {
        _seq?.Kill();
        _seq = DOTween.Sequence()
            .Append(DOTween.To(() => outlineInfo.outlineWidth, x => outlineInfo.outlineWidth = x, 0f, animationDuration)).SetEase(Ease.InBack)
            .OnComplete(() => this.enabled = false)
            .Play();
    }

    public void AnimatePulse()
    {
        _seq?.Kill();
        _seq = DOTween.Sequence()
            .Append(DOTween.To(() => outlineInfo.outlineWidth, x => outlineInfo.outlineWidth = x, baseOutlineInfo.outlineWidth * 1.5f, animationDuration / 2).SetEase(Ease.OutBack))
            .Join(DOTween.To(() => outlineInfo.outlineColor, x => outlineInfo.outlineColor = x, _pulseColor, animationDuration / 2).SetEase(Ease.OutBack))
            .Append(DOTween.To(() => outlineInfo.outlineWidth, x => outlineInfo.outlineWidth = x, baseOutlineInfo.outlineWidth, animationDuration / 2).SetEase(Ease.InBack))
            .Join(DOTween.To(() => outlineInfo.outlineColor, x => outlineInfo.outlineColor = x, baseOutlineInfo.outlineColor, animationDuration / 2).SetEase(Ease.InBack))
            .Play();
    }
}