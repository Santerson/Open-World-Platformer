using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SUtilities
{
    public static bool IsInRange(Vector3 position, Vector3 bottomLeft, Vector3 topRight)
    {
        return (position.x >= bottomLeft.x && position.x <= topRight.x && position.y >= bottomLeft.y && position.y <= topRight.y);
    }
}
