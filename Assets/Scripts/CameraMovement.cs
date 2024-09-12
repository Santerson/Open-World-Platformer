using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] Vector2 FollowingOffset = new Vector2(0.75f, 1);
    [SerializeField] GameObject FollowingObject;
    [SerializeField] float SpeedDivider = 20f;
    [SerializeField] float NeutralDivider = 2.5f;
    [SerializeField] float TimeLookingBeforeMove = 0.5f;
    float MaxYPos;
    float LookingDirection = 0;
    float TimeSpentLooking = 0;
    Vector2 TargetPos;
    // Start is called before the first frame update
    void Start()
    {
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
        if (FindObjectOfType<Player>().IsStrongSwordSwing) 
        {
            CalculateStrongSwordSwingPos();
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


        //Putting the newly created targetpos into the vec2 for target position
        TargetPos = new Vector2(xCameraOffset, yCameraOffset);

        //Red line is the player to the target position
        Debug.DrawLine(followingTargetPos, TargetPos, Color.red);
    }

    private void CalculateStrongSwordSwingPos()
    {
        Vector2 mouse = Input.mousePosition;
        Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, 10));
        Vector2 playerPos = FindObjectOfType<Player>().transform.position;
        mouseWorldPoint.Normalize();
        Vector3 newPos = new Vector3(playerPos.x +  mouseWorldPoint.x * 4, playerPos.y + mouseWorldPoint.y * 4, 0);
        TargetPos = newPos;
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

        transform.position = new Vector3(newCameraPositionX, newCameraPositionY, -10);
    }
}
