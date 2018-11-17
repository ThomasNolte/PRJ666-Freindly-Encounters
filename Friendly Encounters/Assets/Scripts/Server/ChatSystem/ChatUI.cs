﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    public GameObject MessagePrefab = null;
    public Transform ChatPanel = null;
    public ScrollRect scrollRect;

    private List<GameObject> cacheMessages = new List<GameObject>();

    void Update()
    {
        scrollRect.verticalNormalizedPosition = 0;
    }

    public void AddNewLine(string text)
    {
        GameObject newline = Instantiate(MessagePrefab) as GameObject;
        newline.GetComponent<Text>().text = text;
        newline.GetComponent<LayoutElement>().CalculateLayoutInputVertical();
        newline.GetComponent<LayoutElement>().CalculateLayoutInputHorizontal();
        newline.transform.SetParent(ChatPanel, false);
        cacheMessages.Add(newline);
    }

    public void Clean()
    {
        foreach (GameObject g in cacheMessages)
        {
            Destroy(g);
        }
        cacheMessages.Clear();
    }
}
