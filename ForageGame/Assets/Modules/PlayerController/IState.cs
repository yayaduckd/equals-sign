

using UnityEngine;

/// <summary>
/// State class 
/// </summary>
public interface IState {
    /// <summary>
    /// Called once when transitioning into this state.
    /// </summary>
    public void Enter(DuckController duck);

    /// <summary>
    /// Called every fixed framerate frame while in this state.
    /// </summary>
    public void FixedUpdate();

    public Vector2 CalculateVelocity();

    public Quaternion CalculateRotation();


    // public void NextState();

    /// <summary>
    /// Called once when transitioning out of this state.
    /// </summary>
    public void Exit();
}


    class IdleState : IState {
        public DuckController duck;

        public void Enter(DuckController duckController) {
            duck = duckController;
        }

        public void FixedUpdate() {
            NextState();
        }

        public void Exit() {
        }

        public Vector2 CalculateVelocity() {
            return duck.moveInputVector * duck.moveSpeed;
        }

        public Quaternion CalculateRotation() {
            float deltaTime = Time.deltaTime;
            if (!(deltaTime > 0)) return duck.rotation;
            // Smoothly interpolate from current to target look direction
            Vector3 smoothedLookInputDirection = Vector3.Slerp(duck.motor.CharacterForward,
                new Vector3(duck.moveInputVector.x, 0, duck.moveInputVector.y),
                1 - Mathf.Exp(-duck.orientationSharpness * deltaTime)).normalized;

            // Set the current rotation (which will be used by the KinematicCharacterMotor)
            if (smoothedLookInputDirection.sqrMagnitude > 0f) {
                return Quaternion.LookRotation(smoothedLookInputDirection, duck.motor.CharacterUp);
            }

            return duck.rotation;

        }

        private void NextState() {
            // transition to dash state
            if (duck.timeSinceDashInput is >= 0) {
                duck.PushState(duck.dashState);
            }
        }
    }

    public class DashState : IState {
        public DuckController duck;
        private Vector2 _duckDir;
        private Vector2 _dashVelocity;

        public void Enter(DuckController duckController) {
            duck = duckController;
            _duckDir = duck.velocity;
            // if original velocity is zero, set to forward
            if (_duckDir == Vector2.zero) {
                // base on duck.rotation
                var forward = duck.rotation * Vector3.forward;
                _duckDir = new Vector2(forward.x, forward.z);
            }
            
            _dashVelocity = _duckDir.normalized * duck.dashSpeed;

        }

        public void FixedUpdate() {
            NextState();
            duck.timeSinceDashInput += Time.deltaTime;
        }

        public void Exit() {
            duck.timeSinceDashInput = null;
        }

        public Vector2 CalculateVelocity() {
            // same direction but at duck.dashSpeed magnitude
            return _dashVelocity;
        }

        public Quaternion CalculateRotation() {
            // set to dash direction based off of original velocity
            
            return Quaternion.LookRotation(
                new Vector3(_dashVelocity.x, 0, _dashVelocity.y),
                duck.motor.CharacterUp
            );
        }

        public void NextState() {
            // transition to previous state 
            if (duck.timeSinceDashInput >= duck.dashTime) {
                duck.PopState();
            }
        }
    }
