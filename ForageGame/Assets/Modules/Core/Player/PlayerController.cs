using System.Linq;
using NUnit.Framework.Constraints;
using TDK.Physics3DSystem;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace TDK.PlayerSystem
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(VelocityDriver))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private LayerMask physicsColliders;
        public Rigidbody _rigidbody { get; private set; }
        private Animator animator;
        [SerializeField] private PlayerVisuals _visuals;
        [SerializeField] private VelocityDriver _velocityDriver;

        public UnityEvent onJump;
        public UnityEvent onSprint;
        public UnityEvent onAttack;
        public UnityEvent onMove;
        public UnityEvent onLand;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
        }

        #region Move & View

        private Vector3 _inputVector = Vector3.zero;
        public Vector3 InputVector
        {
            get => _inputVector;
            set
            {
                _inputVector = Vector3.ClampMagnitude(value, 1);

                if (value.magnitude > 0.1f)
                {
                    ViewDirection = _inputVector;
                    animator.SetBool("isMoving", true);
                    if (_vdTarget == VelocityDriverTarget.Input)
                        _velocityDriver.SetTargetDirection(InputVector);
                }
                else
                {
                    animator.SetBool("isMoving", false);
                    if (_vdTarget == VelocityDriverTarget.Input)
                        _velocityDriver.SetTargetDirection(new(0, 0, 0));
                }
            }
        }
        private Vector3 _viewDirection = Vector3.right;
        public Vector3 ViewDirection
        {
            get => _viewDirection;
            set
            {
                _viewDirection = value.normalized;
                _visuals.UpdateViewVisuals(ViewDirection);
                if (_vdTarget == VelocityDriverTarget.View)
                    _velocityDriver.SetTargetDirection(ViewDirection);
            }
        }

        #endregion

        #region Triggers

        [SerializeField] private LayerMask waterLayer;

        void OnTriggerEnter(Collider other)
        {
            if ((waterLayer.value & (1 << other.gameObject.layer)) != 0)
            {
                Debug.Log(animator.GetBool("isGrounded"));
                if (Mathf.Abs(_rigidbody.linearVelocity.y) > 0.1f)
                {
                    PlayerSounds.Instance.OnWaterEnter(true);
                }
                else PlayerSounds.Instance.OnWaterEnter(false);
                animator.SetBool("isSwimming", true);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if ((waterLayer.value & (1 << other.gameObject.layer)) != 0)
            {
                PlayerSounds.Instance.OnWaterLeave();
                animator.SetBool("isSwimming", false);
            }
        }

        public void TeleportTo(Vector3 position, bool maintainMomentum)
        {
            Vector3 v = _rigidbody.linearVelocity;
            _rigidbody.isKinematic = true;
            _rigidbody.position = position;
            _rigidbody.isKinematic = false;
            _rigidbody.linearVelocity = v;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();
            InputVector = new Vector3(moveInput.x, 0f, moveInput.y);
            onMove?.Invoke();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started
            && Player.Instance.playerData.sprintUnlocked
            && Player.Instance.energy.energy > 0.01f)
            {
                animator.SetBool("run", true);

                onSprint?.Invoke();
            }
            else if (context.canceled)
                animator.SetBool("run", false);

        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.started
            && Player.Instance.playerData.attackUnlocked
            && Player.Instance.energy.energy > Player.Instance.attackEnergy)
            {
                animator.SetBool("attack", true);
                onAttack?.Invoke();
            }
            else if (context.canceled) animator.SetBool("attack", false);
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                int wingLevel = Player.Instance.playerData.wingLevel;
                float energy = Player.Instance.energy.energy;
                if (wingLevel == 1 && energy > Player.Instance.hopEnergy)
                {
                    animator.SetBool("jump", true);
                    onJump?.Invoke();
                }
                else if (wingLevel >= 2 && energy > 0.01f)
                {
                    animator.SetBool("fly", true);
                    onJump?.Invoke();
                }
            }
            else if (context.canceled)
            {
                animator.SetBool("jump", false);
                animator.SetBool("fly", false);
            }
        }

        #endregion

        #region  Physics 

        private Vector3 _externalForce = Vector3.zero;
        public float LastGroundedHeight { get; private set; } = 0;

        private enum VelocityDriverTarget { Manual, Input, View }
        private VelocityDriverTarget _vdTarget = VelocityDriverTarget.Manual;

        void FixedUpdate()
        {
            _rigidbody.AddForce(_externalForce, ForceMode.Acceleration);
            UpdateGrounded();
        }


        private Collider[] groundColliders;
        private void UpdateGrounded()
        {
            groundColliders = Physics.OverlapBox(transform.position, new(0.1f, 0.1f, 0.1f), Quaternion.identity, physicsColliders);
            if (0 < groundColliders.Length && groundColliders.Any(c => !c.isTrigger))
            {
                if (!animator.GetBool("isGrounded"))
                {
                    onLand?.Invoke();
                }
                animator.SetBool("isGrounded", true);
                LastGroundedHeight = transform.position.y;
            }
            else
                animator.SetBool("isGrounded", false);
        }

        #endregion

        #region API

        public void Reset() // Resets the acceleration, gravity, and locomotion to their defaults.
        {
            _externalForce = Vector3.zero;
            SetGravity(true);
            ResetLocomotion();
        }
        public void SetGravity(bool useGravity) => _rigidbody.useGravity = useGravity;
        public void SetImpulse(Vector3 vector) => _rigidbody.AddForce(vector, ForceMode.VelocityChange);
        public void SetExternalForce(Vector3 vector) => _externalForce = vector;


        #region Locomotion API

        public void SetManualLocomotion(Vector3 targetVelocity, float acceleration)
        {
            _vdTarget = VelocityDriverTarget.Manual;
            SetLocomotion(InputVector, targetVelocity.magnitude, acceleration);
        }
        public void SetInputLocomotion(float speed, float acceleration)
        {
            _vdTarget = VelocityDriverTarget.Input;
            SetLocomotion(InputVector, speed, acceleration);
        }
        public void SetViewLocomotion(float speed, float acceleration)
        {
            _vdTarget = VelocityDriverTarget.View;
            SetLocomotion(ViewDirection, speed, acceleration);
        }
        private void SetLocomotion(Vector3 direction, float speed, float acceleration)
        {
            _velocityDriver.enabled = true;
            _velocityDriver.SetTargetDirection(direction);
            _velocityDriver.SetTargetSpeed(speed);
            _velocityDriver.SetAcceleration(acceleration);
            _velocityDriver.SetAffectMode(VelocityDriver.AffectedAxesMode.NormalPlane);
            _velocityDriver.SetAffectNormal(transform.up);
            _velocityDriver.SetNormalTracksTarget(false);
        }
        private void ResetLocomotion()
        {
            _velocityDriver.Reset();
            _velocityDriver.enabled = false;
        }

        #endregion

        #endregion
    }
}