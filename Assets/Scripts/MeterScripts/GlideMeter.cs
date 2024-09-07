using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlideMeter : MonoBehaviour
{
    float Glide;
    LineRenderer RefRenderer = null;
    [SerializeField] float Xpos = 0;
    [SerializeField] bool Main = false;

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
        Glide = FindObjectOfType<Player>().TimeSpentGliding;
        if (Glide >= 2 || Glide <= 0)
        {
            RefRenderer.enabled = false;
            return;
        }
        RefRenderer.enabled = true;
        if (Main)
        {
            RefRenderer.SetPosition(0, new Vector2(Xpos, 0));
            RefRenderer.SetPosition(1, new Vector2(Xpos, Glide));
        }
        else
        {
            RefRenderer.SetPosition(0, new Vector2(Xpos, 0));
            RefRenderer.SetPosition(1, new Vector2(Xpos, 2));
        }
    }
}
