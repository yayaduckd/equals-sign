using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFlipForwarder : MonoBehaviour
{
    static readonly int FlipX = Shader.PropertyToID("_FlipX");
    static readonly int FlipY = Shader.PropertyToID("_FlipY");

    SpriteRenderer _sr;
    MaterialPropertyBlock _mpb;
    bool _lastFlipX, _lastFlipY;

    void OnValidate()
    {
        _sr = GetComponent<SpriteRenderer>();
        _mpb = new MaterialPropertyBlock();
    }

    void LateUpdate()
    {
        if (_sr.flipX == _lastFlipX && _sr.flipY == _lastFlipY) return;

        _lastFlipX = _sr.flipX;
        _lastFlipY = _sr.flipY;

        _sr.GetPropertyBlock(_mpb);
        _mpb.SetFloat(FlipX, _sr.flipX ? -1f : 1f);
        _mpb.SetFloat(FlipY, _sr.flipY ? -1f : 1f);
        _sr.SetPropertyBlock(_mpb);
    }
}