using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using Unity.VisualScripting;


public class DuckController : MonoBehaviour, ICharacterController {
    public KinematicCharacterMotor motor;
    // transform
    public Transform transform; 

    [Header("Walking")] public float moveSpeed = 5f;
    public float orientationSharpness = 30f;

    [Header("Abilities")] public float dashSpeed = 30f;
    public float dashTime = 0.15f;

    [Header("Misc")] public List<Collider> ignoredColliders = new List<Collider>();
    public Transform cameraFollowPoint;

    public LayerMask interactionLayer;
    public TrailRenderer trailRenderer;
    
    public enum AttackType {
        none,
        light,
        heavy
    }
    public bool attacking = false;
    public AttackType attackType = AttackType.none;
    public float lightAttackTime = 0.2f;
    public float heavyAttackTime = 0.5f;
    
    public BoxCollider hitboxCollider;
    
    
    
    // SECTION: STATES
    private readonly Stack<IState> _stateStack = new Stack<IState>();
    public IState currentState;
    
    // preset states
    public readonly IState idleState = new IdleState();
    public readonly IState dashState = new DashState();
    public readonly IState throughDashState = new ThroughDashState();
    public readonly IState attackState = new AttackState();
    // current dash state (can be dashState or throughDashState)
    public IState currentDashState;

    // state
    public Vector2 velocity = Vector2.zero;
    public Quaternion rotation = Quaternion.identity;

    // inputs
    public Vector2 moveInputVector = Vector2.zero;
    public float? timeSinceDashInput = null;
    public bool interactInput = false;

    //Interaction
    public PlayerInteract playerInteract;
    
    // SECTION: COLLISIONS
    private readonly Collider[] _probedColliders = new Collider[8];


    private void Awake() {
        // Assign the characterController to the motor
        motor.CharacterController = this;
        transform = this.GetComponent<Transform>();
        TryGetComponent<PlayerInteract>(out playerInteract);
        
        // hitbox collider
        // get child
        if (hitboxCollider == null) {
            hitboxCollider = GetComponentInChildren<BoxCollider>();
        }
        hitboxCollider.enabled = false;
        
        trailRenderer.emitting = false;
        
        currentState = idleState;
        currentDashState = dashState;
        currentState.Enter(this);

    }

    /// <summary>
    /// Handles movement state transitions and enter/exit callbacks
    /// </summary>
    public void PushState(IState newState) {
        if (newState == null) return;
        _stateStack.Push(currentState);
        currentState.Exit();
        currentState = newState;
        currentState.Enter(this);
    }

    public void PopState() {
        currentState.Exit();
        currentState = _stateStack.Pop();
        currentState.Enter(this);
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called before the character begins its movement update
    /// </summary>
    public void BeforeCharacterUpdate(float deltaTime) {

        currentState.FixedUpdate();



    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its rotation should be right now. 
    /// This is the ONLY place where you should set the character's rotation
    /// </summary>
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
        rotation = currentState.CalculateRotation();
        currentRotation = rotation;
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its velocity should be right now. 
    /// This is the ONLY place where you can set the character's velocity
    /// </summary>
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
        Vector2 newVelocity = currentState.CalculateVelocity();
        currentVelocity = new Vector3(velocity.x, 0, velocity.y);
        velocity = newVelocity;
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called after the character has finished its movement update
    /// </summary>
    public void AfterCharacterUpdate(float deltaTime) {
        int colliders = motor.CharacterOverlap(motor.TransientPosition, motor.TransientRotation, _probedColliders,
            interactionLayer, QueryTriggerInteraction.Collide);
        // list colliders
        for (int i = 0; i < colliders; i++) {
            Collider col = _probedColliders[i];
            // Debug.Log(col.name);
            if (col.name == "shadeDash") {
                currentDashState = throughDashState;
            }
        }
    }

    public void PostGroundingUpdate(float deltaTime) {
    }

    public bool IsColliderValidForCollisions(Collider coll) {
        if (ignoredColliders.Count == 0) {
            return true;
        }

        if (ignoredColliders.Contains(coll)) {
            return false;
        }

        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport) {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport) {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) {
    }

    protected void OnLanded() {
    }

    protected void OnLeaveStableGround() {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider) {
        // print
        // Debug.Log("Discrete Collision Detected: " + hitCollider.name);
    }
}