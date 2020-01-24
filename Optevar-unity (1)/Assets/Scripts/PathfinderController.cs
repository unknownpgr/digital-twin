using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class PathfinderController : MonoBehaviour
{

    public Transform StartPos;
    Node StartNode;
    public int KindofStartNodeSex = 1;
    public int KindofStartNodeAge = 0;
    public Transform Targets;
    List<Vector3> targetPositions;
    public CustomGrid grid;
    public float refVelo = 1.0f; // m/s
    public WeightCalc wc = new WeightCalc();
    long perform = 0;
    int performCount = 0;
    public int MaxOpenNodes = 100000;
    Pathfinder pathfinder;



    //public int MaxClosedNodes = 

    private void Start()
    {
        // Set start node
        //StartNode = grid.NodeFromWorldPosition(StartPos.position);
        //StartNode.kindSex = KindofStartNodeSex;
        //StartNode.kindAge = KindofStartNodeAge;
        SetTargetNodes();
        grid.StorePaths(grid.GetTargetNodes());
    }
    private void Awake() {
        grid = GetComponent<CustomGrid>();
        pathfinder = ScriptableObject.CreateInstance<Pathfinder>();
        // set position of targets
        targetPositions = new List<Vector3>();
        for (int i = 0; i < Targets.childCount; i++)
        {
            targetPositions.Add(Targets.GetChild(i).position);
        }

        

        wc.init();
        wc.referenceVelocity = refVelo;

    }
    void Update()
    {
        //Calc();
    }
    public void Calc(Vector3 StartPosition) {
        //Debug.Log("Position of StartNode: " + grid.NodeFromWorldPosition(StartPos.position).gridX + ", " + grid.NodeFromWorldPosition(StartPos.position).gridY);
        grid.ResetFinalPaths();
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        //pathfinder.FindPaths(grid.NodeFromWorldPosition(StartPos.position), grid.TargetNodes, grid, grid.GetAdjNodes, false);
        grid.AddPath(grid.GetStoredPathFromPosition(StartPosition));
        sw.Stop();
        perform += sw.ElapsedMilliseconds;
        performCount++;
        Debug.Log(sw.ElapsedMilliseconds.ToString() + "ms");
        //Debug.Log("Distance is " + grid.GetDistance().ToString() + "m.");
        //Debug.Log("Velocity is " + wc.GetVelocity(StartNode) + "m/s.");
        //Debug.Log("Evac time: " + (grid.GetDistance() * wc.GetVelocity(StartNode)) + "sec.");
        Debug.Log("Perform: " + perform / performCount);
    }

    public void Calc(Evacuaters Evacs)
    {
        //Debug.Log("Position of StartNode: " + grid.NodeFromWorldPosition(StartPos.position).gridX + ", " + grid.NodeFromWorldPosition(StartPos.position).gridY);
        //grid.ResetFinalPaths();
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        List<Node[]> tmpL = new List<Node[]>();
        for (int i = 0; i < grid.TargetNodes.Count; i++)
        {
            tmpL.Add(grid.GetStoredPathFromPosition(Evacs.GetPosition(), i));
            
        }
        Evacs.SetPaths(tmpL.ToArray());
        //grid.AddPath(Evacs.GetPath(0));
        sw.Stop();

        perform += sw.ElapsedMilliseconds;
        performCount++;
        Debug.Log(sw.ElapsedMilliseconds.ToString() + "ms");
        Debug.Log("Perform: " + perform / performCount);
    }
    public void Calc(Evacuaters Evacs, int exitNodeIdx)
    {
        //Debug.Log("Position of StartNode: " + grid.NodeFromWorldPosition(StartPos.position).gridX + ", " + grid.NodeFromWorldPosition(StartPos.position).gridY);
        //grid.ResetFinalPaths();
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        List<Node[]> tmpL = new List<Node[]>();
        tmpL.Add(grid.GetStoredPathFromPosition(Evacs.GetPosition(), exitNodeIdx));
        Evacs.SetPaths(tmpL.ToArray());
        grid.AddPath(Evacs.GetPath(0));
        sw.Stop();

        perform += sw.ElapsedMilliseconds;
        performCount++;
        Debug.Log(sw.ElapsedMilliseconds.ToString() + "ms");
        Debug.Log("Perform: " + perform / performCount);
    }

    public void SetTargetNodes(List<Vector3> _targets)
    {
        grid.TargetNodes.Clear();
        for (int i = 0; i < _targets.Count; i++)
        {
            grid.AddNode(grid.NodeFromWorldPosition(_targets[i]));
            grid.AddTargetNode(grid.NodeFromWorldPosition(_targets[i]));
        }
    }

    public void SetTargetNodes()
    {
        grid.TargetNodes.Clear();
        for (int i = 0; i < targetPositions.Count; i++)
        {
            grid.AddNode(grid.NodeFromWorldPosition(targetPositions[i]));
            grid.AddTargetNode(grid.NodeFromWorldPosition(targetPositions[i]));
        }
    }

    // Find shortest path to nearest exit. It takes multiple exits - and selects nearest exit.
    public float GetNodeVelocity()
    {
        return wc.GetNodeVelocity(StartNode, grid.distance);
    }

    

}
