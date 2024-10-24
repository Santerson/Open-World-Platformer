using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordHitCollider : MonoBehaviour
{
    public List<GameObject> targets = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            targets.Add(collision.gameObject);

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            targets.Remove(collision.gameObject);
        }
    }

    
}
