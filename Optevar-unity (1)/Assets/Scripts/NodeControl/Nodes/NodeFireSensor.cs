using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeFireSensor : NodeManager
{
    protected override string prefabName { get => "TEST_PREFAB_NAME"; }
    public override string NodeType { get => "TEST_FIRE_SENSOR"; }

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
