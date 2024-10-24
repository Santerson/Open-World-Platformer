using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] Vector2 FollowingOffset = new Vector2(0.75f, 1);
    [SerializeField] GameObject FollowingObject;
    [SerializeField] float SpeedDivider = 20f;
    [SerializeField] float NeutralDivider = 2.5f;
    [SerializeField] float TimeLookingBeforeMove = 0.5f;
    [SerializeField] float DistanceUntilSnap = 0.01f;
    [SerializeField] float SwordMaxDistance = 3;
    float MaxYPos;
    float LookingDirection = 0;
    float TimeSpentLooking = 0;
    Vector2 TargetPos;
    Player RefPlayer = null;
    // Start is called before the first frame update
    void Start()
    {
        RefPlayer = FindObjectOfType<Player>().GetComponent<Player>();
        if (RefPlayer == null)
        {
            Debug.LogError("Could not find a player object");
        }
        if (FollowingObject == null)
        {
            Debug.LogError("Camera is following no object");
        }
    }

    // Update is called once per frame
    void Update()
    {
        CalculateTargetPos();
        MoveCameraTowardsTarget();
    }

    private void CalculateTargetPos()
    {
        if (FollowingObject == null) { return; }
        if (LookingDirection == 0) { LookingDirection = FindObjectOfType<Player>().RawInputNoZero; }
        if (RefPlayer.HoldingObject[1].Equals("Sword"))
        {
            CalculateSwordSwingPos();
            return;
        }

        //Get camera offset depending on what the player is doing
        float PlayerImposedOffsetMultiplierX = FindObjectOfType<Player>().CalculateCameraMultiplierX();
        float PlayerImposedOffsetMultiplierY = FindObjectOfType<Player>().CalculateCameraMultiplierY();

        //Get target object position
        Vector2 followingTargetPos = FollowingObject.transform.position;

        //Inverts the position of the camera if the player is looking left
        float invertFollowing = FindObjectOfType<Player>().RawInputNoZero;

        //Creates a target position for the camera. A x and y position are created to be put into a Vec2 later
        float xCameraOffset = followingTargetPos.x + FollowingOffset.x * invertFollowing * PlayerImposedOffsetMultiplierX;
        float yCameraOffset = followingTargetPos.y + FollowingOffset.y * PlayerImposedOffsetMultiplierY;

        TargetPos = new Vector2(xCameraOffset, yCameraOffset);

        //Red line is the player to the target position
        Debug.DrawLine(followingTargetPos, TargetPos, Color.red);
    }

    private void CalculateSwordSwingPos()
    {
        Vector2 playerPos = FindObjectOfType<Player>().transform.position;

        // Get the mouse position in world space
        Vector3 mousePos = Input.mousePosition;

        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10));

        // Calculate the direction from the target to the mouse
        Vector2 direction = ((Vector2)(worldMousePos) - playerPos).normalized;

        // Set the camera's position to the set distance in that direction
        Vector3 newPosition = playerPos + direction * SwordMaxDistance;

        DebugExtensions.DrawCircle(playerPos, SwordMaxDistance, Color.magenta, 32);
        Debug.DrawLine(playerPos, worldMousePos, Color.blue);
        Debug.DrawLine(playerPos, newPosition, Color.green);

        // Update camera position
        TargetPos = newPosition;

    }

    private void MoveCameraTowardsTarget()
    {
        //Yellow line is Camera to Target position
        Debug.DrawLine(transform.position, TargetPos, Color.yellow);

        //Creates a vector2 for the line between the camera and the target position
        Vector2 cameraMovement = new Vector2(TargetPos.x - transform.position.x, TargetPos.y - transform.position.y);

        float neutralDivider = 1;
        if (FindObjectOfType<Player>().IsIdle)
        {
            neutralDivider = NeutralDivider;
        }

        //Moves a fraction of that each frame
        float newCameraPositionX = transform.position.x + (cameraMovement.x / SpeedDivider / neutralDivider) * Time.deltaTime * 100;
        float newCameraPositionY = transform.position.y + (cameraMovement.y / SpeedDivider) * Time.deltaTime * 100;

        if (SUtilities.IsInRange(newCameraPositionX, TargetPos.x - DistanceUntilSnap, TargetPos.x + DistanceUntilSnap))
        {
            newCameraPositionX = TargetPos.x;
        }
        if (SUtilities.IsInRange(newCameraPositionY, TargetPos.y - DistanceUntilSnap, TargetPos.y + DistanceUntilSnap))
            {
                newCameraPositionY = TargetPos.y;
            }

        transform.position = new Vector3(newCameraPositionX, newCameraPositionY, -10);
    }
}
