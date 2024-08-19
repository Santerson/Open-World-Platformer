using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtension
{
    public static void Shuffle<T>(this List<T> list)
    {
        if (list == null) return;
        List<T> leftStack = new List<T>();
        List<T> rightStack = new List<T>();
        for (int i = 0; i < list.Count; ++i)
        {
            //If even, then add item to left, else add to the right stack
            if (i % 2 == 0)
            {
                leftStack.Add(list[i]);
            }
            else
            {
                rightStack.Add(list[i]);
            }
        }
        list.Clear();
        while (leftStack.Count > 0 || rightStack.Count > 0)
        {
            bool isHeads = Random.value > 0.5f;
            if (isHeads && leftStack.Count > 0)
            {
                list.Add(leftStack[0]);
                leftStack.RemoveAt(0);
            }
            else if (rightStack.Count > 0)
            {
                list.Add(rightStack[0]);
                rightStack.RemoveAt(0);
            }
        }
    }

    public static void Print<T>(this List<T> list)
    {
        string output = "";
        foreach (T item in list)
        {
            output += item.ToString() + ", ";
        }
        Debug.Log(output);
    }
}
