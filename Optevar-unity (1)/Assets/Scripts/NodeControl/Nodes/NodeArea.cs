using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class NodeArea : NodeManager
{
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

            // Like candles of birthday cake.
            int visibleNumber = value / 10;
            if (visibleNumber >= humanoidList.Length) visibleNumber = humanoidList.Length - 1;
            for (int i = 0; i < humanoidList.Length; i++)
            {
                humanoidList[i].SetActive(i < visibleNumber);
                humanoidList[i].transform.localScale = Vector3.one;
            }
            humanoidList[visibleNumber].SetActive(value % 10 > 0);
            humanoidList[visibleNumber].transform.localScale = Vector3.one * (value > 5 ? 1 : .5f);
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
        Vector3 distance = new Vector3(1.5f, 0, 0);
        Vector3 offset = distance;
        Vector3 origin = gameObject.transform.position;
        for (int i = 0; i < humanoidList.Length; i++)
        {
            humanoidList[i] = (GameObject)GameObject.Instantiate(humanoidPrefab);
            humanoidList[i].transform.position = origin + offset;
            offset += distance;
        }

        Num = 0;
    }
}
