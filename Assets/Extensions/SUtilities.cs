using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SUtilities
{
    public static bool IsInRange(Vector3 position, Vector3 bottomLeft, Vector3 topRight)
    {
        return (position.x >= bottomLeft.x && position.x <= topRight.x && position.y >= bottomLeft.y && position.y <= topRight.y);
    }

    public static bool IsInRange(float number, float lowEnd, float highEnd)
    {
        return (number >= lowEnd && number <= highEnd);
    }

    public static bool PointInBox(Vector2 point, Vector2 TopLeftOfBox, Vector2 BottomRightOfBox)
    {
        bool inXRange = point.x > TopLeftOfBox.x && point.x < BottomRightOfBox.x;
        bool inYRange = point.y < TopLeftOfBox.y && point.y > BottomRightOfBox.y;
        return inXRange && inYRange;
    }
}
