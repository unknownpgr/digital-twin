﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeExit : NodeManager
{
    protected override string prefabName { get => "Sensor"; }

    public override string DisplayName { get => "탈출구 " + PhysicalID; }

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
