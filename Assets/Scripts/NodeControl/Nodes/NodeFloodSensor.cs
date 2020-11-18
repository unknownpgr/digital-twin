using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeFloodSensor : NodeManager
{
    protected override string prefabName { get => "Sensor"; }

    public override string DisplayName { get => "수재해센서:" + PhysicalID; }

    protected override void Init()
    {
    }
}
