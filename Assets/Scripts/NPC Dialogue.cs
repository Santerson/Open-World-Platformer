using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class NPCDialogue : MonoBehaviour
{
    [SerializeField] float DialogueRange = 3f;
    [SerializeField] TextMeshProUGUI TalkText = null;
    [SerializeField] List<string> DialogueText = new List<string>();

    int timesYapped = 0;
    string baseTalkText = "Yap";
    GameObject MainPlayer = null;
    Vector2 PlayerPos;

    bool Yappable = false;

    // Start is called before the first frame update
    void Start()
    {
        MainPlayer = FindObjectOfType<Player>().transform.gameObject;
        if (MainPlayer == null)
        {
            Debug.LogError("No player found!");
        }
        if (TalkText == null)
        {
            Debug.LogError("No Talk Text found!");
        }
        baseTalkText = TalkText.text;
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfCanYap();
        if (Yappable && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Yap");
            TalkText.SetText(DialogueText[timesYapped]);
            timesYapped++;
            if (timesYapped >= DialogueText.Count)
            {
                timesYapped = 0;
            }
        }
    }

    void CheckIfCanYap()
    {
        if (MainPlayer == null)
        {
            return;
        }

        //find the player pos
        PlayerPos = MainPlayer.transform.position;

        //take the current position of the npc
        Vector2 pos = transform.position;
        //create a box that is dialogueRange distance away from it
        Vector2 topLeft = new Vector2(pos.x - DialogueRange, pos.y + DialogueRange);
        Vector2 bottomRight = new Vector2(pos.x + DialogueRange, pos.y - DialogueRange);
        DebugExtensions.DrawBox(topLeft, bottomRight, Color.red);
        //if player in that box
        if (SUtilities.PointInBox(PlayerPos, topLeft, bottomRight))
        {
            TalkText.gameObject.SetActive(true);
            Yappable = true;
        }
        else
        {
            TalkText.gameObject.SetActive(false);
            Yappable = false;
            TalkText.SetText(baseTalkText);
        }
    }
}
