using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Player : MonoBehaviour
{
    //Movement
    [Header("Movement")]
    [SerializeField] float MaxSpeed = 0.03f;
    [SerializeField] float SprintMultiplier = 1.3f;
    [SerializeField] float Acceleration = 0.1f;
    [SerializeField] float Deceleration = 0.2f;
    [SerializeField] float DistanceFromWalls = 0.05f;
    [SerializeField] float IdleTimeoutTime = 2f;

    //Dashing
    [Header("Dashing")]
    [SerializeField] float DashPower = 0.2f;
    [SerializeField] float DashLength = 100f;
    [SerializeField] float DashMaxSpeed = 0.05f;
    [SerializeField] float DashCooldown = 1.0f;
    [SerializeField] float DashRolloutTime = 0.5f;
    [SerializeField] float DashRolloutDivider = 500f;

    //Jumping
    [Header("Jumping")]
    [SerializeField] float JumpHeight = 1f;
    [SerializeField] float GravityIntensity;
    [SerializeField] float MaxFallSpeed = -1f;
    [SerializeField] int Jumps = 2;
    [SerializeField] float HeightOffGround = 0.03f;
    [SerializeField] float EarlyEndJumpGravityBoost = 3f;
    [SerializeField] float DoubleJumpPower = 1.5f;
    [SerializeField] float GroundedGraceDistance = 0.1f;
    [SerializeField] float HeadHitDistance = 0.1f;

    //Gliding
    [Header("Gliding")]
    [SerializeField] float GlideGravity = 100f;
    [SerializeField] float GlideTime = 2.0f;

    //Camera
    [Header("Camera")]
    [SerializeField] float SprintCameraOffsetMultiplier = 1.3f;
    [SerializeField] float DashCameraOffsetMultiplier = 1.5f;
    [SerializeField] float GlideCameraOffset = -1f;
    [SerializeField] float CameraMaxDistanceFromPlayerY = 2f;
    [SerializeField] float TimeToCameraStayDownAfterGlide = 0.5f;
    [SerializeField] float TimeToLookVertically = 0.5f;
    [SerializeField] float LookUpCameraAddedOffset = 1.5f;
    [SerializeField] float LookDownCameraAddedOffset = -4f;

    //Keybinds
    [Header("Keybinds")]
    [SerializeField] KeyCode LeftKey = KeyCode.A;
    [SerializeField] KeyCode RightKey = KeyCode.D;
    [SerializeField] KeyCode JumpKey = KeyCode.Space;
    [SerializeField] KeyCode SprintKey = KeyCode.LeftShift;
    [SerializeField] KeyCode DashKey = KeyCode.LeftControl;
    [SerializeField] KeyCode GlideKey = KeyCode.C;

    /// <summary>
    /// The number that changes the player's position as they move. THIS SHOULD BE THE ONLY THING MOVING THE PLAYER
    /// </summary>
    public float Velocity { get; private set; }
    public float Gravity { get; private set; }
    public int Direction { get; private set; }
    public int KeyStrokeDirection { get; private set; }
    public int PlayerLookingDirection { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsIdle { get; private set; }
    public bool IsGliding { get; private set; }
    public bool IsLanded { get;private set; }


    int JumpsLeft = 0;
    bool IsUpwardAcceleration = false;
    bool DoubleJumpReady = false;
    bool SlowDownDash = false;
    bool CameraStartedGliding = false;
    bool LookingUp = false;
    bool LookingDown = false;
    float DashLeft = 0;
    float DashCDLeft = 0;
    float DashRolloutLeft = 0;
    float GlideTimeLeft = 0;
    float IdleTimeLeft = 0;

    Rigidbody2D RefRigidbody = null;
    CapsuleCollider2D RefCollider = null;

    void Awake()
    {
        // Cache a reference to the rigidbody component on the game start to avoid
        // the expensive GetComponent<> call each frame.
        RefRigidbody = GetComponent<Rigidbody2D>();
        if (RefRigidbody == null)
        {
            Debug.LogError("The player controller needs to have a rigidbody.");
        }
        RefCollider = GetComponent<CapsuleCollider2D>();
        if (RefCollider == null)
        {
            Debug.LogError("Player Controller could not find a Collider");
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateVariables();

        UpdateMovement();
        UpdateGravity();
        UpdateDash();
        UpdateIdleTimeout();
        transform.position = new Vector2(transform.position.x + Velocity * Time.deltaTime * 300, transform.position.y + Gravity  * (Time.deltaTime * 100));
    }

    private void UpdateVariables()
    {
        if (Velocity < 0)
        {
            Direction = -1;
        }
        else
        {
            Direction = 1;
        }

        if (KeyStrokeDirection != 0)
        {
            PlayerLookingDirection = KeyStrokeDirection;
        }
    }

    private void UpdateMovement()
    {
        //Checking for sprint
        float sprintMultiplier = CalculateMovementMultiplier();

        //Checking if player is moving too fast
        if (Mathf.Abs(Velocity) > MaxSpeed * sprintMultiplier + DashLeft)
        {
            if (DashLeft != 0)
            {
                Velocity += DashLeft * Direction;
            }
            else
            {
                Velocity = MaxSpeed * sprintMultiplier * Direction;
            }
        }
        
            //Moving the player
        //Defining rates of acceleration and deceleration
        float accelerationRate = Acceleration * Time.deltaTime;
        float decelerationRate = Deceleration * Time.deltaTime;
        //Moves player
        MovePlayer(accelerationRate, decelerationRate, sprintMultiplier);
        //Stops player if touching a wall
        CheckIfPlayerIsTooCloseToWalls();
    }
    private void UpdateGravity()
    {
        //The origin of the raycast should be from the bottom of the player's capsule
        Vector2 origin = new Vector2(transform.position.x, transform.position.y - 1.001f);
        Vector2 direction = Vector2.down;

        float frameDifference = Time.deltaTime * 100;

        //Detects if the player is on the floor and stops them from falling/gives them their jumps
        DetectFloorAndRoof(origin, direction, frameDifference);

        //Glides the player if the keybind is being held
        Glide();

        //Elevates the player
        JumpPlayer(frameDifference);
    }

    private void UpdateDash()
    {
        //Detects if the player is holding down the dash key and is able to dash
        if (Input.GetKeyDown(DashKey) && Velocity != 0 && DashCDLeft <= 0) 
        {
            IsDashing = true;
            DashLeft = DashPower;
            DashCDLeft = DashCooldown;
        }

        //Checks if the player is dashing
        if (IsDashing)
        {
            //Creates a change variable which will be the distance covered this frame
            float change = DashLeft / DashLength * (Time.deltaTime * 300);
            //Reduces from the amount of dash left
            //*Dash left is the total amount of 'dash' in the dash, we take part of this dash
            //as distance traveled*
            DashLeft -= change;
            //Cap the speed at a max speed so sprint dashing isn't faster than normal dashing
            if (Mathf.Abs(Velocity) < DashMaxSpeed)
            {
                Velocity += change * KeyStrokeDirection;
            }
            else
            {
                Velocity = DashMaxSpeed * KeyStrokeDirection;
            }
            //Stop the dash if the dash is almost =0
            if (DashLeft < 0.1f)
            {
                DashLeft = 0;
                DashRolloutLeft = DashRolloutTime;
                SlowDownDash = true;
                IsDashing = false;
            }
        }
        
        //If the dash is complete, we continue to move the player a bit, slower so the stop isn't as jarring
        if (SlowDownDash)
        {
            //Adds a small amount to the Velocity
            Velocity += DashLength * Time.deltaTime / DashRolloutDivider * Direction;
            //Decreases the amount of time left in the dash rollout
            DashRolloutLeft -= Time.deltaTime;
            if (DashRolloutLeft <= 0) 
            {
                
                SlowDownDash = false;
            }
        }

        //Decreases the Dash Cooldown if the Dash is on cooldown
        if (DashCDLeft > 0)
        {
            DashCDLeft -= Time.deltaTime;
        }
    }

    private float CalculateMovementMultiplier()
    {
        //Create a return variable
        float sprintMultiplier;
        //Check if the player is sprinting
        if (Input.GetKey(SprintKey))
        {
            //Add sprint multiplier
            sprintMultiplier = SprintMultiplier;
            IsSprinting = true;
        }
        else
        {
            sprintMultiplier = 1;
            IsSprinting = false;
        }
        /*
         * Because of the way this is implemented, if there are any future attempts at trying to impact
         * the player's speed, there will not be a difference unless the player is sprinting.
         */
        return sprintMultiplier;
    }
    private void MovePlayer(float accelerationRate, float decelerationRate, float sprintMultiplier)
    {

        //Rightward movement
        if (Input.GetKey(RightKey) && Velocity < MaxSpeed * sprintMultiplier && !IsDashing)
        {
            //Checks if the player is turning around or accelerating from nothing
            if (Velocity < 0)
            {
                Velocity += decelerationRate * sprintMultiplier;
            }
            else
            {
                Velocity += accelerationRate * sprintMultiplier;
            }
            IsIdle = false;
            KeyStrokeDirection = 1;
        }
        //Leftward movement
        if (Input.GetKey(LeftKey) && Velocity > -MaxSpeed * sprintMultiplier && !IsDashing)
        {
            //Checks if the player is turning around or accelerating from nothing
            if (Velocity > 0)
            {
                Velocity -= decelerationRate * sprintMultiplier;
            }
            else
            {
                Velocity -= accelerationRate * sprintMultiplier;
            }
            IsIdle = false;
            KeyStrokeDirection = -1;
        }
        //Checks if player is not moving or if they are moving faster than they should be
        if ((!Input.GetKey(LeftKey) && !Input.GetKey(RightKey) || Input.GetKey(LeftKey) && Input.GetKey(RightKey)) && !IsDashing)
        {
            //Decelerates the player depenign on which way they are moving
            if (Velocity > 0)
            {
                Velocity -= decelerationRate;
            }
            else if (Velocity < 0)
            {
                Velocity += decelerationRate;
            }
            //Completely stops the player's movement if they stop moving
            if (Velocity < 0.001f && Velocity > -0.001f)
            {
                Velocity = 0;
            }
            KeyStrokeDirection = 0;
        }
    }

    private void DetectFloorAndRoof(Vector2 origin, Vector2 direction, float frameDifference)
    {

        //Checks if the player is in the range to recover their jumps
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, GroundedGraceDistance);

        IsLanded = false;
        if (IsGrounded(hit) && !IsUpwardAcceleration && Physics2D.Raycast(origin, direction, GroundedGraceDistance))
        {
            IsLanded = true;
        }

        //Checks if the raycast hit object is considered ground
        if (IsLanded)
        {
            //Resets jumps
            JumpsLeft = Jumps;
            DoubleJumpReady = false;
        }

        //Checks if the player is actually touching the ground (using a very small raycast)
        if (Physics2D.Raycast(origin, direction, HeightOffGround) && Gravity < 0)
        {
            //Checks if the object is considered ground
            if (IsGrounded(hit))
            {
                //Stops the player from falling
                Gravity = 0;
            }
        }
        //Checks if the player is currently airborne by using the grounded grace distance
        else if (!IsLanded)
        {
            //Reduces the player's ySpeed if so
            if (Gravity > MaxFallSpeed)
            {
                if (IsGliding && !IsUpwardAcceleration)
                {
                    Gravity = 0 - GlideGravity;
                }
                else
                {
                    Gravity -= GravityIntensity * frameDifference;
                }
            }
        }

        //Changing the direction of the raycast to be from the top of the player going up
        origin = new Vector2(transform.position.x, transform.position.y + 1.001f);
        direction = Vector2.up;

        //Checking if the player hits their head on something above them
        if (Physics2D.Raycast(origin, direction, HeadHitDistance))
        {
            Gravity = 0 - GravityIntensity;
        }
    }

    private void CheckIfPlayerIsTooCloseToWalls()
    {
        //Creating an origin for raycasts
        Vector2 lowOrigin = new Vector2(transform.position.x + 0.5001f, transform.position.y - 0.5f);
        Vector2 highOrigin = new Vector2(transform.position.x + 0.5001f, transform.position.y + 0.5f);

        //Disallows the player to move into walls by using a raycast
        //(Technically, you can, but the wall will push you out, and it looks jank. This fixes that)
        if (Physics2D.Raycast(lowOrigin, Vector2.right, DistanceFromWalls) && Input.GetKey(KeyCode.D) ||
            Physics2D.Raycast(new Vector2(transform.position.x - 0.5001f, lowOrigin.y), Vector2.left, DistanceFromWalls) && Input.GetKey(KeyCode.A) ||
            Physics2D.Raycast(highOrigin, Vector2.right, DistanceFromWalls) && Input.GetKey(KeyCode.D) ||
            Physics2D.Raycast(new Vector2(transform.position.x - 0.5001f, highOrigin.y), Vector2.left, DistanceFromWalls) && Input.GetKey(KeyCode.A))
        {
            Velocity = 0;
        }
    }

    private void JumpPlayer(float frameDifference)
    {

        //Checks if the player can jump
        if (Input.GetKeyDown(JumpKey) && JumpsLeft > 0)
        {
            //Jumps the player and reduces their jumps left
            --JumpsLeft;
            if (DoubleJumpReady)
            {
                //increases jump height if the player is doing a double jump
                Gravity = JumpHeight * DoubleJumpPower;
            }
            else
            {
                Gravity = JumpHeight;
                DoubleJumpReady = true;
            }
            IsUpwardAcceleration = true;
        }
        //Slows down the player more dramatically if they stop holding space while jumping
        if (!Input.GetKey(JumpKey) && IsUpwardAcceleration)
        {
            Gravity -= GravityIntensity * EarlyEndJumpGravityBoost * frameDifference;
        }
        //Checks if the player is no longer gaining height in their jump
        if (Gravity < 0 && IsUpwardAcceleration)
        {
            IsUpwardAcceleration = false;
        }
    }

    private void Glide()
    {
        if (Input.GetKey(GlideKey) && !IsLanded && GlideTimeLeft > 0)
        {
            IsGliding = true;
        }
        else
        {
            IsGliding = false;
        }

        if (Input.GetKeyUp(GlideKey))
        {
            GlideTimeLeft = 0;
        }

        if (IsGliding)
        {
            GlideTimeLeft -= Time.deltaTime;
            if (GlideTimeLeft < 0)
            {
                IsGliding = false;
            }
        }

        if (IsLanded)
        {
            GlideTimeLeft = GlideTime;
            CameraStartedGliding = false;
        }

        if (GlideTime - GlideTimeLeft > TimeToCameraStayDownAfterGlide && IsGliding)
        {
            CameraStartedGliding = true;
        }
    }

    private void UpdateIdleTimeout()
    {
        if (Velocity == 0 && Gravity == 0)
        {
            IdleTimeLeft -= Time.deltaTime;
            if (IdleTimeLeft < 0)
            {
                IsIdle = true;
            }
        }
        else
        {
            IsIdle = false;
            IdleTimeLeft = IdleTimeoutTime;
        }
    }

    public float CalculateCameraMultiplierX()
    {
        float multiplier = 1f;
        if (IsSprinting) multiplier *= SprintCameraOffsetMultiplier;
        if (IsDashing) multiplier *= DashCameraOffsetMultiplier;
        if (IsIdle) multiplier *= 0;
        return multiplier;
    }

    public float CalculateCameraMultiplierY()
    {
        float multiplier = 1f;
        float ySpeed = Gravity;

        //Lowers the camera is the player is/was gliding
        if (IsGliding || CameraStartedGliding)
        {
            //GlideCameraOffset (should) be a negative number
            multiplier *= GlideCameraOffset;
        }

        //Creates a return Value to be clamped within our range
        float returnVal = multiplier * ySpeed;
        //Clamps the return value within the range (DOESN'T WORK RIGHT NOW)
        Mathf.Clamp(returnVal, -CameraMaxDistanceFromPlayerY, CameraMaxDistanceFromPlayerY);
        return multiplier;
    }

    bool IsGrounded(RaycastHit2D hitInfo)
    {
        bool bCloseToGround = hitInfo.distance < GroundedGraceDistance;
        bool bIsFalling = RefRigidbody.velocity.y <= 0;
        return bCloseToGround && bIsFalling;
    }
}
