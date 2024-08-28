using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashMeter : MonoBehaviour
{
    float Dash;
    LineRenderer RefRenderer = null;
    [SerializeField] float Ypos = 0;

    private void Awake()
    {
        RefRenderer = GetComponent<LineRenderer>();
        if (RefRenderer == null)
        {
            Debug.LogError("No line renderer attached to this object!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (RefRenderer == null) return;
        Dash = FindObjectOfType<Player>().DashCDLeft;
        if (Dash <= 0) 
        {
            RefRenderer.enabled = false;
            return; 
        }
        RefRenderer.enabled = true;
        RefRenderer.SetPosition(0, new Vector2(1 - Dash, Ypos));
        RefRenderer.SetPosition(1, new Vector2(1, Ypos));
    }
}
