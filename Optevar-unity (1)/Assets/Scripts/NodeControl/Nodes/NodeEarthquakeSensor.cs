using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeEarthquakeSensor : NodeManager
{
    protected override string prefabName { get => "Sensor"; }

    public override string DisplayName { get => "지진센서:" + PhysicalID; }

    protected override void Init()
    {
    }
}
