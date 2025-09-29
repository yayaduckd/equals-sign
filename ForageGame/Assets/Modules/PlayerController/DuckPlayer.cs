using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using UnityEngine.InputSystem;

public class DuckPlayer : MonoBehaviour {
    public DuckController duck;
    public DuckCameraController cam;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;

        // Tell camera to follow transform
        cam.SetFollowTransform(duck.cameraFollowPoint);

        // Ignore the character's collider(s) for camera obstruction checks
        cam.IgnoredColliders.Clear();
        cam.IgnoredColliders.AddRange(duck.GetComponentsInChildren<Collider>());
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        cam.Update();

    }

    private void LateUpdate() {
        // Handle rotating the camera along with physics movers
        if (cam.RotateWithPhysicsMover && duck.motor.AttachedRigidbody != null) {
            cam.PlanarDirection =
                duck.motor.AttachedRigidbody.GetComponent<PhysicsMover>().RotationDeltaFromInterpolation *
                cam.PlanarDirection;
            cam.PlanarDirection = Vector3
                .ProjectOnPlane(cam.PlanarDirection, duck.motor.CharacterUp).normalized;
        }
    }

    // inputs
    public void OnMove(InputAction.CallbackContext context) {
        // read the value for the "move" action each event call
        Vector2 moveAmount = context.ReadValue<Vector2>();
        // print
        duck.moveInputVector = Vector2.ClampMagnitude(moveAmount, 1);
    }

    public void OnDash(InputAction.CallbackContext context) {
        // jump for now
        // read digital
        // var p = context.action.IsPressed();
        // var q = context.action.WasPressedThisFrame();
        // var r = context.action.WasReleasedThisFrame();
        // pretty print
        // Debug.Log($"dash: {p} {q} {r}");
        // if pressed this frame, call dash
        if (context.action.WasPressedThisFrame()) {
            duck.timeSinceDashInput = 0;
        }
    }
}