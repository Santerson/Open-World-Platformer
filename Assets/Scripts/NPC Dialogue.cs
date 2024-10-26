using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class NPCDialogue : MonoBehaviour
{
    [SerializeField] float DialogueRange = 3f;
    [SerializeField] TextMeshProUGUI BeginDialogueText = null;
    [SerializeField] List<string> DialogueText = new List<string>();

    TextMeshProUGUI DialogueOutputText = null;
    GameObject DialogueItemsParent = null;
    int timesYapped = 0;
    KeyCode BeginYapKeybind = KeyCode.None;
    KeyCode ProgressYapKeybind = KeyCode.None;
    Player RefMainPlayerScript = null;
    string baseBeginTalkToText = "Yap";
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
        else
        {
            RefMainPlayerScript = MainPlayer.GetComponent<Player>();
            BeginYapKeybind = RefMainPlayerScript.TalkToKey;
            DialogueOutputText = RefMainPlayerScript.DialogueOutputText;
            ProgressYapKeybind = RefMainPlayerScript.ProgressDialogueKey;
            DialogueItemsParent = RefMainPlayerScript.DialogueItemsObject;
        }
        if (BeginDialogueText == null)
        {
            Debug.LogError("No Talk Text found!");
        }
        else
        {
            baseBeginTalkToText = BeginDialogueText.text;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfCanYap();
        if (Yappable && !RefMainPlayerScript.IsTalking && Input.GetKeyDown(BeginYapKeybind))
        {
            ProgressYap();
        }
        if (Yappable && RefMainPlayerScript.IsTalking && Input.GetKeyDown(ProgressYapKeybind))
        {
            ProgressYap();
        }
        if (!Yappable)
        {
            DialogueItemsParent.gameObject.SetActive(false);
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
            BeginDialogueText.gameObject.SetActive(true);
            Yappable = true;
        }
        else
        {
            BeginDialogueText.gameObject.SetActive(false);
            Yappable = false;
            BeginDialogueText.SetText(baseBeginTalkToText);
        }
    }

    void ProgressYap()
    {
        DialogueItemsParent.gameObject.SetActive(true);
        if (timesYapped >= DialogueText.Count)
        {
            DialogueItemsParent.gameObject.SetActive(false);
            RefMainPlayerScript.IsTalking = false;
            timesYapped = 0;
            return;
        }
        RefMainPlayerScript.IsTalking = true;
        DialogueOutputText.SetText(DialogueText[timesYapped]);
        timesYapped++;
    }
}
