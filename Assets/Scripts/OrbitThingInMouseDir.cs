using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitThingInMouseDir : MonoBehaviour
{
    [SerializeField] public Transform Target;  // The object around which the camera moves
    [SerializeField] bool PointTowardsMouse = false;
    [SerializeField] Vector3 Axis = Vector3.forward;
    public float Radius = 5f; // Radius of the circle

    private void Start()
    {
        if (Target == null)
        {
            Debug.LogError("No target set to orbit LMAO LOOK AT THIS LOL");
        }
    }

    void Update()
    {
        if (Target == null) return;
        // Get the mouse position in world space
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z - Target.position.z); // Adjust for camera's distance

        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        // Calculate the direction from the target to the mouse
        Vector3 direction = (worldMousePos - Target.position).normalized;

        // Set the camera's position at a distance of 'radius' in that direction
        Vector3 newPosition = Target.position + direction * Radius;
        transform.position = newPosition;

        if (PointTowardsMouse)
        {
            // Rotate the object to face the mouse
            Vector2 lookDir = worldMousePos - transform.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Axis);
        }
    }
}
