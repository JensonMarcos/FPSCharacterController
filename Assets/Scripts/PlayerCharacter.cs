using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

public struct PlayerCharacterInputs
{
    public float ForwardAxis;
    public float RightAxis;
    public Quaternion CameraRotation;
    public bool Jump;
    public bool Crouch;
}

public enum Stance
{
    Stand, Crouch, Slide
}

public struct CharacterState
{
    public bool Grounded;
    public Stance Stance;
    public Vector3 Velocity;
}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    public KinematicCharacterMotor Motor;

    public Transform camTarget;
    public Transform root;

    PlayerCharacterInputs Inputs;
    Quaternion wishRotation;
    Vector3 wishMovement;
    bool wishJump;
    bool wishCrouch;
    bool wishCrouchInAir;

    float timeUngrounded;
    float timeJumpRequested;
    bool ungroundedBcJump;

    Collider[] uncrouchColliders = new Collider[8];

    public CharacterState state;
    CharacterState laststate, tempstate;

    [Space]
    [Header("Walk")]
    [SerializeField] float walkSpeed;
    [SerializeField] float walkAcceleration;

    [Space]
    [Header("Air")]
    [SerializeField] float airSpeed;
    [SerializeField] float airAcceleration;

    [Space]
    [Header("Crouch")]
    [SerializeField] float crouchSpeed;
    [SerializeField] float crouchAcceleration;
    
    [SerializeField] float standHeight, crouchHeight;
    [SerializeField] float camStandHeight, camCrouchHeight;

    [Space]
    [Header("Jump")]
    [SerializeField] float jumpSpeed;
    [SerializeField] float coyoteTime;
    [SerializeField] float gravity;

    [Space]
    [Header("Slide")]
    [SerializeField] float slideStartSpeed;
    [SerializeField] float slideEndSpeed;
    [SerializeField] float slideFriction;
    [SerializeField] float slideAcceleration;

    private void Awake()
    {
        // Assign the characterController to the motor
        Motor.CharacterController = this;
        state.Stance = Stance.Stand;
        laststate = state;

        uncrouchColliders = new Collider[8];
    }


    public void SetInputs(ref PlayerCharacterInputs inputs)
    {
        Inputs = inputs;

        wishRotation = inputs.CameraRotation;

        wishMovement = Vector3.ClampMagnitude(new Vector3(inputs.RightAxis, 0f, inputs.ForwardAxis), 1f);
        wishMovement = inputs.CameraRotation * wishMovement;

        var wasRequestingJump = wishJump;
        wishJump = wishJump || inputs.Jump;
        if (wishJump && !wasRequestingJump) timeJumpRequested = 0f;

        var wasRequestingCrouch = wishCrouch;
        wishCrouch = inputs.Crouch;
        if (wishCrouch && !wasRequestingCrouch)
            wishCrouchInAir = !state.Grounded;
        else if (!wishCrouch && wasRequestingCrouch)
            wishCrouchInAir = false;
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its rotation should be right now. 
    /// This is the ONLY place where you should set the character's rotation
    /// </summary>
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        var forward = Vector3.ProjectOnPlane(Inputs.CameraRotation * Vector3.forward, Motor.CharacterUp);

        if (forward != Vector3.zero) currentRotation = Quaternion.LookRotation(forward, Motor.CharacterUp);
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its velocity should be right now. 
    /// This is the ONLY place where you can set the character's velocity
    /// </summary>
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        //grounded
        if (Motor.GroundingStatus.IsStableOnGround)
        {
            timeUngrounded = 0f;
            ungroundedBcJump = false;

            var groundedMovement = Motor.GetDirectionTangentToSurface(wishMovement, Motor.GroundingStatus.GroundNormal);
            //initiate slide
            {
                var moving = groundedMovement.sqrMagnitude > 0f;
                var crouching = state.Stance == Stance.Crouch;
                var wasStanding = laststate.Stance == Stance.Stand;
                var wasInAir = !laststate.Grounded;

                if (moving && crouching && (wasStanding || wasInAir))
                {
                    state.Stance = Stance.Slide;

                    if (wasInAir)
                    {
                        currentVelocity = Vector3.ProjectOnPlane(laststate.Velocity, Motor.GroundingStatus.GroundNormal);
                    }

                    var effectiveSlideStartSpeed = slideStartSpeed;
                    if (!laststate.Grounded && !wishCrouchInAir)
                    {
                        effectiveSlideStartSpeed = 0f;
                        wishCrouchInAir = false;
                    }
                    var slideSpeed = Mathf.Max(effectiveSlideStartSpeed, currentVelocity.magnitude);
                    currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * slideSpeed;
                }
            }
            //move
            if (state.Stance is Stance.Stand or Stance.Crouch)
            {
                var speed = state.Stance == Stance.Stand ? walkSpeed : crouchSpeed;

                var acceleration = state.Stance == Stance.Stand ? walkAcceleration : crouchAcceleration;

                var targetVelocity = groundedMovement * speed;
                currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1f - Mathf.Exp(-acceleration * deltaTime));
                //currentVelocity *= Friction(currentVelocity, walkFriction, walkDeceleration);
                //currentVelocity += Accelerate(groundedMovement, speed, acceleration, currentVelocity);
            }
            else //sliding
            {
                currentVelocity -= currentVelocity * (slideFriction * deltaTime);

                //slope
                var force = Vector3.ProjectOnPlane(-Motor.CharacterUp, Motor.GroundingStatus.GroundNormal) * gravity;
                currentVelocity -= force * deltaTime;

                //steer
                var currentSpeed = currentVelocity.magnitude;
                var targetVelocity = groundedMovement * currentVelocity.magnitude;
                var steerForce = (targetVelocity - currentVelocity) * slideAcceleration * deltaTime;
                currentVelocity += steerForce;
                currentVelocity = Vector3.ClampMagnitude(currentVelocity, currentSpeed);

                if (currentVelocity.magnitude < slideEndSpeed) state.Stance = Stance.Crouch;
            }
        }
        else //air
        {
            timeUngrounded += deltaTime;

            if (wishMovement.sqrMagnitude > 0f)
            {
                var planarMovement = Vector3.ProjectOnPlane(wishMovement, Motor.CharacterUp);
                var currentPlanarVelocity = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);
                var movementForce = planarMovement * airAcceleration * deltaTime;
                //var movementForce = Accelerate(planarMovement, airSpeed, airAcceleration, currentPlanarVelocity);

                if (currentPlanarVelocity.magnitude < airSpeed) //add air movement when below max speed
                {
                    var targetPlanarVelocity = currentPlanarVelocity + movementForce;
                    targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);
                    movementForce = targetPlanarVelocity - currentPlanarVelocity;
                }
                else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
                { //add movement force that isnt toward velocity
                    var constrainedMovementForce = Vector3.ProjectOnPlane(movementForce, currentPlanarVelocity.normalized);
                    movementForce = constrainedMovementForce;
                }

                currentVelocity += movementForce;
            }


            currentVelocity += Motor.CharacterUp * gravity * deltaTime;
        }

        if (wishJump)
        {
            var grounded = Motor.GroundingStatus.IsStableOnGround;
            var canCoyote = timeUngrounded < coyoteTime && !ungroundedBcJump;

            if (grounded || canCoyote)
            {
                wishJump = false;

                Motor.ForceUnground(0.1f);
                ungroundedBcJump = true;

                var currentVerticalSpeed = Vector3.Dot(currentVelocity, Motor.CharacterUp);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
                currentVelocity += Motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            }
            else
            {
                timeJumpRequested += deltaTime;

                var canJumpLater = timeJumpRequested < coyoteTime;

                wishJump = canJumpLater;
            }
        }
    }


    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called before the character begins its movement update
    /// </summary>
    public void BeforeCharacterUpdate(float deltaTime)
    {
        tempstate = state;
        if (wishCrouch && state.Stance == Stance.Stand)
        {
            state.Stance = Stance.Crouch;
            Motor.SetCapsuleDimensions(Motor.Capsule.radius, crouchHeight, crouchHeight * 0.5f);
            //camTarget.localPosition = new Vector3(0, camCrouchHeight, 0);
            //root.localScale = new Vector3(1, crouchHeight / standHeight, 1);
        }
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called after the character has finished its movement update
    /// </summary>
    public void AfterCharacterUpdate(float deltaTime)
    {
        if (!wishCrouch && state.Stance != Stance.Stand)
        {
            Motor.SetCapsuleDimensions(Motor.Capsule.radius, standHeight, standHeight * 0.5f);

            //check if can uncrouch
            if (Motor.CharacterOverlap(Motor.TransientPosition, Motor.TransientRotation, uncrouchColliders, Motor.CollidableLayers, QueryTriggerInteraction.Ignore) > 0)
            {
                Motor.SetCapsuleDimensions(Motor.Capsule.radius, crouchHeight, crouchHeight * 0.5f);
            }
            else
            {
                state.Stance = Stance.Stand;
                //camTarget.localPosition = new Vector3(0, camStandHeight, 0);
                //root.localScale = Vector3.one;
            }

        }

        state.Grounded = Motor.GroundingStatus.IsStableOnGround;
        state.Velocity = Motor.Velocity;
        laststate = tempstate;
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        if (!Motor.GroundingStatus.IsStableOnGround && state.Stance is Stance.Slide) state.Stance = Stance.Crouch;

        // // Handle landing and leaving ground
        // if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
        // {
        //     OnLanded();
        // }
        // else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
        // {
        //     OnLeaveStableGround();
        // }
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    public void SetPosition(Vector3 position, bool killVelocity = true)
    {
        Motor.SetPosition(position);
        if (killVelocity) Motor.BaseVelocity = Vector3.zero;
    }


    Vector3 Accelerate(Vector3 wishDir, float wishSpeed, float acceleration, Vector3 velocity)
    {

        float _addSpeed;
        float _currentSpeed;
        Vector3 _newVel;

        // the sauce
        _currentSpeed = Vector3.Dot(velocity, wishDir);
        _addSpeed = Mathf.Clamp(wishSpeed - _currentSpeed, 0, acceleration * wishSpeed * Time.fixedDeltaTime);

        _newVel = wishDir * _addSpeed;

        return _newVel;
    }

    float Friction(Vector3 velocity, float friction, float acceleration)
    {
        float _speed = velocity.magnitude;
        float _control;
        float _newSpeed;

        _control = _speed < acceleration ? acceleration : _speed;
        _newSpeed = _speed - _control * friction * Time.fixedDeltaTime;

        if (_newSpeed < 0) _newSpeed = 0;
        _newSpeed /= _speed;

        return _newSpeed;
    }


}