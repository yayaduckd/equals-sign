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


public class IdleState : IState {
    public DuckController duck;

    public virtual void Enter(DuckController duckController) {
        duck = duckController;
    }

    public virtual void FixedUpdate() {
        if (duck.interactInput) { duck.playerInteract?.Interact(); }
        else { duck.playerInteract?.StopInteract();}
        NextState();
    }

    public virtual void Exit() {
        if (duck.interactInput) { duck.playerInteract?.StopInteract(); }
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
            duck.PushState(duck.currentDashState);
        }
        else if (duck.attacking) {
            duck.PushState(duck.attackState);
        }
    }
}

public class DashState : IState {
    public DuckController duck;
    protected Vector2 duckDir;
    protected Vector2 dashVelocity;

    public virtual void Enter(DuckController duckController) {
        duck = duckController;
        duckDir = duck.velocity;
        // if original velocity is zero, set to forward
        if (duckDir == Vector2.zero) {
            // base on duck.rotation
            var forward = duck.rotation * Vector3.forward;
            duckDir = new Vector2(forward.x, forward.z);
        }

        dashVelocity = duckDir.normalized * duck.dashSpeed;
        duck.trailRenderer.emitting = true;
        duck.trailRenderer.startColor = Color.yellow;
        duck.trailRenderer.endColor = Color.yellow;

    }

    public virtual void FixedUpdate() {
        NextState();
        duck.timeSinceDashInput += Time.deltaTime;
    }

    public virtual void Exit() {
        duck.timeSinceDashInput = null;
        duck.trailRenderer.emitting = false;

    }

    public Vector2 CalculateVelocity() {
        // same direction but at duck.dashSpeed magnitude
        return dashVelocity;
    }

    public Quaternion CalculateRotation() {
        // set to dash direction based off of original velocity

        return Quaternion.LookRotation(
            new Vector3(dashVelocity.x, 0, dashVelocity.y),
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

public class ThroughDashState : DashState {
    private Collider _collider;

    public override void Enter(DuckController duckController) {
        base.Enter(duckController);
        duck.motor.CollidableLayers.value &= ~(1 << LayerMask.NameToLayer("Throughdashable"));
        duck.trailRenderer.startColor = Color.black;
        duck.trailRenderer.endColor = Color.black;

    }

    public override void Exit() {
        // Debug.Log("exiting through dash state");
        base.Exit();
        duck.motor.CollidableLayers.value |= (1 << LayerMask.NameToLayer("Throughdashable"));
    }
}

public class AttackState : IdleState {
    // public DuckController duck;
    private float attackTime;
    private float timeSinceAttackStart;

    public override void Enter(DuckController duckController) {
        duck = duckController;
        if (duck.attackType == DuckController.AttackType.light) {
            attackTime = duck.lightAttackTime;
            // duck.animator.Play("LightAttack", 0, 0);
        } else if (duck.attackType == DuckController.AttackType.heavy) {
            attackTime = duck.heavyAttackTime;
            // duck.animator.Play("HeavyAttack", 0, 0);
        }

        timeSinceAttackStart = 0;
        // duck.attacking = false; // reset input
        // Debug.Log("entering attack state");
        duck.hitboxCollider.enabled = true;
    }

    public override void FixedUpdate() {
        timeSinceAttackStart += Time.deltaTime;
        NextState();
    }

    public override void Exit() {
        // Debug.Log("exiting attack state");
        duck.attackType = DuckController.AttackType.none;
        duck.attacking = false;
        duck.hitboxCollider.enabled = false;
    }
    

    private void NextState() {
        // transition to previous state
        if (timeSinceAttackStart >= attackTime) {
            duck.PopState();
        }
    }
}