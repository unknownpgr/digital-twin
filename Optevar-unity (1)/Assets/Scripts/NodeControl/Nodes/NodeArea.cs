using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeArea : NodeManager
{
    protected override string prefabName { get => "AreaNumber"; }

    public override string DisplayName { get => "대피자 구역 " + PhysicalID; }

    public int Num = 0;
    public float Velocity = 4;

    protected override void Init()
    {

    }
}
