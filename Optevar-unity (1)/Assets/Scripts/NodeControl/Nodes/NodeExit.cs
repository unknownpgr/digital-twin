using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
이 스크립트는 화재 센서에 해당하는 스크립트를 구현한 것이다.
*/

public class NodeExit : NodeManager
{
    protected override string prefabName { get => "Sensor"; }

    public override string DisplayName { get => "탈출구" + PhysicalID; }

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
