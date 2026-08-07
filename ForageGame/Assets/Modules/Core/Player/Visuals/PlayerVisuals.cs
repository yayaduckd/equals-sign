using System;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.Events;

namespace TDK.PlayerSystem
{
    [RequireComponent(typeof(SpriteLibrary))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerVisuals : MonoBehaviour
    {
        [System.Serializable]
        public class DuckOrientationGroup
        {
            public SpriteLibraryAsset FrontLeft;
            public SpriteLibraryAsset FrontRight;
            public SpriteLibraryAsset BackLeft;
            public SpriteLibraryAsset BackRight;

            public SpriteLibraryAsset GetSpriteLibrary(bool isFacingLeft, bool isFacingFront)
            {
                if (isFacingLeft)
                {
                    if (isFacingFront) return FrontLeft;
                    else return BackLeft;
                }
                else
                {
                    if (isFacingFront) return FrontRight;
                    else return BackRight;
                }
            }
        }

        [SerializeField] private DuckOrientationGroup[] _duckOrientationGroup = new DuckOrientationGroup[4];
        [SerializeField] private SpriteLibrary spriteLibrary;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private bool _isFacingLeft = true;
        public bool IsFacingLeft => _isFacingLeft;
        private bool _isFacingFront = true;
        public bool IsFacingFront => _isFacingFront;
        private int _wingLevel = 0;

        public UnityEvent<bool, bool> onFacingDirectionChanged; // (L/R, F/B)

        public void UpdateVisuals(int wingLevel, Vector3 viewDir)
        {
            bool wingChanged = SetWingState(wingLevel);
            bool xChanged = SetViewStateX(viewDir);
            bool zChanged = SetViewStateZ(viewDir);
            if (wingChanged || xChanged || zChanged)
                ApplyVisuals();
        }

        public void UpdateWingVisuals(int wingLevel)
        {
            if (SetWingState(wingLevel))
                ApplyVisuals();
        }

        public void UpdateViewVisuals(Vector3 viewDir)
        {
            bool xChanged = SetViewStateX(viewDir);
            bool zChanged = SetViewStateZ(viewDir);
            if (xChanged || zChanged)
            {
                onFacingDirectionChanged?.Invoke(_isFacingLeft, _isFacingFront);
                // Debug.Log($"[PlayerVisuals]: Facing direction changed: Left={_isFacingLeft}, Front={_isFacingFront}");
                ApplyVisuals();
            }
        }

        void Start()
        {
            //so the lantern can initialize when loading in cave
            onFacingDirectionChanged?.Invoke(_isFacingLeft, _isFacingFront);
        }

        private bool SetWingState(int wingLevel) // returns true if anything changed
        {
            if (_wingLevel == wingLevel) return false;
            _wingLevel = wingLevel;

            if (_wingLevel < 0 || _wingLevel >= _duckOrientationGroup.Length)
            {
                Debug.LogError($"Wing level {_wingLevel} is out of bounds for the duck orientation group array.");
                _wingLevel = Mathf.Clamp(_wingLevel, 0, _duckOrientationGroup.Length - 1);
            }

            return true;
        }

        private bool SetViewStateX(Vector3 viewDir)
        {
            bool isFacingLeft = _isFacingLeft;
            if (viewDir.x < 0) isFacingLeft = true;
            else if (viewDir.x > 0) isFacingLeft = false;
            // else if == 0, we do nothing as not to snap the player around
            if (_isFacingLeft == isFacingLeft) return false;
            _isFacingLeft = isFacingLeft;
            return true;
        }

        private bool SetViewStateZ(Vector3 viewDir)
        {
            bool isFacingFront = viewDir.z <= 0;
            // We snap the player to face forward if given the option
            if (_isFacingFront == isFacingFront) return false;
            _isFacingFront = isFacingFront;
            return true;
        }

        private void ApplyVisuals()
        {
            spriteLibrary.spriteLibraryAsset = _duckOrientationGroup[_wingLevel].GetSpriteLibrary(_isFacingLeft, _isFacingFront);
            spriteRenderer.flipX = !_isFacingLeft;
            //var scale = transform.parent.localScale;
            //transform.parent.localScale = new Vector3(_isFacingLeft ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x), scale.y, scale.z);
        }
    }
}