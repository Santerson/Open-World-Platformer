using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprintMeter : MonoBehaviour
{
    float CurrentSprint;
    float MaxSprint;
    LineRenderer RefRenderer = null;
    [SerializeField] float Ypos = 0;
    [SerializeField] bool Main = false;

    private void Awake()
    {
        MaxSprint = FindObjectOfType<Player>().MaxStamina;
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
        CurrentSprint = FindObjectOfType<Player>().CurrentStamina;
        if ((CurrentSprint <= 0 && Main) || CurrentSprint >= MaxSprint)
        {
            RefRenderer.enabled = false;
            return;
        }
        RefRenderer.enabled = true;
        if (Main)
        {
            RefRenderer.SetPosition(0, new Vector2(1 - CurrentSprint / MaxSprint, Ypos));
            RefRenderer.SetPosition(1, new Vector2(1, Ypos));
        }
        else
        {
            RefRenderer.SetPosition(0, new Vector2(0, Ypos));
            RefRenderer.SetPosition(1, new Vector2(1, Ypos));
        }
    }
}
