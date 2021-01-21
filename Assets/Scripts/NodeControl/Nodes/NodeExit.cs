using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeExit : NodeManager
{
    protected override string prefabName { get => "ExitSign"; }

    public override string DisplayName { get => "탈출구:" + PhysicalID; }

    protected override void Init()
    {
    }
}
