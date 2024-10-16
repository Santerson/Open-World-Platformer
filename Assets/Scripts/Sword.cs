using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] GameObject RefSwordCollider = null;
    SpriteRenderer RefRenderer = null;
    Player RefPlayer = null;
    
    bool IsActive = false;
    // Start is called before the first frame update
    void Start()
    {
        RefPlayer = FindObjectOfType<Player>().GetComponent<Player>();
        RefRenderer = GetComponent<SpriteRenderer>();

        if (RefRenderer == null)
        {
            Debug.LogError("Could not pull Renderer from attached thingys");
        }
        if (RefPlayer == null)
        {
            Debug.LogError("Somehow, the player has no player object");
        }
        if (RefSwordCollider == null)
        {
            Debug.LogError("No sword Collider attached to sword object!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfShow();
    }

    void CheckIfShow()
    {
        if (RefPlayer == null) { return; }
        if (RefSwordCollider == null) { return; }

        List<string> heldItem = RefPlayer.HoldingObject;
        string itemName = heldItem[0];
        if (itemName.Equals("Sword"))
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    void Show()
    {
        RefRenderer.enabled = true;
        IsActive = true;
    }

    void Hide()
    {
        RefRenderer.enabled = false;
        IsActive = false;
    }
}
