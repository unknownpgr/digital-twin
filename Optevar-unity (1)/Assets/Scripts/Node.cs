using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompNodes : IComparer<Node>
{    public int Compare(Node a, Node b)
    {
        if (a.FCost < b.FCost || a.FCost == b.FCost && a.hCost < b.hCost) return -1;
        else if (a.FCost == b.FCost) return 0;
        else return 1;
    }
}

public class CompAdjNodes: IComparer<Node>
{
    Dictionary<Node, float> dists;
    public CompAdjNodes(Dictionary<Node, float> d)
    {
        this.dists = d;
    }
    public void SetDistances(Dictionary<Node, float> d)
    {
        dists = d;
    }
    public int Compare(Node a, Node b)
    {
        if (dists[a] == dists[b]) return 0;
        else if (dists[a] < dists[b]) return -1;
        else return 1;
    }
}

public class ParentNode : System.IComparable<ParentNode>
{
    Node parentNode;
    Node targetNode;
    float regidualDistance = 100000f;
    public int CompareTo(ParentNode a)
    {
        if (this.GetRegidualDistance() == a.GetRegidualDistance())
            return 0;
        else if (this.GetRegidualDistance() < a.GetRegidualDistance())
            return -1;
        else return 1;
    }
    public void SetParentNode(Node _node)
    {
        this.parentNode = _node;
    }

    public Node GetParentNode()
    {
        return this.parentNode;
    }
    public void SetTargetNode(Node _target)
    {
        this.targetNode = _target;
    }
    public Node GetTargetNode()
    {
        return this.targetNode;
    }
    public void SetRegidualDistance(float _dist)
    {
        this.regidualDistance = _dist;
    }
    /*public  void SetDistanceTo(Node _target)
    {
        this.distance = this.GetDistance();
    }*/
    public float GetRegidualDistance()
    {
        return this.regidualDistance;
    }
}

public class Node {
    public int gridX, gridY;
    public int nodeId;
    public bool IsWall;
    public bool IsWallAround = false;
    public bool IsDanger = false;
    public bool IsClosed = false;
    public bool IsCorner = false;

    
    public Vector3 Position;
    public Node Parent; // Use when returning paths.
    public List<ParentNode> Parents;
    //public List<AdjNode> AdjNodes;
    public Node[] adjNodes;
    public List<AdjNode> adjDistances;
    public bool isAdjSorted;

    public float weight;

    public int kindSex;
    public int kindAge;
    public int numbers;

    public float gCost;
    public float hCost;

    public float FCost {get {return gCost + hCost;}}
    public void InitCost()
    {
        gCost = 0f;
        hCost = 0f;
        IsClosed = false;
        this.Parent = null;
    }
    public Node(bool a_IsWall, Vector3 a_Pos, int a_gridX, int a_gridY) {
        IsWallAround = false;
        IsWall = a_IsWall;
        Position = a_Pos;
        gridX = a_gridX;
        gridY = a_gridY;
        Parents = new List<ParentNode>();
        adjDistances = new List<AdjNode>();
        isAdjSorted = false;
    }

    public void UpdateWeight()
    {
        // adjNodes가 존재하고, 자신이 센서 노드일 때
        // adjNodes 중 위험한 노드에 표시하는 메소드.

    }
    public void AddAdjNode(Node node)
    {
        AdjNode tmp = new AdjNode(node, node.DistanceTo(this));
        adjDistances.Add(tmp);
        isAdjSorted = false;
    }
    public void ClearAdjNode()
    {
        adjNodes = null;
        adjDistances.Clear();
        isAdjSorted = false;
    }
    public void SortAdjNodes()
    {
        this.adjDistances.Sort(new CompDist());
        this.adjNodes = this.GetNodes();
        isAdjSorted = true;
    }
    public Node[] GetAdjNodesSorted()
    {
        if (isAdjSorted) return adjNodes;
        else
        {
            SortAdjNodes();
            return adjNodes;
        }
    }
    public Node()
    {
    }

    public int ParentContains(Node _node)
    {
        for (int i = 0; i < this.Parents.Count; i++)
        {
            if (this.Parents[i] != null)
                if (_node == this.Parents[i].GetParentNode())
                {
                    return i;
                }
        }
        return -1;
    }

    public Node[] GetPathTo(Node _target)
    {
        List<Node> path = new List<Node>();
        for (int t = 0; t < this.Parents.Count; t++)
        {
            if (this.Parents[t].GetTargetNode() == _target)
            {
                Node tmp = this;
                while (tmp != _target)
                {
                    if (tmp != null)
                    {
                        path.Add(tmp);
                        tmp = tmp.Parents[t].GetParentNode();
                    }
                    else
                    {
                        return null;
                    }
                    
                }
                path.Add(_target);
            }
        }
        return path.ToArray();
    }
    public float DistanceTo(Node _target)
    {
        float dist = Vector2.Distance(new Vector2(this.gridX, this.gridY), new Vector2(_target.gridX, _target.gridY));
        return dist;
    }
    public void InitParents()
    {
        for (int i = 0; i < this.Parents.Count; i++)
            this.Parents[i] = null;
    }
    Node[] GetNodes()
    {
        Node[] ret = new Node[this.adjDistances.Count];
        for (int i = 0; i < this.adjDistances.Count; i++)
            ret[i] = this.adjDistances[i].node;
        return ret;
    }
}

public class AdjNode
{
    public Node node;
    public float distance;
    public AdjNode(Node n, float d)
    {
        this.node = n;
        this.distance = d;
    }
}
public class CompDist : IComparer<AdjNode>
{
    public int Compare(AdjNode a, AdjNode b)
    {
        if (a.distance == b.distance) return 0;
        else if (a.distance < b.distance) return -1;
        else return 1;
    }
}