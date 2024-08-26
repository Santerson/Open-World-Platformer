using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Player : MonoBehaviour
{
    //Movement
    [SerializeField] float MaxSpeed = 0.03f;
    [SerializeField] float SprintMultiplier = 1.3f;
    [SerializeField] float Acceleration = 0.1f;
    [SerializeField] float Deceleration = 0.2f;
    [SerializeField] float DistanceFromWalls = 0.05f;

    //Dashing
    [SerializeField] float DashPower = 0.2f;
    [SerializeField] float DashLength = 100f;
    [SerializeField] float DashCooldown = 1.0f;

    //Jumping
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
    [SerializeField] float GlideGravity = 100f;
    [SerializeField] float GlideTime = 2.0f;

    //Camera
    [SerializeField] float SprintCameraOffsetMultiplier = 1.3f;
    [SerializeField] float DashCameraOffsetMultiplier = 1.5f;
    [SerializeField] float CameraAscendMultiplier = 1.3f;
    [SerializeField] float CameraDescendMultiplier = -0.1f;
    [SerializeField] float CameraMaxDistanceFromPlayerY = 2f;

    //Keybinds
    [SerializeField] KeyCode GlideKey = KeyCode.C;

    /// <summary>
    /// The number that changes the player's position as they move. THIS SHOULD BE THE ONLY THING MOVING THE PLAYER
    /// </summary>
    public float Velocity { get; private set; }
    public float Gravity { get; private set; }
    int Direction = 1;
    int KeyStrokeDirection = 0;
    public bool IsSprinting { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsIdle { get; private set; }
    public bool IsGliding { get; private set; }
    public bool IsLanded { get;private set; }
    int JumpsLeft = 0;
    bool IsUpwardAcceleration = false;
    bool DoubleJumpReady = false;
    float DashLeft = 0;
    float DashCDLeft = 0;
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
        if (Velocity < 0)
        {
            Direction = -1;
        }
        else
        {
            Direction = 1;
        }

        UpdateMovement();
        UpdateGravity();
        UpdateDash();
        transform.position = new Vector2(transform.position.x + Velocity * Time.deltaTime * 300, transform.position.y + Gravity  * (Time.deltaTime * 100));
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

    private void UpdateDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && Velocity != 0 && DashCDLeft <= 0) 
        {
            IsDashing = true;
            DashLeft = DashPower;
            DashCDLeft = DashCooldown;
        }

        if (IsDashing)
        {
            float change = DashLeft / DashLength * (Time.deltaTime * 300);
            DashLeft -= change;
            Velocity += change * KeyStrokeDirection;
            if (DashLeft < 0.1f)
            {
                DashLeft = 0;
                IsDashing = false;
            }
        }
        
        if (DashCDLeft > 0)
        {
            DashCDLeft -= Time.deltaTime;
        }
    }

    private void UpdateGravity()
    {
        //The origin of the raycast should be from the bottom of the player's capsule
        Vector2 origin = new Vector2(transform.position.x, transform.position.y - 1.001f);
        Vector2 direction = Vector2.down;

        float frameDifference = Time.deltaTime * 100;

        if (Input.GetKey(GlideKey))
        {
            IsGliding = true;
        }
        else
        {
            IsGliding = false;
        }

        if (Input.GetKeyUp(GlideKey))

        if (IsGliding)
        {
            GlideTime -= Time.deltaTime;
            if (GlideTime < 0)
            {
                IsGliding = false;
            }
        }

        //Detects if the player is on the floor and stops them from falling/gives them their jumps
        DetectFloorAndRoof(origin, direction, frameDifference);

        //Elevates the player
        JumpPlayer(frameDifference);
    }

    private void JumpPlayer(float frameDifference)
    {

        //Checks if the player can jump
        if (Input.GetKeyDown(KeyCode.Space) && JumpsLeft > 0)
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
        if (!Input.GetKey(KeyCode.Space) && IsUpwardAcceleration)
        {
            Gravity -= GravityIntensity * EarlyEndJumpGravityBoost * frameDifference;
        }
        //Checks if the player is no longer gaining height in their jump
        if (Gravity < 0 && IsUpwardAcceleration)
        {
            IsUpwardAcceleration = false;
        }
    }

    private float CalculateMovementMultiplier()
    {
        float sprintMultiplier;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            sprintMultiplier = SprintMultiplier;
        }
        else
        {
            sprintMultiplier = 1;
        }
        return sprintMultiplier;
    }

    private void MovePlayer(float accelerationRate, float decelerationRate, float sprintMultiplier)
    {

        //Rightward movement
        if (Input.GetKey(KeyCode.D) && Velocity < MaxSpeed * sprintMultiplier && !IsDashing)
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
        if (Input.GetKey(KeyCode.A) && Velocity > -MaxSpeed * sprintMultiplier && !IsDashing)
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
        if ((!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) && !IsDashing)
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

    bool IsGrounded(RaycastHit2D hitInfo)
    {
        bool bCloseToGround = hitInfo.distance < GroundedGraceDistance;
        bool bIsFalling = RefRigidbody.velocity.y <= 0;
        return bCloseToGround && bIsFalling;
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
        float ySpeed = RefRigidbody.velocity.y;
        //TODO: make the camera not go down if the player has jumped
        if (ySpeed > 0f)
        {
            multiplier *= Mathf.Clamp(Mathf.Abs(ySpeed) / 2, 0, CameraAscendMultiplier);
        }
        else if (ySpeed < -0.1f)
        {
            multiplier *= Mathf.Clamp(CameraDescendMultiplier * Mathf.Abs(ySpeed), -CameraMaxDistanceFromPlayerY, 0);
        }
        return multiplier;
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
                    Gravity = GlideGravity;
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
}
