using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEditor.Rendering;
using UnityEngine;

public class CutoutController : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private LayerMask _cutoutLayers;

    [SerializeField] private Material baseMat;
    [SerializeField] private float _speed;
    [Header("Cutout Presets")]
    [SerializeField] private Vector2 standardOff = new(0, 2);
    [SerializeField] private Vector2 standardOn = new(1, 2);
    [SerializeField] private Vector2 caveOff = new(0, 2);
    [SerializeField] private Vector2 caveOn = new(1, 2);


    private bool _isActive = false;
    public enum CutoutMode { Standard, Cave }
    private CutoutMode _mode = CutoutMode.Standard;

    public void SetCutoutMode(CutoutMode cutoutMode)
    {
        if (_mode != cutoutMode)
        {
            _mode = cutoutMode;
            UpdateCutout();
        }
    }

    private void UpdateCutout()
    {
        if (_isActive)
        {
            if (_mode == CutoutMode.Cave)
                SetCutout(caveOn);
            else
                SetCutout(standardOn);
        }
        else
        {
            if (_mode == CutoutMode.Cave)
                SetCutout(caveOff);
            else
                SetCutout(standardOff);
        }
    }


    #region Obstruction detection

    void LateUpdate()
    {
        Vector3 vector = _playerTransform.position - _cameraTransform.position;

        if (_isActive != 0 < Physics.RaycastNonAlloc(_cameraTransform.position, vector, null, vector.magnitude, _cutoutLayers))
        {
            _isActive = !_isActive;
            UpdateCutout();
        }
    }

    #endregion

    #region Material adjustments

    public void SetCutout(Vector2 cutoutSize)
    {
        StopAllCoroutines();
        StartCoroutine(SetCutoutCoroutine(cutoutSize));
    }

    private IEnumerator SetCutoutCoroutine(Vector2 targetCutoutSize)
    {
        Vector2 initialCutoutSize = GetMaterialProperties();

        float t = 0;
        float relativeSpeed = _speed / (Vector2.Distance(initialCutoutSize, targetCutoutSize) + 0.01f); // +0.01f for div 0 protection

        while (t < 1)
        {
            SetMaterialProperties(Vector2.Lerp(initialCutoutSize, targetCutoutSize, t));
            t += Time.deltaTime * relativeSpeed;
            yield return 0;
        }

        SetMaterialProperties(targetCutoutSize);
    }

    private Vector2 GetMaterialProperties() =>
        new(baseMat.GetFloat("_Near_Radius"), baseMat.GetFloat("_Far_Radius"));

    private void SetMaterialProperties(Vector2 cutoutSize)
    {
        baseMat.SetFloat("_Near_Radius", cutoutSize.x);
        baseMat.SetFloat("_Far_Radius", cutoutSize.y);
    }

    void OnDestroy() => StopAllCoroutines();

    #endregion
}
