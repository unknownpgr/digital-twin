using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeArea : NodeManager
{
    protected override string prefabName { get => "Sensor"; }

    public override string DisplayName { get => "대피자 구역 " + PhysicalID; }

    public int Num = 0;
    public float Velocity = 4;

    protected override void Init()
    {

    }

    protected override void DictToProperty(Dictionary<string, string> dict)
    {

    }

    protected override Dictionary<string, string> PropertyToDict()
    {
        return new Dictionary<string, string>();
    }
}
