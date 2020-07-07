using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class NodeArea : NodeManager
{
    protected override string prefabName { get => "AreaNumber"; }

    public override string DisplayName { get => "대피자 구역 " + PhysicalID; }

    private static Vector3 HUMANOID_DISTANCE = new Vector3(1.5f, 0, 0);

    private List<Vector2> headCount = new List<Vector2>();

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
            int currentHeadCount = 0;
            foreach(NodeArea node in GetNodesByType<NodeArea>())
            {
                currentHeadCount += node.num;
            }

            headCount.Add(new Vector2(headCount.Count, currentHeadCount));
        }
    }

    // Humanoid model 
    private GameObject humanoidPrefab;
    private GameObject[] humanoidList = new GameObject[10]; // Maximum 100.

    public float Velocity = 4;

    private TextMesh textMesh;

    protected override void Init()
    {
        textMesh = gameObject.GetComponent<TextMesh>();

        humanoidPrefab = (GameObject)Resources.Load("Prefabs/humanoid2D");
        Vector3 offset = HUMANOID_DISTANCE;
        Vector3 origin = gameObject.transform.position;
        for (int i = 0; i < humanoidList.Length; i++)
        {
            humanoidList[i] = Object.Instantiate(humanoidPrefab);

            humanoidList[i].transform.position = origin + offset;
            offset += HUMANOID_DISTANCE;
        }

        Num = 0;
    }
}
