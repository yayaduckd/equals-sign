using System.Collections;
using UnityEngine;

public class CutoutController : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private LayerMask _cutoutLayers;

    [SerializeField] private Material baseMat;
    [SerializeField] private float _speed;
    [Header("Cutout Presets")]
    [SerializeField] private Vector3 standardOff = new(0, 2, 2); // near radius, far radius, softness (near = camera, far = player)
    [SerializeField] private Vector3 standardOn = new(1, 2, 2);
    [SerializeField] private Vector3 caveOff = new(0, 2, 2);
    [SerializeField] private Vector3 caveOn = new(1, 2, 2);


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

    [SerializeField] private Vector3 _playerOffset = new(0, 0.2f, 0);


    // Matches the variable name in the Shader
    private static readonly int PosID = Shader.PropertyToID("_GlobalPlayerPos");

    void LateUpdate()
    {
        Shader.SetGlobalVector(PosID, _playerTransform.position); // Send the player's position to ALL shaders containing this variable

        Vector3 vector = _playerTransform.position + _playerOffset - _cameraTransform.position;
        if (_isActive != Physics.Raycast(_cameraTransform.position, vector, vector.magnitude, _cutoutLayers))
        {
            _isActive = !_isActive;
            UpdateCutout();
        }
    }

    #endregion

    #region Material adjustments

    public void SetCutout(Vector3 cutoutSize)
    {
        StopAllCoroutines();
        StartCoroutine(SetCutoutCoroutine(cutoutSize));
    }

    private IEnumerator SetCutoutCoroutine(Vector3 targetCutoutSize)
    {
        Vector3 initialCutoutSize = GetMaterialProperties();

        float t = 0;
        float relativeSpeed = _speed / (Vector3.Distance(initialCutoutSize, targetCutoutSize) + 0.01f); // +0.01f for div 0 protection

        while (t < 1)
        {
            SetMaterialProperties(Vector3.Lerp(initialCutoutSize, targetCutoutSize, Mathf.SmoothStep(0, 1, t)));
            t += Time.deltaTime * relativeSpeed;
            yield return 0;
        }

        SetMaterialProperties(targetCutoutSize);
    }

    private Vector3 GetMaterialProperties() =>
        new(baseMat.GetFloat("_Near_Radius"), baseMat.GetFloat("_Far_Radius"), baseMat.GetFloat("_Cutout_Smoothness"));

    private void SetMaterialProperties(Vector3 cutoutSize)
    {
        baseMat.SetFloat("_Near_Radius", cutoutSize.x);
        baseMat.SetFloat("_Far_Radius", cutoutSize.y);
        baseMat.SetFloat("_Cutout_Smoothness", cutoutSize.z);
    }

    void OnDestroy() => StopAllCoroutines();

    #endregion




}
