﻿using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class NodeArea : NodeManager
{
    private static Vector3 HUMANOID_DISTANCE = new Vector3(1.5f, 0, 0);
    private static int totalHeadCount = 0;
    private static List<Vector2> headCount = new List<Vector2>();
    private static DateTime baseTime = DateTime.MinValue;

    protected override string prefabName { get => "AreaNumber"; }

    public override string DisplayName { get => "대피자 구역 " + PhysicalID; }

    // Do not directly access here.
    private int num = 0;

    [JsonIgnore]
    public int Num
    {
        get => num;
        set
        {
            num = value;
            textMesh.text = value + "";

            int visibleNumber = value / 10;
            if (visibleNumber >= humanoidList.Length) visibleNumber = humanoidList.Length - 1;
            for (int i = 0; i < humanoidList.Length; i++)
            {
                humanoidList[i].SetActive(i < visibleNumber);
                humanoidList[i].transform.localScale = Vector3.one;
            }
            humanoidList[visibleNumber].SetActive(value % 10 > 0);
            humanoidList[visibleNumber].transform.localScale = Vector3.one * (value % 10 > 5 ? 1 : .75f);

            // Update list
            totalHeadCount = 0;
            foreach (NodeArea node in GetNodesByType<NodeArea>()) totalHeadCount += node.num;
            if (baseTime == DateTime.MinValue) baseTime = DateTime.Now;
            headCount.Add(new Vector2((float)((DateTime.Now - baseTime).TotalMilliseconds), totalHeadCount));

            GraphManager.StaticSetGraph(headCount);
        }
    }

    // Humanoid model 
    private GameObject humanoidPrefab;
    private GameObject[] humanoidList = new GameObject[10]; // Maximum 100.

    public float Velocity = 4;

    private TextMesh textMesh;

    protected override void Init()
    {
        if (FunctionManager.BuildingName == "노유자시설")
        {
            gameObject.transform.localScale = new Vector3(0.16f, 0.16f, 0.5000002f);
        }

        textMesh = gameObject.GetComponent<TextMesh>();

        humanoidPrefab = (GameObject)Resources.Load("Prefabs/humanoid2D");
        Vector3 offset = HUMANOID_DISTANCE;
        Vector3 origin = gameObject.transform.position;
        for (int i = 0; i < humanoidList.Length; i++)
        {
            humanoidList[i] = UnityEngine.Object.Instantiate(humanoidPrefab);

            humanoidList[i].transform.position = origin + offset;
            offset += HUMANOID_DISTANCE;
        }

        Num = 0;
    }
}