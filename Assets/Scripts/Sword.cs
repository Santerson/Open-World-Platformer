using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] GameObject RefSwordCollider = null;
    SpriteRenderer RefRenderer = null;
    Player RefPlayer = null;
    SwordHitCollider RefSwordColliderScript = null;
    
    bool IsActive = false;
    // Start is called before the first frame update
    void Start()
    {
        RefPlayer = FindObjectOfType<Player>().GetComponent<Player>();
        RefRenderer = GetComponent<SpriteRenderer>();
        RefSwordColliderScript = RefSwordCollider.GetComponent<SwordHitCollider>();

        if (RefSwordColliderScript == null)
        {
            Debug.LogError("Sword Collider Script not found!");
        }
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
        CheckIfKill();
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

    void CheckIfKill()
    {
        if (Input.GetMouseButtonDown(0) && RefPlayer.HoldingObject[1].Equals("Sword"))
        {
            KillTargets();
        }
    }

    void KillTargets()
    {
        List<GameObject> targets = RefSwordColliderScript.targets;
        for (int i = targets.Count; i > 0; i--) 
        {
            GameObject enemy = targets[i - 1];
            enemy.GetComponent<Enemy>().TakenDamage(1);
        }
    }
}
