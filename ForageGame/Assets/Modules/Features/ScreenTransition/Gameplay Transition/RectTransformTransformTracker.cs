using UnityEngine;

namespace TDK.UISystem
{
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformTransformTracker : MonoBehaviour // wow; what an utterly garbage name :|
    {
        [Header("References")]
        [SerializeField] private Transform _target;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Camera _camera; // leave as null to use Camera.main
        // [Header("Settings")]
        // [SerializeField] private bool _clampToCanvas = false; // TODO

        public void Update()
        {
            UpdateRectTransform();
        }

        Vector2 screenPosition;
        Vector2 canvasPosition;
        public void UpdateRectTransform()
        {
            if (!ValidateReferences()) return;

            screenPosition = _camera.WorldToScreenPoint(_target.position);

            // Convert screen position to canvas position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.GetComponent<RectTransform>(),
                screenPosition,
                _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _camera,
                out canvasPosition
            );

            // Set the anchored position
            _rectTransform.anchoredPosition = canvasPosition;
        }

        private bool ValidateReferences()
        {
            if (_camera == null)
            {
                if (Camera.main == null)
                {
                    Debug.LogError("Target Camera is not assigned!", this);
                    return false;
                }
                else
                {
                    _camera = Camera.main;
                }
            }

            if (_target == null)
            {
                Debug.LogError("Target is not assigned!", this);
                return false;
            }

            if (_canvas == null)
            {
                Debug.LogError("Canvas is not assigned!", this);
                return false;
            }

            if (_rectTransform == null)
            {
                Debug.LogError("RectTransform is not assigned!", this);
                return false;
            }

            return true;
        }
    }
}