using System;
using UnityEngine;

public static class DebugExtensions
{
    public static void DrawBox(Vector2 pos1, Vector2 pos2, Color color)
    {
        Vector2 lowerRight = new Vector2(pos2.x, pos1.y);
        Vector2 upperLeft = new Vector2(pos1.x, pos2.y);
        Debug.DrawLine(pos1, lowerRight, color);
        Debug.DrawLine(lowerRight, pos2, color);
        Debug.DrawLine(pos2, upperLeft, color);
        Debug.DrawLine(upperLeft, pos1, color);
    }

    public static void DrawBox(Vector2 pos1, Vector2 pos2)
    {
        DrawBox(pos1, pos2, Color.white);
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
