using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Grid3 : MonoBehaviour
{
    // Line draw values
    public List<Node3[]> FinalPaths = new List<Node3[]>();
    public List<Node3[]> MinPaths = new List<Node3[]>();
    public GameObject lineClass;
    List<GameObject> renderer = new List<GameObject>();
    public GameObject arrowLine;
    public bool ViewMinPath = false;

    public float cellRad = 0.2f; // 0.2m
    float cellDiameter;
    Vector3 minNode, maxNode;
    List<Vector3Int> nodes;
    int mx, my, mz;
    Node3[,,] grid;
    List<int> yList;
    public Dictionary<int, int> FloorDict; // height - floor
    Bresenham3D bres;


    public void CreateGrid(Vector3[] nodes, int floor)
    {
        //this.nodes = nodes;
        cellDiameter = cellRad * 2;
        
        maxNode = minNode = nodes[0];
        Vector3 tmp;
        yList = new List<int>();
        FloorDict = new Dictionary<int, int>();
        for (int i = 1; i < nodes.Length; i++)
        {
            tmp = nodes[i] - minNode;
            if (tmp.x < 0) minNode.x = nodes[i].x;
            if (tmp.y < 0) minNode.y = nodes[i].y;
            if (tmp.z < 0) minNode.z = nodes[i].z;
            tmp = nodes[i] - maxNode;
            if (tmp.x > 0) maxNode.x = nodes[i].x;
            if (tmp.y > 0) maxNode.y = nodes[i].y;
            if (tmp.z > 0) maxNode.z = nodes[i].z;
            
            
        }

        mx = Mathf.CeilToInt((maxNode.x - minNode.x) / cellDiameter);
        my = Mathf.CeilToInt((maxNode.y - minNode.y) / (cellDiameter));
        
        mz = Mathf.CeilToInt((maxNode.z - minNode.z) / cellDiameter);
        grid = new Node3[mx + 1, my + 1, mz + 1];
        tmp = maxNode - minNode;
        this.nodes = new List<Vector3Int>();
        int tmpY, tmpX, tmpZ;
        Dictionary<int, int> tmpCount = new Dictionary<int, int>();
        List<int> tmpYList;
        for (int i = 0; i < nodes.Length; i++)
        {
            
            tmpY = Mathf.CeilToInt((nodes[i].y - minNode.y) / cellDiameter);
            tmpX = Mathf.CeilToInt((nodes[i].x - minNode.x) / cellDiameter);
            tmpZ = Mathf.CeilToInt((nodes[i].z - minNode.z) / cellDiameter);
            if (!tmpCount.ContainsKey(tmpY))
            {
                tmpCount.Add(tmpY, 1);
            }
            else
            {
                tmpCount[tmpY]++;
            }
            this.nodes.Add(new Vector3Int(tmpX, tmpY, tmpZ));
        }
        // 건물 층수에 맞는 yList 계산
        tmpYList = tmpCount.Values.ToList();
        tmpYList.Sort();
        tmpYList.Reverse();
        int tmpT = tmpYList[floor];
        tmpYList = tmpCount.Keys.ToList();
        tmpYList.Sort();
        for (int i = 0; i < tmpYList.Count; i++)
        {
            if (tmpCount[tmpYList[i]] > tmpT)
            {
                yList.Add(tmpYList[i]);
                FloorDict.Add(tmpYList[i], yList.Count);
            }
        }
        yList = tmpYList;
        for (int x = 0; x <= mx; x++)
        {
            //for (int y = 0; y <= my; y++)
            {
                for (int z = 0; z <= mz; z++)
                {
                    for (int y = 0; y < tmpYList.Count; y++)
                        grid[x, tmpYList[y], z] = new Node3 (
                            minNode + new Vector3(x * tmp.x / mx, tmpYList[y] * tmp.y / my, z * tmp.z / mz));
                }
            }
        }
        mx++;my++;mz++;

    }


    // input이 되는 
    public int GetFloorFromPosition(Vector3 _pos)
    {
        if (FloorDict.ContainsKey(Mathf.CeilToInt((_pos.y - minNode.y) / cellDiameter)))
        {
            Debug.Log(FloorDict[Mathf.CeilToInt((_pos.y - minNode.y) / cellDiameter)]);
            return FloorDict[Mathf.CeilToInt((_pos.y - minNode.y) / cellDiameter)];
        }
        return -1;
    }
    public Node3 GetNodeFromPosition(Vector3 _pos)
    {
        int tmpY, tmpX, tmpZ;
        tmpY = Mathf.CeilToInt((_pos.y - minNode.y) / cellDiameter);
        tmpX = Mathf.CeilToInt((_pos.x - minNode.x) / cellDiameter);
        tmpZ = Mathf.CeilToInt((_pos.z - minNode.z) / cellDiameter);
        if ((mx > tmpX) & (my > tmpY) & (mz > tmpZ))
            return grid[tmpX, tmpY, tmpZ];
        else
            return null;
    }
    private Vector3Int GetIdxesFromPosition(Vector3 _pos)
    {
        int tmpY, tmpX, tmpZ;
        tmpY = Mathf.CeilToInt((_pos.y - minNode.y) / cellDiameter);
        tmpX = Mathf.CeilToInt((_pos.x - minNode.x) / cellDiameter);
        tmpZ = Mathf.CeilToInt((_pos.z - minNode.z) / cellDiameter);
        if ((mx > tmpX) & (my > tmpY) & (mz > tmpZ))
            return new Vector3Int(tmpX, tmpY, tmpZ);
        else
            return Vector3Int.zero;
    }
    public Node3[] GetNodesFromLine(Vector3 _a, Vector3 _b)
    {
        List<Node3> ret = new List<Node3>();
        Vector3 a = new Vector3(
            Mathf.Round(_a.x * 10f) / 10f,
            Mathf.Round(_a.y * 10f) / 10f,
            Mathf.Round(_a.z * 10f) / 10f);
        Vector3 b = new Vector3(
            Mathf.Round(_b.x * 10f) / 10f,
            Mathf.Round(_b.y * 10f) / 10f,
            Mathf.Round(_b.z * 10f) / 10f);
        Vector3Int st = GetIdxesFromPosition(a);
        Vector3Int en = GetIdxesFromPosition(b);
        bres = new Bresenham3D(st, en);
        foreach (Vector3 item in bres)
        {
            Node3 tmp = grid[(int)(item.x), (int)item.y, (int)item.z];
            if (tmp != null)
                ret.Add(tmp);
        }
        return ret.ToArray();
    }

    public float CalcTime(Node3[] path, float v, float vup, float vdown)
    {
        float ret, tn, tu, td, tmp;
        ret = tn = tu = td = tmp = 0f;
        for (int i = 0; i < path.Length - 1; i++)
        {
            tmp = Vector3.Distance(path[i].position, path[i + 1].position);
            if (path[i].y - path[i + 1].y < 0.1f)
                tn += tmp;
            else if (path[i].y > path[i + 1].y)
                td += tmp;
            else
                tu += tmp;

        }
        ret = tn * v + tu * vup + td * vdown;
        return ret;
    }
    private void OnDrawGizmos()
    {
        if (grid == null) return;
        Gizmos.DrawWireCube((maxNode + minNode) / 2, (maxNode - minNode));

        for (int i = 1; i < nodes.Count; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(grid[nodes[i].x, nodes[i].y, nodes[i].z].position, Vector3.one * (cellDiameter));
        }

        for (int x = 0; x < mx; x++)
        {

            /*
            for (int y = 0; y < my; y++)
            {
                for (int z = 0; z < mz; z++)
                {
                    Gizmos.DrawCube(grid[x,y,z].position, Vector3.one * (cellDiameter));
                }
            }
            */
            {
                for (int z = 0; z < mz; z++)
                {
                    for (int y = 0; y < yList.Count; y++)
                    {
                        switch (grid[x, yList[y], z].weight)
                        {
                            case 1:
                                Gizmos.color = Color.red;
                                break;
                            case 2:
                                Gizmos.color = Color.yellow;
                                break;
                            case 3:
                                Gizmos.color = Color.blue;
                                break;
                        }
                        if (grid[x, yList[y], z].weight >= 1)
                            Gizmos.DrawCube(grid[x, yList[y], z].position, Vector3.one * (cellDiameter));
                        //else if (grid[x, yList[y], z].weight == 0)
                        //  Gizmos.color = Color.clear;

                    }
                }
            }
        }
        
        
    }

    public void InitWeight()
    {
        for (int x = 0; x < mx; x++)
        {

            /*
            for (int y = 0; y < my; y++)
            {
                for (int z = 0; z < mz; z++)
                {
                    Gizmos.DrawCube(grid[x,y,z].position, Vector3.one * (cellDiameter));
                }
            }
            */
            {
                for (int z = 0; z < mz; z++)
                {
                    for (int y = 0; y < my; y++)
                    {
                        if (grid[x, y, z] != null)
                            grid[x, y, z].weight = 0;
                    }
                }
            }
        }
    }


    public void InitLiner()
    {
        for (int i = 0; i < renderer.Count; i++)
        {
            Destroy(renderer[i]);
        }
        renderer.Clear();
    }

    void SetLine(LineRenderer _line, Color c, float _size = 0.3f)
    {
        _line.useWorldSpace = true;
        _line.endColor = new Color(c.r, c.g, c.b, 1);
        _line.startColor = c;
        _line.material.color = c;
        _line.startWidth = _size;
        _line.endWidth = _size;
    }

    Vector3[] NodesToPos(Node3[] nodes)
    {
        List<Vector3> ret = new List<Vector3>(nodes.Length);
        for (int i = 0; i < nodes.Length; i++)
        {
            ret.Add(new Vector3(nodes[i].position.x, nodes[i].position.y + 1f, nodes[i].position.z));
        }
        return ret.ToArray();
    }
    public void ResetFinalPaths()
    {
        this.FinalPaths.Clear();
    }
    public void Liner()
    {
        InitLiner();
        if (ViewMinPath)
        {
            if (this.MinPaths != null)
            {
                for (int i = 0; i < this.MinPaths.Count; i++)
                {
                    if (MinPaths[i] != null)
                    {

                        GameObject lineTmp = GameObject.Instantiate(lineClass);
                        LineRenderer line = lineTmp.GetComponent<LineRenderer>();
                        Color c;
                        if (i == 0) c = new Color(0, 1, 0, 1f);
                        else if (i == 1) c = new Color(1, 0, 0, 1f);
                        else c = new Color(0, 0, 1, 1f);

                        SetLine(line, c);
                        line.positionCount = this.MinPaths[i].Length;
                        line.SetPositions(NodesToPos(this.MinPaths[i]));
                        line.enabled = true;
                        this.renderer.Add(lineTmp);
                        
                        //DrawArrow(MinPaths[i]);
                    }

                }
            }
        }
        else if (true)
            if (this.FinalPaths != null)
            {
                for (int i = 0; i < this.FinalPaths.Count; i++)
                {
                    if (FinalPaths[i] != null)
                    {

                        GameObject lineTmp = GameObject.Instantiate(lineClass);
                        LineRenderer line = lineTmp.GetComponent<LineRenderer>();
                        //Color c = new Color(1, 0, 0, 0.5f);
                        Color c;
                        if (i == 0) c = new Color(0, 1, 0, 1f);
                        else if (i == 1) c = new Color(1, 0, 0, 1f);
                        else c = new Color(0, 0, 1, 1f);
                        
                        SetLine(line, c, 1.2f);
                        line.positionCount = this.FinalPaths[i].Length;
                        line.SetPositions(NodesToPos(this.FinalPaths[i]));
                        line.enabled = true;
                        this.renderer.Add(lineTmp);
                        
                        //DrawArrow(FinalPaths[i]);
                    }

                }
            }

    }
    //Using Arrow Renderer
    void DrawArrow(Node3[] _nodes)
    {
        
        for (int i = 0; i < _nodes.Length - 1; i++)
        {
            GameObject lineTmp = Instantiate(arrowLine, arrowLine.transform.parent);
            ArrowRenderer ar = lineTmp.GetComponent<ArrowRenderer>();
            Vector3 st = _nodes[i].position;
            st.y += 1f;
            Vector3 en = _nodes[i + 1].position;
            en.y += 1f;
            ar.SetPositions(st, en);
            renderer.Add(lineTmp);

        }
    }
    Vector3 RoundF(Vector3 _a)
    {
        Vector3 a = new Vector3(
            Mathf.Round(_a.x * 10f) / 10f,
            Mathf.Round(_a.y * 10f) / 10f,
            Mathf.Round(_a.z * 10f) / 10f);
        return a;
    }
}