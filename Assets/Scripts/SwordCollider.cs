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


    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log($"Hit {collision.gameObject}");
        if (FindObjectOfType<Player>().IsSwordSwing && collision.tag == "Enemy")
        {
            FindObjectOfType<Sword>().HitEnemy(collision.gameObject);
        }
    }

}
