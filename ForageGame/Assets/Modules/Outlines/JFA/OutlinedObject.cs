using System.Collections.Generic;
using Modules.Outlines;
using UnityEngine;

[ExecuteAlways]
public class OutlineObject : MonoBehaviour
{
    public static readonly List<OutlineObject> All = new();

    Renderer[] m_Renderers;
    public Renderer[] Renderers => m_Renderers;
    
    public OutlineInfo outlineInfo = new OutlineInfo();

    void OnEnable()
    {
        m_Renderers = GetComponentsInChildren<Renderer>();
        All.Add(this);
    }

    void OnDisable() => All.Remove(this);
}