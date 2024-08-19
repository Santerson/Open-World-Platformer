using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField] Vector2 FollowingOffset;
    [SerializeField] GameObject FollowingObject;
    [SerializeField] float SpeedDivider = 100f;
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
        float PlayerImposedOffsetMultiplierX = FindObjectOfType<PlayerPhysicsController>().CalculateCameraMultiplierX();
        Vector2 followingTargetPos = FollowingObject.transform.position;
        float invertFollowing = 1;
        if (FindObjectOfType<PlayerPhysicsController>().LookingDirection == Vector2.left)
        {
            invertFollowing *= -1;
        }
        float xCameraOffset = followingTargetPos.x + FollowingOffset.x * invertFollowing * PlayerImposedOffsetMultiplierX;
        float yCameraOffset = followingTargetPos.y + FollowingOffset.y;
        TargetPos = new Vector2(xCameraOffset, yCameraOffset);
        Debug.DrawLine(followingTargetPos, TargetPos, Color.red);
    }

    private void MoveCameraTowardsTarget()
    {
        Debug.DrawLine(transform.position, TargetPos, Color.yellow);
        Vector2 cameraMovement = new Vector2(TargetPos.x - transform.position.x, TargetPos.y - transform.position.y);
        transform.position = new Vector3(transform.position.x + (cameraMovement.x / SpeedDivider), transform.position.y + (cameraMovement.y / SpeedDivider), -10);
    }
}
