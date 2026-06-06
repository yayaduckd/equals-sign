using System;
using System.Collections.Generic;
using DG.Tweening;
using Modules.Outlines;
using UnityEngine;

[ExecuteAlways]
public class OutlineObject : MonoBehaviour
{
    public static readonly List<OutlineObject> All = new();

    Renderer[] m_Renderers;
    public Renderer[] Renderers => m_Renderers;
    
    public OutlineInfo outlineInfo = new OutlineInfo();
    private OutlineInfo baseOutlineInfo;
    Tweener inoutTween;

    private static readonly Color defaultSuccesColor = new Color32(170, 227, 159, 255);
    private static readonly Color defaultFailureColor = new Color32(227, 168, 159, 255);

    void OnEnable()
    {
        m_Renderers = GetComponentsInChildren<Renderer>();
        All.Add(this);
    }

    private void Start()
    {
        baseOutlineInfo = outlineInfo.Copy();
    }

    void OnDisable() => All.Remove(this);

    public void AnimateIn(float toWidth, float duration = 0.2f)
    {
        inoutTween?.Kill();
        this.enabled = true;
        inoutTween = DOTween.To(() => outlineInfo.outlineWidth, x => outlineInfo.outlineWidth = x, toWidth, duration).SetEase(Ease.OutBack);
        inoutTween.Play();
    }

    public void AnimateOut(float duration = 0.1f)
    {
        inoutTween?.Kill();
        inoutTween = DOTween.To(() => outlineInfo.outlineWidth, x => outlineInfo.outlineWidth = x, 0f, duration).SetEase(Ease.InBack);
        inoutTween.OnComplete(() => this.enabled = false);
        inoutTween.Play();
    }

    public void AnimateSuccess(float duration = 0.15f, Color? color = null)
    {
        if(color == null) color = defaultSuccesColor;

        Sequence bounceSequence = DOTween.Sequence()
            .Append(DOTween.To(() => outlineInfo.outlineWidth, x => outlineInfo.outlineWidth = x, baseOutlineInfo.outlineWidth * 1.5f, duration / 2).SetEase(Ease.OutBack))
            .Append(DOTween.To(() => outlineInfo.outlineWidth, x => outlineInfo.outlineWidth = x, baseOutlineInfo.outlineWidth, duration / 2).SetEase(Ease.InBack));

        Sequence colorInoutSeq = DOTween.Sequence()
            .Append(DOTween.To(() => outlineInfo.outlineColor, x => outlineInfo.outlineColor = x, color.Value, duration / 2).SetEase(Ease.OutBack))
            .Append(DOTween.To(() => outlineInfo.outlineColor, x => outlineInfo.outlineColor = x, baseOutlineInfo.outlineColor, duration / 2).SetEase(Ease.InBack));

        colorInoutSeq.Play();
        bounceSequence.Play();
    }

    public void AnimateFailure(float duration = 0.15f, Color? color = null)
    {
        if (color == null) color = defaultFailureColor;

        Sequence colorInoutSeq = DOTween.Sequence()
            .Append(DOTween.To(() => outlineInfo.outlineColor, x => outlineInfo.outlineColor = x, color.Value, duration / 2).SetEase(Ease.OutBack))
            .Append(DOTween.To(() => outlineInfo.outlineColor, x => outlineInfo.outlineColor = x, baseOutlineInfo.outlineColor, duration / 2).SetEase(Ease.InBack));

        Sequence bounceSequence = DOTween.Sequence()
            .Append(DOTween.To(() => outlineInfo.outlineWidth, x => outlineInfo.outlineWidth = x, baseOutlineInfo.outlineWidth * 1.5f, duration / 2).SetEase(Ease.OutBack))
            .Append(DOTween.To(() => outlineInfo.outlineWidth, x => outlineInfo.outlineWidth = x, baseOutlineInfo.outlineWidth, duration / 2).SetEase(Ease.InBack));
        
        colorInoutSeq.Play();
        bounceSequence.Play();
    }
}