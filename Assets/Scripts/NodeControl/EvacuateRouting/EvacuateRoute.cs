using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class is used to represent single evacuate route.
 */
public class EvacuateRoute : IComparable<EvacuateRoute>
{
    private readonly NodeArea start;
    private readonly NodeExit end;
    private readonly Vector3[] route;
    private readonly float requiredTime;
    private Texture2D screenshot;

    public NodeArea Start
    {
        get { return start; }
    }

    public NodeExit End
    {
        get { return end; }
    }

    public Vector3[] Route
    {
        get { return route; }
    }

    public float RequiredTime
    {
        get { return requiredTime; }
    }

    public Texture2D Screenshot
    {
        get { return screenshot; }
        set { screenshot = value; }
    }

    public EvacuateRoute(NodeArea start,NodeExit end,Vector3[] route, float requiredTime)
    {
        this.start = start;
        this.end = end;
        this.route = route;
        this.requiredTime = requiredTime;
    }

    public int CompareTo(EvacuateRoute obj)
    {
        if (requiredTime > obj.requiredTime) return 1;
        if (requiredTime < obj.requiredTime) return -1;
        else return 0;
    }
}
