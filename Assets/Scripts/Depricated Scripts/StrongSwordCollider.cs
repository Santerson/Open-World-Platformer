using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrongSwordCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (FindObjectOfType<Player>().IsSwordSwing && collision.tag == "Enemy" && FindObjectOfType<Player>().IsStrongSwordSwing)
        {
            //FindObjectOfType<Sword>().HitEnemy(collision.gameObject);
        }
    }
}
