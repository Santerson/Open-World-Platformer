using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hotbar : MonoBehaviour
{
    [SerializeField] KeyCode Item1Slot = KeyCode.Alpha1;
    [SerializeField] KeyCode Item2Slot = KeyCode.Alpha2;
    [SerializeField] KeyCode Item3Slot = KeyCode.Alpha3;
    /// <summary>
    /// Hotbar, full of 2d list
    /// STRING 1: object name
    /// STRING 2: object classification
    /// </summary>
    public List<List<string>> HotbarContent = new List<List<string>>();
    public List<string> HeldItem;

    // Start is called before the first frame update
    void Start()
    {
        //Temporary code to be overridden once the game is created
        HotbarContent.Add(new List<string> { "Sword" , "Sword"});
        HotbarContent.Add(new List<string> { "Bow" , "Bow"});
        HotbarContent.Add(new List<string> { "Shield" , "Shield"});
        HeldItem = new List<string>() { "", ""};
    }

    // Update is called once per frame
    void Update()
    {
        CheckForHotbarInput();
        Debug.Log(HeldItem);
    }

    void CheckForHotbarInput()
    {
        //Sets the held item to the coreresponding keycode if it is being pressed
        if (Input.GetKeyDown(Item1Slot))
        {
            SetHeldItem(0);
        }
        if (Input.GetKeyDown(Item2Slot))
        {
            SetHeldItem(1);
        }
        if (Input.GetKeyDown(Item3Slot))
        {
            SetHeldItem(2);
        }
    }

    /// <summary>
    /// Pulls an item from the hotbar and returns it.
    /// Returns an empty string if the item is the held item
    /// </summary>
    /// <param name="index">the index in the hotbar that the function should return</param>
    /// <returns></returns>
    List<string> PullFromHotbar(int index)
    {
        List<string> content = HotbarContent[index];
        if (content.Equals(HeldItem)) {
            return new List<string> { "", ""};
        }
        return content;
    }

    void SetHeldItem(int index)
    {
        HeldItem = PullFromHotbar(index);
    }
}
