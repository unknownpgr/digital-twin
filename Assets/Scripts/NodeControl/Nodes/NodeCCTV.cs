using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeCCTV : NodeManager
{
    protected override string prefabName => "Camera";

    public override string DisplayName { get => "CCTV:" + PhysicalID; }

    protected override void Init()
    {

    }
}
