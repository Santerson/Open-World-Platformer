using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordCollider : MonoBehaviour
{
    float lastNum = 1;
    private void Update()
    {
        float direction = FindObjectOfType<Player>().Direction;
        if (direction == 0)
        {
            direction = 1;
        }
        transform.position = new Vector2(FindObjectOfType<Player>().transform.position.x + 1.5f * direction, FindObjectOfType<Player>().transform.position.y);
    }

}
