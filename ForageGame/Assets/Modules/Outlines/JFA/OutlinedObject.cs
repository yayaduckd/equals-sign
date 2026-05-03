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

    Tweener inoutTween;

    void OnEnable()
    {
        m_Renderers = GetComponentsInChildren<Renderer>();
        All.Add(this);
    }

    void OnDisable() => All.Remove(this);

    public void AnimateIn(float toWidth, float duration = 0.2f)
    {
        inoutTween?.Kill();
        this.enabled = true;
        inoutTween = DOTween.To(() => outlineInfo.outlineWidth, x => outlineInfo.outlineWidth = x, toWidth, duration).SetEase(Ease.OutBack);
        inoutTween.Play();
    }

    public void AnimateOut(float duration = 0.2f)
    {
        inoutTween?.Kill();
        inoutTween = DOTween.To(() => outlineInfo.outlineWidth, x => outlineInfo.outlineWidth = x, 0f, duration).SetEase(Ease.InBack);
        inoutTween.OnComplete(() => this.enabled = false);
        inoutTween.Play();
    }
}