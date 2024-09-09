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
    [Tooltip("The... max speed...")]
    [SerializeField] float MaxSpeed = 0.025f;
    [Tooltip("The rate at which the player accelerates up to max speed before reaching max speed")]
    [SerializeField] float Acceleration = 0.1f;
    [Tooltip("The rate at which the player slows down once releasing movement keys OR TURNING AROUND")]
    [SerializeField] float Deceleration = 0.2f;
    [Tooltip("The distance the player can be from a wall before being unable to move towards it")]
    [SerializeField] float DistanceFromWalls = 0.01f;
    [Tooltip("The amount of time the player can't move before being considered 'idle'")]
    [SerializeField] float IdleTimeoutTime = 2f;

    //Stamina and Sprint
    [Header("Stamina and Sprint")]
    [Tooltip("The multiplier to player speed if they are sprinting")]
    [SerializeField] float SprintMultiplier = 1.3f;
    [Tooltip("The amount of time the player can run for (in seconds)")]
    [SerializeField] public float MaxStamina = 5f;
    [Tooltip("The amound of time after stopping sprinting before recovering Stamina (in seconds)")]
    [SerializeField] float StaminaRecoveryTime = 0.5f;
    [Tooltip("The amount of time after running out of Stamina before recovering Stamina (in seconds)")]
    [SerializeField] float ExhaustionRecoveryTime = 2f;
    [SerializeField] float SprintStaminaCostMultiplier = 1f;
    [SerializeField] float GlideStaminaCostMultiplier = 2f;
    [SerializeField] float DashStaminaCost = 1.5f;
    [SerializeField] float DoubleJumpStaminaCost = 1;
    [SerializeField] float StaminaRegenMultiplier = 2f;

    //Dashing
    [Header("Dashing")]
    [Tooltip("The power of the dash (somewhat obselite)")]
    [SerializeField] float DashPower = 0.2f;
    [Tooltip("The length of the dash. Shorter numbers mean a shorter dash.")]
    [SerializeField] float DashLength = 100f;
    [Tooltip("The maximum NEEEEEEEWWWWWWMMMMMMness of a dash")]
    [SerializeField] float DashMaxSpeed = 0.06f;
    [Tooltip("The amount of time the player needs to wait between dashes (in seconds)")]
    [SerializeField] public float DashCooldown = 1.0f;
    [Tooltip("After a dash is complete, the amount of time a dash is rolled out for")]
    [SerializeField] float DashRolloutTime = 0.1f;
    [Tooltip("After a dash is complete, the speed of the rollout (Higher numbers mean a SLOWER dash)")]
    [SerializeField] float DashRolloutDivider = 500f;

    //Jumping
    [Header("Jumping")]
    [Tooltip("Tonight my friends, we will soar through the heavens - Queen Scarlet, Wings of Fire")]
    [SerializeField] float JumpHeight = 0.1f;
    [Tooltip("The... Intensity of the Gravity... Does anyone actually read these?")]
    [SerializeField] float GravityIntensity = 0.003f;
    [Tooltip("The max fall speed of the player. The player can by no means exceed this fallrate")]
    [SerializeField] float MaxFallSpeed = -0.2f;
    [Tooltip("Remember, the jump off the ground counts!")]
    [SerializeField] int Jumps = 2;
    [Tooltip("The actual player's height off the ground when they are landed")]
    [SerializeField] float HeightOffGround = 0f;
    [Tooltip("The player's height off the ground for them to be considered as far as the game is considered," +
        " 'landed' (Return jump, refresh glide, etc.)")]
    [SerializeField] float GroundedGraceDistance = 0.1f;
    [Tooltip("If the player stops holding jump mid-jump, the amplification of the gravity before they 0 out in height")]
    [SerializeField] float EarlyEndJumpGravityBoost = 1.5f;
    [Tooltip("The extra power of future jumps (Is a multiplication factor)")]
    [SerializeField] float DoubleJumpPower = 1.5f;
    [Tooltip("Just how terrible do you want to make the hitboxes?")]
    [SerializeField] float HeadHitDistance = 0.03f;

    //Gliding
    [Header("Gliding")]
    [Tooltip("The rate at which the player falls while gliding")]
    [SerializeField] float GlideGravity = 0.01f;
    [Tooltip("Resets the glide if the glide button is released mid air")]
    [SerializeField] bool ResetGlideOnKeyUp = false;

    //Sword
    [Header("Sword")]
    [SerializeField] public float SwordSwingDuration = 1f;
    [SerializeField] public float SwordReloadTime = 1.5f;

    //Camera
    [Header("Camera")]
    [Tooltip("While the player is sprinting, the camera's distance will be multiplied by this amount")]
    [SerializeField] float SprintCameraOffsetMultiplier = 1.3f;
    [Tooltip("Honeestly, I think most of these are fairly self explanitory")]
    [SerializeField] float DashCameraOffsetMultiplier = 1.5f;
    [Tooltip("I doubt I will ever read these.")]
    [SerializeField] float GlideCameraOffset = -1f;
    [Tooltip("The player's maximum distance from the camera. (NOT WORKING)")]
    [SerializeField] float CameraMaxDistanceFromPlayerY = 2f;
    [Tooltip("The amount of time spent gliding before the camera stays looking down while not gliding")]
    [SerializeField] float TimeToCameraStayDownAfterGlide = 0.3f;
    [Tooltip("The amount of time the player has to hold looking up or down for the camera to move (in seconds)")]
    [SerializeField] float TimeToLookVertically = 0.5f;
    [Tooltip("The amount of y that is added when looking up")]
    [SerializeField] float LookUpCameraAddedOffset = 1.5f;
    [Tooltip("The amount of y that is added when looking down (should be negative)")]
    [SerializeField] float LookDownCameraAddedOffset = -4f;
    [Tooltip("Counts the player as Idle while looking up or down")]
    [SerializeField] bool LookCountsAsIdle = false;

    //Keybinds
    [Header("Keybinds")]
    [Tooltip("Default: A")]
    [SerializeField] KeyCode LeftKey = KeyCode.A;
    [Tooltip("Default: D")]
    [SerializeField] KeyCode RightKey = KeyCode.D;
    [Tooltip("Defualt: W")]
    [SerializeField] KeyCode LookUpKey = KeyCode.W;
    [Tooltip("Default: S")]
    [SerializeField] KeyCode LookDownKey = KeyCode.S;
    [Tooltip("Default: space")]
    [SerializeField] KeyCode JumpKey = KeyCode.Space;
    [Tooltip("Default: Lshift")]
    [SerializeField] KeyCode SprintKey = KeyCode.LeftShift;
    [Tooltip("Default: Lctrl")]
    [SerializeField] KeyCode DashKey = KeyCode.LeftControl;
    [Tooltip("Default: C")]
    [SerializeField] KeyCode GlideKey = KeyCode.C;
    [Tooltip("Default: LeftMouse (0)")]
    [SerializeField] int AttackButton = 0;
    [Tooltip("default: RightMouse(1)")]
    [SerializeField] int StrongAttackButton = 1;

    //Position changes each frame (Velocity = x, Gravity = y)
    public float Velocity { get; private set; }
    public float Gravity { get; private set; }

    //Player Input Direction

    /// <summary>
    /// The direction the player is heading where 1 is right and -1 is left. Cannot be 0.
    /// This is not the player's input but rather the player's velocity and which way they are actively going
    /// For raw input, use RawInputDirection
    /// </summary>
    public int Direction { get; private set; }
    /// <summary>
    /// The raw input from the player's keyboard, where 1 is right, -1 is left, and 0 is no input/both right and left.
    /// This is the player's raw input regardless of whether they can move or not
    /// </summary>
    public int RawInputDirection { get; private set; }
    /// <summary>
    /// The raw input of the player, where 1 is right and -1 is left. CANNOT BE 0.
    /// This is the player's raw input, regardless of whether they can move or not.
    /// (THE LARGE DIFFERENCE BETWEEN THIS AND RAW INPUT IS THAT IT CANNOT BE 0, hence, no divide by 0 errors or multiply by 0)
    /// </summary>
    public int RawInputNoZero { get; private set; }

    //Is ... actions
    public bool IsSprinting { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsIdle { get; private set; }
    public bool IsGliding { get; private set; }
    public bool IsLanded { get; private set; }
    public bool IsFalling { get; private set; }
    public bool IsLookingUp { get; private set; }
    public bool IsLookingDown { get; private set; }
    public bool IsSwordSwing { get; private set; }
    public bool IsStrongSwordSwing { get; private set; }

    //Public cooldowns
    public float DashCDLeft { get; private set; }
    public float TimeSpentGliding { get; private set; }
    public float CurrentStamina { get; private set; }



    //Movement
    float IdleTimeLeft = 0;


    //Sprinting
    float NoSprintTime = 0;

    bool IsExhausted = false;

    //Jumping
    int JumpsLeft = 0;

    bool IsUpwardAcceleration = false;

    //Gliding

    //Dashing
    float DashLeft = 0;
    float DashRolloutLeft = 0;

    bool SlowDownDash = false;

    //Sword
    float SwordAttackingTimeLeft;
    float SwordReloadingTimeLeft;

    //Camera
    float TimeUntilLookUp = 0;
    float TimeUntilLookDown = 0;

    bool CameraStartedGliding = false;

    private void Start()
    {
        CurrentStamina = MaxStamina;
    }

    void Update()
    {
        UpdateVariables();

        UpdateMovement();
        UpdateGravity();
        UpdateDash();
        UpdateIdleTimeout();
        UpdateAttack();
        transform.position = new Vector2(transform.position.x + Velocity * Time.deltaTime * 300, transform.position.y + Gravity * (Time.deltaTime * 100));
    }




    //Movement Related functions
    private void UpdateMovement()
    {
        CalculateStamina();

        //Checking for sprint
        float sprintMultiplier = CalculateMovementMultiplier();

        //Checking if player is moving too fast
        //Reduces their speed if so
        if (Mathf.Abs(Velocity) > MaxSpeed * sprintMultiplier + DashLeft)
        {
            //Checks if the player is dashing, and if so, sets that to the speed
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
        //Checks if the player is looking up or down
        LookUpAndDown();
    }
    private float CalculateMovementMultiplier()
    {
        //Create a return variable
        float sprintMultiplier;
        //Check if the player is sprinting
        if (IsSprinting)
        {
            //Add sprint multiplier
            sprintMultiplier = SprintMultiplier;
        }
        else
        {
            sprintMultiplier = 1;
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
        if (Input.GetKey(RightKey) && Velocity < MaxSpeed * sprintMultiplier && !IsDashing && !IsStrongSwordSwing)
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
            RawInputDirection = 1;
        }
        //Leftward movement
        if (Input.GetKey(LeftKey) && Velocity > -MaxSpeed * sprintMultiplier && !IsDashing && !IsStrongSwordSwing)
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
            RawInputDirection = -1;
        }
        //Checks if player is not moving or if they are moving faster than they should be
        if (((!Input.GetKey(LeftKey) && !Input.GetKey(RightKey) || Input.GetKey(LeftKey) && Input.GetKey(RightKey)) && !IsDashing) || IsStrongSwordSwing)
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
            RawInputDirection = 0;
        }
    }
    private void UpdateDash()
    {
        //Detects if the player is holding down the dash key and is able to dash
        if (Input.GetKeyDown(DashKey) && Velocity != 0 && DashCDLeft <= 0 && CurrentStamina > DashStaminaCost)
        {
            IsDashing = true;
            DashLeft = DashPower;
            DashCDLeft = DashCooldown;
            CurrentStamina -= DashStaminaCost;
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
                Velocity += change * RawInputDirection;
            }
            else
            {
                Velocity = DashMaxSpeed * RawInputDirection;
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
    private void LookUpAndDown()
    {

        if (Input.GetKey(LookUpKey) && RawInputDirection == 0 && Gravity == 0)
        {
            TimeUntilLookUp += Time.deltaTime;
            if (TimeUntilLookUp > TimeToLookVertically)
            {
                IsLookingUp = true;
                if (LookCountsAsIdle)
                {
                    IsIdle = true;
                }
            }
        }
        else
        {
            IsLookingUp = false;
            TimeUntilLookUp = 0;
        }
        if (Input.GetKey(LookDownKey) && RawInputDirection == 0 && Gravity == 0)
        {
            TimeUntilLookDown += Time.deltaTime;
            if (TimeUntilLookDown > TimeToLookVertically)
            {
                IsLookingDown = true;
                if (LookCountsAsIdle)
                {
                    IsIdle = true;
                }
            }
        }
        else
        {
            IsLookingDown = false;
            TimeUntilLookDown = 0;
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
    private void CalculateStamina()
    {
        //Check if the player is sprinting and if they have stamina to sprint
        if (Input.GetKey(SprintKey) && CurrentStamina > 0)
        {
            IsSprinting = true;
        }
        else
        {
            IsSprinting = false;
        }

        //If the player is sprinting
        if (IsSprinting)
        {
            //Reduce their stamina
            CurrentStamina -= Time.deltaTime * SprintStaminaCostMultiplier;
            //Reset their time without sprinting
            NoSprintTime = 0;
        }

        if (IsGliding)
        {
            CurrentStamina -= Time.deltaTime * GlideStaminaCostMultiplier;

            NoSprintTime = 0;
        }

        //If the player is not sprinting and is not out of stamina, and isnt at max stamina
        else if (!IsExhausted && CurrentStamina < MaxStamina)
        {
            //increase the time without sprinting
            NoSprintTime += Time.deltaTime;
            //regenerate stamina if the player has not sprinted for long enough
            if (NoSprintTime > StaminaRecoveryTime)
            {
                CurrentStamina += Time.deltaTime * StaminaRegenMultiplier;
            }
        }
        //If the player is at 0 stamina
        else if (CurrentStamina < MaxStamina)
        {
            //Increase their no sprint time
            NoSprintTime += Time.deltaTime;
            //Increase their stamina if they have recovered from their exhaustion time
            if (NoSprintTime > ExhaustionRecoveryTime)
            {
                CurrentStamina += Time.deltaTime * StaminaRegenMultiplier;
                IsExhausted = false;
            }
        }

        //Capping stamina
        if (CurrentStamina > MaxStamina)
        {
            CurrentStamina = MaxStamina;
        }
    }


    //Jumping Related Functions
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
    private void JumpPlayer(float frameDifference)
    {

        //Checks if the player can jump
        if (Input.GetKeyDown(JumpKey) && JumpsLeft == Jumps)
        {
            --JumpsLeft;
            Gravity = JumpHeight;

            IsUpwardAcceleration = true;
        }
        else if (Input.GetKeyDown(JumpKey) && JumpsLeft > 0 && CurrentStamina > DoubleJumpStaminaCost)
        {
            --JumpsLeft;
            //increases jump height if the player is doing a double jump
            Gravity = JumpHeight * DoubleJumpPower;
            CurrentStamina -= DoubleJumpStaminaCost;
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
    private void Glide()
    {
        if (Input.GetKey(GlideKey) && !IsLanded && !IsExhausted)
        {
            IsGliding = true;
        }
        else
        {
            IsGliding = false;
        }

        if (IsGliding)
        {
            TimeSpentGliding += Time.deltaTime;
            if (IsExhausted)
            {
                IsGliding = false;
            }
        }

        if (IsLanded)
        {
            TimeSpentGliding = 0;
            CameraStartedGliding = false;
            IsGliding = false;
        }

        if (TimeSpentGliding > TimeToCameraStayDownAfterGlide && IsGliding)
        {
            CameraStartedGliding = true;
        }
    }



    //Camera Related Functions
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

        if (IsLookingDown)
        {
            multiplier += LookDownCameraAddedOffset;
        }
        if (IsLookingUp)
        {
            multiplier += LookUpCameraAddedOffset;
        }

        //Creates a return Value to be clamped within our range
        float returnVal = multiplier * ySpeed;
        //Clamps the return value within the range (DOESN'T WORK RIGHT NOW)
        Mathf.Clamp(returnVal, -CameraMaxDistanceFromPlayerY, CameraMaxDistanceFromPlayerY);
        return multiplier;
    }


    //Sword Functions
    private void UpdateAttack()
    {
        StandardAttack();

        StrongAttack();
    }

    private void StandardAttack()
    {
        //Checks if the player is attacking and if they are able to attack
        if (Input.GetMouseButton(AttackButton) && SwordReloadingTimeLeft <= 0)
        {
            //Makes the player attack with their sword
            IsSwordSwing = true;
            //Starts the animation
            FindObjectOfType<Sword>().SwingSword();
            //Lowers the duration of the attack
            SwordAttackingTimeLeft = SwordSwingDuration;
        }

        if (IsSwordSwing)
        {
            SwordAttackingTimeLeft -= Time.deltaTime;
            SwordReloadingTimeLeft = SwordReloadTime;
            if (SwordAttackingTimeLeft <= 0)
            {
                IsSwordSwing = false;
                SwordReloadingTimeLeft = SwordReloadTime;
            }
        }

        if (SwordReloadingTimeLeft > 0 && !IsSwordSwing)
        {
            SwordReloadingTimeLeft -= Time.deltaTime;
        }
    }

    private void StrongAttack()
    {
        if (Input.GetMouseButtonDown(StrongAttackButton))
        {
            IsStrongSwordSwing = true;
        }
    }

    //Misc. Functions
    bool IsGrounded(RaycastHit2D hitInfo)
    {
        bool bCloseToGround = hitInfo.distance < GroundedGraceDistance;
        return bCloseToGround;
    }
    private void UpdateVariables()
    {
        //Update direction
        if (Velocity < 0)
        {
            Direction = -1;
        }
        if (Velocity > 0) 
        {
            Direction = 1;
        }
        
        //Update playerLookingDirection
        if (RawInputDirection != 0)
        {
            RawInputNoZero = RawInputDirection;
        }

        //Update IsFalling
        IsFalling = Gravity < 0;

        //Setting IsExhausted
        IsExhausted = CurrentStamina <= 0;
    }
}
