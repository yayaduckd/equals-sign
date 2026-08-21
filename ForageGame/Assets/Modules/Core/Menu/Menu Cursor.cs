using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UICursor : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float _positionSpeed = 15f;
    [SerializeField] private float _sizeSpeed = 15f;

    [Header("Padding")]
    [SerializeField] private Vector2 _padding = Vector2.zero;

    private RectTransform _cursorRect;
    private RectTransform _targetRect;

    private readonly Vector3[] _corners = new Vector3[4];

    private void Awake()
    {
        _cursorRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        _targetRect = EventSystem.current.currentSelectedGameObject?.GetComponent<RectTransform>();

        if (_targetRect == null)
            return;

        // Get the selected UI element's world-space corners.
        _targetRect.GetWorldCorners(_corners);

        RectTransform parent = _cursorRect.parent as RectTransform;

        if (parent == null)
            return;

        // Convert corners into the cursor parent's local space.
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        for (int i = 0; i < _corners.Length; i++)
        {
            Vector2 localCorner = parent.InverseTransformPoint(_corners[i]);

            min = Vector2.Min(min, localCorner);
            max = Vector2.Max(max, localCorner);
        }

        // Smoothly move the cursor.
        _cursorRect.anchoredPosition = Vector2.Lerp(
            _cursorRect.anchoredPosition,
            (min + max) * 0.5f,
            1f - Mathf.Exp(-_positionSpeed * Time.unscaledDeltaTime)
        );

        // Smoothly resize the cursor.
        _cursorRect.sizeDelta = Vector2.Lerp(
            _cursorRect.sizeDelta,
            (max - min) + _padding * 2f,
            1f - Mathf.Exp(-_sizeSpeed * Time.unscaledDeltaTime)
        );
    }
}