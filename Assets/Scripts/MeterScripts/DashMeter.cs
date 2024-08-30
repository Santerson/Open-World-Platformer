using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashMeter : MonoBehaviour
{
    float Dash;
    float MaxDash;
    LineRenderer RefRenderer = null;
    [SerializeField] float Ypos = 0;
    [SerializeField] bool Main = false;

    private void Awake()
    {
        MaxDash = FindObjectOfType<Player>().DashCooldown;
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
        if (Main)
        {
            RefRenderer.SetPosition(0, new Vector2(1 - Dash / (MaxDash * 2), Ypos));
            RefRenderer.SetPosition(1, new Vector2(0 + Dash / (MaxDash * 2), Ypos));
        }
        else
        {
            RefRenderer.SetPosition(0, new Vector2(0, Ypos));
            RefRenderer.SetPosition(1, new Vector2(1, Ypos));
        }
    }
}
