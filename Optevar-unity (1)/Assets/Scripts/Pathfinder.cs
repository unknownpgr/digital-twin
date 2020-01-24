using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class Pathfinder : ScriptableObject
{
    public void FindPaths(Node a_sn, List<Node> a_tn, CustomGrid grid, System.Func<Node, Node[]> GetNeighbors, bool IsManhatten = true)
    {
        System.Func<Node, Node, float> calc;
        if (IsManhatten) calc = GetManhattenDistance_weighted;
        else calc = GetDistance_weighted;
        
        List<Node[]> paths = new List<Node[]>(a_tn.Count);
        for (int t = 0; t < a_tn.Count; t++)
        {
            paths.Add(FindPath(grid, a_sn, a_tn[t], GetNeighbors, calc, IsManhatten));
        }
        if (paths.Count > 0)
        {
            float tmp = GetDistanceOfPath(paths[0]);
            float tmp2 = 0;
            int idx = 0;

            for (int i = 1; i < paths.Count; i++)
            {
                tmp2 = GetDistanceOfPath(paths[i]);
                if (tmp > tmp2)
                {
                    idx = i;
                    tmp = tmp2;
                }
            }
            //grid.FinalPath = paths[idx];
            grid.AddPath(paths[idx]);
        } else
        {
            Debug.Log("There isnt path.");
        }
        
    }

    float GetManhattenDistance(Node a_nodeA, Node a_nodeB)
    {
        int ix = Mathf.Abs(a_nodeA.gridX - a_nodeB.gridX);
        int iy = Mathf.Abs(a_nodeA.gridY - a_nodeB.gridY);

        return ix + iy;
    }
    float GetManhattenDistance_weighted(Node a_nodeA, Node a_nodeB)
    {
        int ix = Mathf.Abs(a_nodeA.gridX - a_nodeB.gridX);
        int iy = Mathf.Abs(a_nodeA.gridY - a_nodeB.gridY);
        return ix + iy + a_nodeA.weight + a_nodeB.weight;
    }

    float GetDistance_weighted(Node a, Node b)
    {
        float ret = a.DistanceTo(b) + a.weight + b.weight;
        
        return ret;
    }
    public Node[] FindPath(CustomGrid grid, Node StartNode, Node TargetNode, System.Func<Node, Node[]> GetNeighbors, System.Func<Node, Node, float> CalcDistance, bool IsManhatten = true)
    {
        if (GetNeighbors == null) GetNeighbors = grid.GetAdjNodes;
        if (CalcDistance == null) CalcDistance = this.GetDistance_weighted;
        Node[] ret = null;
        Node[] tmpNodeList;
        Node tmpNode;
        Node CurrentNode;
        grid.InitCost();
        ListPriorityQueue OpenList = new ListPriorityQueue(1000);
        OpenList.Enqueue(StartNode);

        while (OpenList.Count() > 0)
        {
            CurrentNode = OpenList.Dequeue();

            CurrentNode.IsClosed = true;
            if (CurrentNode == TargetNode)
            {
                ret = GetFinalPath(StartNode, TargetNode);
                OpenList.Clear();
                continue;
            }
            tmpNodeList = GetNeighbors(CurrentNode);
            for (int nn = 0; nn < tmpNodeList.Length; nn++)
            {
                tmpNode = tmpNodeList[nn];
                if (tmpNode.IsDanger || tmpNode.IsWall || tmpNode.IsClosed)
                {
                    continue;
                }
                float MoveCost = CurrentNode.gCost + CalcDistance(CurrentNode, tmpNode);

                if (MoveCost < tmpNode.gCost || !OpenList.Contains(tmpNode))
                {
                    tmpNode.gCost = MoveCost;
                    tmpNode.hCost = CalcDistance(tmpNode, TargetNode);
                    tmpNode.Parent = CurrentNode;
                    OpenList.Enqueue(tmpNode);
                }

            }

        }
        return ret;
    }

    public Node[] FindPathUsingRaycast(CustomGrid grid, Node StartNode, Node TargetNode, System.Func<Node, Node[]> GetNeighbors, System.Func<Node, Node, float> CalcDistance, bool IsManhatten = true)
    {
        if (GetNeighbors == null) GetNeighbors = grid.GetAdjNodes;
        if (CalcDistance == null) CalcDistance = this.GetDistance_weighted;
        Node[] ret = null;
        Node[] tmpNodeList;
        Node tmpNode;
        Node CurrentNode;
        grid.InitCost();
        ListPriorityQueue OpenList = new ListPriorityQueue(1000);
        OpenList.Enqueue(StartNode);

        while (OpenList.Count() > 0)
        {
            CurrentNode = OpenList.Dequeue();

            CurrentNode.IsClosed = true;
            if (CurrentNode == TargetNode)
            {
                ret = GetFinalPath(StartNode, TargetNode);
                OpenList.Clear();
                continue;
            }
            tmpNodeList = GetNeighbors(CurrentNode);
            for (int nn = 0; nn < tmpNodeList.Length; nn++)
            {
                tmpNode = tmpNodeList[nn];
                if (tmpNode.IsDanger || tmpNode.IsWall || tmpNode.IsClosed)
                {
                    continue;
                }
                // Use raycast
                Ray ray = new Ray(CurrentNode.Position, (tmpNode.Position - CurrentNode.Position));
                RaycastHit hit;
                // if not casted. (When there is no obstacle)
                if (!Physics.Raycast(ray, out hit, Vector3.Distance(CurrentNode.Position, tmpNode.Position)))
                {
                    float MoveCost = CurrentNode.gCost + CalcDistance(CurrentNode, tmpNode);

                    if (MoveCost < tmpNode.gCost || !OpenList.Contains(tmpNode))
                    {
                        tmpNode.gCost = MoveCost;
                        tmpNode.hCost = CalcDistance(tmpNode, TargetNode);
                        tmpNode.Parent = CurrentNode;
                        OpenList.Enqueue(tmpNode);
                    }
                }

            }

        }
        return ret;
    }
    public float GetDistanceOfPath(Node[] _path)
    {
        float ret = 0;
        for (int i = 1; i < _path.Length; i++)
        {
            ret += Vector3.Distance(_path[i - 1].Position, _path[i].Position);
        }
        return ret;
    }
    Node[] GetFinalPath(Node a_sn, Node a_en)
    {
        List<Node> FinalPath = new List<Node>();

        Node CurrentNode = a_en;

        while (CurrentNode != a_sn)
        {
            FinalPath.Add(CurrentNode);
            CurrentNode = CurrentNode.Parent;
        }
        FinalPath.Add(CurrentNode);
        FinalPath.Reverse();

        return FinalPath.ToArray();
    }



    
   
}