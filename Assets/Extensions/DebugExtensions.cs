using System;
using UnityEngine;

public static class DebugExtensions
{
    public static void DrawBox(Vector2 min, Vector2 max, Color color)
    {
        Vector2 lowerRight = new Vector2(max.x, min.y);
        Vector2 upperLeft = new Vector2(min.x, max.y);
        Debug.DrawLine(min, lowerRight, color);
        Debug.DrawLine(lowerRight, max, color);
        Debug.DrawLine(max, upperLeft, color);
        Debug.DrawLine(upperLeft, min, color);
    }

    public static void DrawBox(Vector2 min, Vector2 max)
    {
        DrawBox(min, max, Color.white);
    }

    public static void DrawCircle(Vector2 center, float radius, Color color, int numberOfPoints = 16)
    {
        Vector2 radiusVector = Vector2.right * radius;
        float stepAngle = 360.0f / numberOfPoints;
        Quaternion rotator = Quaternion.AngleAxis(stepAngle, Vector3.forward);
        //loop around the circle
        for (int i = 0; i < numberOfPoints; ++i)
        {
            Vector2 firstPointOnCircle = center + radiusVector;
            radiusVector = rotator * radiusVector;
            Vector2 secondPointOnCircle = center + radiusVector;
            Debug.DrawLine(firstPointOnCircle, secondPointOnCircle, color);
        }
    }

    public static void DrawCircle(Vector2 center, float radius)
    {
        DrawCircle(center, radius, Color.white);
    }
}
