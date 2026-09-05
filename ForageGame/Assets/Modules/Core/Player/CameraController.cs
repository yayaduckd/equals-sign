using UnityEngine;
using TDK.PlayerSystem;

namespace TDK.CameraSystem
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target Tracking (Orbital)")]
        public Transform _viewingTarget;
        public Vector3 _viewingTargetOffset = Vector3.zero;
        public float _translationalSpeed = 1;
        public float _orbitalSpeed = 1;
        public float _targetRotation = 30;
        public float _targetRadius = 1;

        [Header("Player Tracking Mode")]
        private bool _playerTrackingMode = true;
        public float playerVelocityWeight = 1; // how much should the camera care about your velocity 
        public float playerDirectionWeight = 1; // how much should the camera care about the firection you are facing

        void Start()
        {
            SetPlayerTarget();
            TeleportToTarget();
        }

        public void SetPlayerTarget(float targetRadius = 12, float targetRotation = 32)
        {
            _viewingTarget = Player.Instance.transform;
            _viewingTargetOffset = new(0, 0.5f, 0);
            _playerTrackingMode = true;
            _targetRotation = targetRotation;
            _targetRadius = targetRadius;
        }

        public void SetTarget(Transform viewingTarget, Vector3 viewingTargetOffset, float targetRotation = 30, float targetRadius = 30, bool playerTrackingMode = false)
        {
            _viewingTarget = viewingTarget;
            _viewingTargetOffset = viewingTargetOffset;
            _targetRotation = targetRotation;
            _targetRadius = targetRadius;
            _playerTrackingMode = playerTrackingMode;
        }

        public void TeleportToTarget()
        {
            _orbitalRotation = Quaternion.Euler(_targetRotation, 0, 0);
            _orbitalPosition = _orbitalRotation * Vector3.forward * _targetRadius * (-1); // prior translational position: this is the anchor from which we "orbit";
            _translationPosition = _viewingTarget == null ? Vector3.zero : _viewingTarget.position;

            transform.SetPositionAndRotation(
                _orbitalPosition + _translationPosition,
                _orbitalRotation
                );
        }

        private Vector3 _targetTranslationPosition = Vector3.zero;
        private Vector3 _translationPosition = Vector3.zero;
        private Vector3 _orbitalPosition = Vector3.zero;
        private Quaternion _orbitalRotation = Quaternion.identity;
        void LateUpdate()
        {
            _orbitalRotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(_targetRotation, 0, 0), 1f - Mathf.Exp(-_orbitalSpeed * Time.unscaledDeltaTime));
            _orbitalPosition = _orbitalRotation * Vector3.forward * Mathf.Lerp(Vector3.Distance(transform.position, _translationPosition), _targetRadius, 1f - Mathf.Exp(-_orbitalSpeed * Time.unscaledDeltaTime)) * (-1); // prior translational position: this is the anchor from which we "orbit";

            _targetTranslationPosition = _viewingTarget == null ? Vector3.zero : _viewingTarget.position;
            _targetTranslationPosition += _viewingTargetOffset;
            if (_playerTrackingMode)
            {
                _targetTranslationPosition += playerVelocityWeight * Player.Instance.playerController._rigidbody.linearVelocity;
                _targetTranslationPosition += playerDirectionWeight * Player.Instance.playerController.ViewDirection;
            }
            _translationPosition = Vector3.Lerp(_translationPosition, _targetTranslationPosition, 1f - Mathf.Exp(-_translationalSpeed * Time.unscaledDeltaTime));

            transform.SetPositionAndRotation(
                _orbitalPosition + _translationPosition,
                _orbitalRotation
                );
        }
    }
}