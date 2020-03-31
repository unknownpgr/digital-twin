using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeArea : NodeManager
{
    protected override string prefabName { get => "AreaNumber"; }

    public override string DisplayName { get => "대피자 구역 " + PhysicalID; }

    private int num = 0;
    public int Num
    {
        get => num;
        set
        {
            num = value;
            if (textMesh == null) textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.text = value + "";
        }
    }

    public float Velocity = 4;

    private TextMesh textMesh;

    protected override void Init()
    {
    }
}
