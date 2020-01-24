using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CustomGrid : MonoBehaviour
{
    public Transform StartPosition;
    public new Transform transform;
    public LayerMask WallMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public Transform GridPostion;
    public float distance;
    public float ThresholdOfWeight = 5f;
    public float ConstantOfDistance = 5f;
    public List<Node> nodes; // Corner + Exit
    public Dictionary<int, Node> Sensors = new Dictionary<int, Node>();
    public List<Node> TargetNodes = new List<Node>();
    public List<Node[]> FinalPaths = new List<Node[]>();
    public List<Node[]> MinPaths = new List<Node[]>();
    public bool ViewMinPath = false;
    List<GameObject> renderer = new List<GameObject>();
    public GameObject lineClass;
    float nodeDiameter;
    int gridSizeX, gridSizeY;
    Pathfinder pathfinder;
    Node[,] grid;
    int sensorIdSeq = 0;
    bool isPathUpdated = false;
    List<Node> updated = new List<Node>();
    Node tmpNode;


    private void Awake() {
        transform = this.GetComponent<Transform>();
        //renderer = this.GetComponent<LineRenderer>();
        if (lineClass == null)
            lineClass = GameObject.Find("Line");
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        pathfinder = ScriptableObject.CreateInstance<Pathfinder>();
        CreateGrid();
    }

    public int GetSensorSequence()
    {
        return this.sensorIdSeq;
    }
    public Node[] GetAdjNodes(Node _node)
    {
        return _node.GetAdjNodesSorted();
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 bottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                grid[x, y] = new Node(false, worldPoint, x, y);
            }
        }
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                if (Physics.CheckSphere(worldPoint, nodeRadius, WallMask))
                {
                    grid[x, y].IsWall = true;
                    //grid[x, y].IsWallAround = false;
                    // Set around
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            //if (i != 0  j != 0)
                            //{
                            if (x + i > 0 & x + i < this.gridSizeX)
                                if (y + j > 0 & y + j < this.gridSizeY) 
                                    grid[x + i, y + j].IsWallAround = true;
                            //}
                        }
                    }
                }
            }
        }
        // Set Nodes: corners, exits

        InitCornerNodes();



        
    }

    private void InitCornerNodes()
    {
        nodes = new List<Node>();
        
        for (int y = 1; y < gridSizeY - 1; y++)
        {
            for (int x = 1; x < gridSizeX - 1; x++)
            {
                if (CheckCorner(grid[x, y]))
                {
                    grid[x, y].IsCorner = true;
                    nodes.Add(grid[x, y]);

                    /*grid[x, y+1].IsCorner = true;
                    grid[x, y - 1].IsCorner = true;
                    grid[x + 1, y].IsCorner = true;
                    grid[x - 1, y].IsCorner = true;
                    */
                }
            }
        }

        /*
        for (int y = 1; y < gridSizeY - 1; y++)
        {
            for (int x = 1; x < gridSizeX - 1; x++)
            {
                if (grid[x, y].IsCorner)
                {
                    nodes.Add(grid[x, y]);
                }
            }
        }
        */

        SetAdjNodes(nodes);
        
    }
    void SetAdjNodes(List<Node> _nodes)
    {
        // Raycast test (edging)
        for (int i = 0; i < _nodes.Count - 1; i++)
        {
            for (int j = i + 1; j < _nodes.Count; j++)
            {
                if (IsRayPath(_nodes[i], _nodes[j]))
                {
                    // Link
                    _nodes[i].AddAdjNode(_nodes[j]);
                    _nodes[j].AddAdjNode(_nodes[i]);
                }
                else
                {
                    //
                }
            }
        }
    }
    void SetSensorAdjNode(Node _sensor, List<Node> _nodes)
    {
        // Raycast test (edging)
        for (int i = 0; i < _nodes.Count - 1; i++)
        {
                if (IsRayPath(_nodes[i], _sensor))
                {
                    // Link
                    _sensor.AddAdjNode(_nodes[i]);
                }
                else
                {
                    //
                }

        }
    }
    public void InitSensors()
    {
        this.Sensors.Clear();
    }
    public void AddSensor(int _id, Node _sensor)
    {
        this.Sensors.Add(_id, _sensor);
        this.SetSensorAdjNode(_sensor, this.nodes);
    }
    public void AddSensor(Node _sensor)
    {
        sensorIdSeq++;
        this.Sensors.Add(sensorIdSeq, _sensor);
        this.SetSensorAdjNode(_sensor, this.nodes);
        

    }


    public void StorePaths()
        // It uses this.TargetNodes
    {
        Node[] exits = this.TargetNodes.ToArray();
        for (int j = 0; j < nodes.Count; j++)
        {
            nodes[j].Parents = new List<ParentNode>();
            for (int i = 0; i < exits.Length; i++)
                nodes[j].Parents.Add(null);
        }
        for (int i = 0; i < exits.Length; i++)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].Parents[i] == null)
                {
                    // Find path and store parents.
                    Node[] path = pathfinder.FindPath(this, nodes[j], exits[i], null, null);
                    // Set regidual distance & parent node.
                    float distance = 0f;
                    for (int o = 0; o < path.Length - 1; o++)
                    {
                        distance += path[o].DistanceTo(path[o + 1]);
                    }
                    for (int o = 0; o < path.Length - 1; o++)
                    {
                        path[o].Parents[i] = new ParentNode();
                        path[o].Parents[i].SetParentNode(path[o + 1]);
                        path[o].Parents[i].SetRegidualDistance(distance);
                        path[o].Parents[i].SetTargetNode(exits[i]);
                        distance -= path[o].DistanceTo(path[o + 1]);
                    }
                }
                else
                {
                    // continue
                }
            }

        }
        isPathUpdated = true;

    }
    public void StorePaths(Node[] exits)
    {

        for (int j = 0; j < nodes.Count; j++)
        {
            nodes[j].Parents = new List<ParentNode>();
            for (int i = 0; i < exits.Length; i++)
                nodes[j].Parents.Add(null);
        }

        for (int i = 0; i < exits.Length; i++)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].Parents[i] == null)
                {
                    // Find path and store parents.
                    Node[] path = pathfinder.FindPath(this, nodes[j], exits[i], null, null);
                    if (path == null)
                    {
                        nodes[j].IsDanger = true;
                        continue;

                    }
                    // Set regidual distance & parent node.
                    float distance = 0f;
                    for (int o = 0; o < path.Length - 1; o++)
                    {
                        distance += path[o].DistanceTo(path[o + 1]);
                    }
                    for (int o = 0; o < path.Length - 1; o++)
                    {
                        path[o].Parents[i] = new ParentNode();
                        path[o].Parents[i].SetParentNode(path[o + 1]);
                        path[o].Parents[i].SetRegidualDistance(distance);
                        path[o].Parents[i].SetTargetNode(exits[i]);
                        distance -= path[o].DistanceTo(path[o + 1]);
                    }
                }
                else
                {
                    // continue
                }
                
            }
            exits[i].Parents[i] = new ParentNode();
            exits[i].Parents[i].SetRegidualDistance(0f);
            exits[i].Parents[i].SetTargetNode(exits[i]);
            /*
            if (i == exits.Length - 1)
            {
                // Sort the parent nodes
                for (int j = 0; j < nodes.Count; j++)
                {
                    nodes[j].Parents.Sort();
                }
            }
            */
        }
        isPathUpdated = true;

    }

    public void UpdatePaths()
    {
        // Use this.TargetNodes.
        // It updates weighted node only.
        // 1. Delete relative paths (Init)
        // 2. Re-calculate paths.
        Node[] exits = this.TargetNodes.ToArray();

        // 1. Delete relative paths
        for (int i = 0; i < this.updated.Count; i++)
        {
            for (int j = 0; j < exits.Length; j++)
                this.updated[i].Parents[j] = null;
            for (int j = 0; j < this.nodes.Count; j++)
            {
                int tmp = nodes[j].ParentContains(this.updated[i]);
                //if (tmp >= 0)
                if (tmp >= 0)
                for (int o = 0; o < nodes[j].Parents.Count; o++)
                {
                    nodes[j].Parents[o] = null;

                }
            }
        }

        // 2. Re-calculate paths with weighted graph.
        // TODO: 원형의 영향권 내부를 통과하는 path에 대해서도 초기화하여야한다.
        for (int i = 0; i < exits.Length; i++)
        {

            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].Parents[i] == null)
                {
                    // Find path and store parents.
                    Node[] path = pathfinder.FindPathUsingRaycast(this, nodes[j], exits[i], null, null);
                    if (path == null)
                    {
                        //nodes[j].IsDanger = true;
                        nodes[j].Parents[i] = new ParentNode();
                        continue;

                    }
                    // Set regidual distance & parent node.
                    float distance = 0f;
                    for (int o = 0; o < path.Length - 1; o++)
                    {
                        distance += path[o].DistanceTo(path[o + 1]);
                    }
                    for (int o = 0; o < path.Length - 1; o++)
                    {
                        path[o].Parents[i] = new ParentNode();
                        path[o].Parents[i].SetParentNode(path[o + 1]);
                        path[o].Parents[i].SetRegidualDistance(distance);
                        path[o].Parents[i].SetTargetNode(exits[i]);
                        distance -= path[o].DistanceTo(path[o + 1]);
                    }
                }
                else
                {
                    // continue
                }
            }
        }

        this.updated.Clear();
        isPathUpdated = true;
        
        
    }
    public void GetGrid(CustomGrid _grid)
    {
        _grid = this;
    }

    // 센서 ID를 받아, 해당 센서의 가중치를 업데이트하고 기준을 넘었다면 주변 노드에 전파한다.
    public void UpdateWeight(int _id, float _value)
    {
        Node tmp = this.Sensors[_id];
        tmp.weight = _value;
        if (_value > this.ThresholdOfWeight)
        {
            tmp.SortAdjNodes();
            for (int i = 0; i < tmp.adjNodes.Length; i++)
            {
                if (tmp.adjDistances[i].distance >= _value / ConstantOfDistance)
                    break;
                tmp.adjNodes[i].weight = 10f;
                tmp.adjNodes[i].IsDanger = true;
                this.updated.Add(tmp.adjNodes[i]);
            }
            
            isPathUpdated = false;
        }

    }
    
    /* Legacy of UpdateWeight()
    {

        foreach (Node sensor in Sensors){
            int xC, yC;
            int dist = 2;
            for (int x = -dist; x <= dist; x++)
            {
                for (int y = -dist; y <= dist; y++)
                {
                    xC = sensor.gridX + x;
                    yC = sensor.gridY + y;

                    if (xC >= 0 && xC < gridSizeX && yC >= 0 && yC < gridSizeY)
                    {
                        grid[xC, yC].weight = sensor.weight - (Mathf.Abs(x) + Mathf.Abs(y));
                        if (grid[xC, yC].weight > 8)
                            grid[xC, yC].IsDanger = true;
                    }
                }
            }
        }
    }
    */

    public void InitCost()
    {
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                grid[x, y].InitCost();
            }
        }
    }
    public void InitWeight()
    {
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                grid[x, y].weight = 0;
            }
        }
    }
    public void InitDangerFlag()
    {
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                grid[x, y].IsDanger = false;
            }
        }
    }
    int UpdateGrid()
    {
        
        Vector3 bottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);

                if (Physics.CheckSphere(worldPoint, nodeRadius, WallMask))
                {
                    //Wall = true;
                }
                //grid[x, y] = new Node(Wall, worldPoint, x, y);
            }
        }
        return 0;
    }
    public Node NodeFromWorldPosition(Vector3 a_WorldPos) {
        float xp = ((a_WorldPos.x - this.transform.position.x + gridWorldSize.x / 2) / gridWorldSize.x);
        float yp = ((a_WorldPos.z - this.transform.position.z + gridWorldSize.y / 2) / gridWorldSize.y);

        xp = Mathf.Clamp01(xp);
        yp = Mathf.Clamp01(yp);
        int x = Mathf.RoundToInt((gridSizeX - 1) * xp);
        int y = Mathf.RoundToInt((gridSizeY - 1) * yp);
        return grid[x, y];
    }

    public Node LerpNode(Node a, Node b, float t)
    {
        return NodeFromWorldPosition(Vector3.Lerp(a.Position, b.Position, t));
    }

    public List<Node> GetNeighboringNodes(Node a_Node) {
        List<Node> NeighboringNodes = new List<Node>();

        int xC, yC;

        for(int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if(x == 0 && y == 0) {
                    continue;
                }

                xC = a_Node.gridX + x;
                yC = a_Node.gridY + y;

                if(xC >= 0 && xC < gridSizeX && yC >= 0 && yC < gridSizeY) {
                    NeighboringNodes.Add(grid[xC, yC]); 
                }

            }
        }
        return NeighboringNodes;
    }
    public void AddPath(Node[] nodes)
    {
        this.FinalPaths.Add(nodes);
    }

    public void ResetPaths()
    {
        this.FinalPaths.Clear();
        this.MinPaths.Clear();
    }
    // 장애물은 이미 계산되어있는 것으로 가정 (장애물 갱신마다 이미 업데이트 되는 것을 가정)
    public Node[] GetStoredPathFromPosition(Vector3 _worldPosition)
    {
        Node[] ret = null;
        Node StartNode = this.NodeFromWorldPosition(_worldPosition);
        float minDistance = Mathf.Infinity;
        float tmp;
        Node PathStartNode = null;
        Node TargetNode;

        // Find nearest exit node
        //// The nearest exit node's idx is 0.

        // Find nearest optimal 'node'
        for (int i = 0; i < nodes.Count; i++)
        {
            if (IsRayPath(StartNode, nodes[i]))
            {
                tmp = nodes[i].Parents[0].GetRegidualDistance() + nodes[i].DistanceTo(StartNode);
                if (minDistance > tmp)
                {
                    minDistance = tmp;
                    PathStartNode = nodes[i];
                }
            }  
        }
        
        // GetStoredPath at 'node'
        Node[] tmpp = PathStartNode.GetPathTo(PathStartNode.Parents[0].GetTargetNode());
        ret = new Node[tmpp.Length + 1];
        ret.SetValue(StartNode, 0);
        for (int i = 0; i < tmpp.Length; i++)
            ret.SetValue(tmpp[i], i + 1);
        
        return ret;
    }

    public Node[] GetStoredPathFromPosition(Vector3 _worldPosition, int exit)
    {
        Node[] ret = null;
        Node StartNode = this.NodeFromWorldPosition(_worldPosition);
        float minDistance = Mathf.Infinity;
        float tmp;
        Node PathStartNode = null;
        Node TargetNode;

        // Find nearest exit node
        //// The nearest exit node's idx is 0.

        // Find nearest optimal 'node'
        for (int i = 0; i < nodes.Count; i++)
        {
            if (IsRayPath(StartNode, nodes[i]))
            {
                tmp = nodes[i].Parents[exit].GetRegidualDistance() + nodes[i].DistanceTo(StartNode);
                if (minDistance > tmp)
                {
                    minDistance = tmp;
                    PathStartNode = nodes[i];
                }
            }
        }

        // GetStoredPath at 'node'
        Node[] tmpp = PathStartNode.GetPathTo(PathStartNode.Parents[exit].GetTargetNode());
        if (tmpp == null) return null;
        ret = new Node[tmpp.Length + 1];
        ret.SetValue(StartNode, 0);
        for (int i = 0; i < tmpp.Length; i++)
            ret.SetValue(tmpp[i], i + 1);

        return ret;
    }

    // Check obstacles using raycast?
    /*
    public Node[] GetStoredPath(Node _StartNode)
    {
        List<Node> Targets = new List<Node>(Exits.Length);
        int tmpI;
        for (int c = 0; c < Exits.Length; c++)
        {
            float tmpF = _StartNode.Parents[c].GetRegidualDistance();
            tmpI = c;
            for (int i = c + 1; i < Exits.Length; i++)
            {
                if (tmpF > _StartNode.Parents[Exits[i]].GetRegidualDistance())
                {
                    tmpF = _StartNode.Parents[Exits[i]].GetRegidualDistance();
                    tmpI = i;
                    Targets.Add(Exits[i]);
                }
            }
            
        }
        return Targets.ToArray();
    }*/
    public void ResetFinalPaths()
    {
        this.FinalPaths.Clear();
    }

    Vector3[] NodesToPos(Node[] nodes)
    {
        List<Vector3> ret = new List<Vector3>(nodes.Length);
        for (int i = 0; i < nodes.Length; i++)
        {
            ret.Add(new Vector3(nodes[i].Position.x, nodes[i].Position.y+1f, nodes[i].Position.z));
        }
        return ret.ToArray();
    }
    public void InitLiner()
    {
        for (int i = 0; i < renderer.Count; i++)
        {
            Destroy(renderer[i]);
        }
        renderer.Clear();
    }

    void SetLine(LineRenderer _line, Color c)
    {
        
        _line.endColor = new Color(c.r, c.g, c.b, 1);
        _line.startColor = c;
        _line.startWidth = 0.3f;
        _line.endWidth = .3f;
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
                        Color c = new Color(0, 1, 0, 1f);
                        SetLine(line, c);
                        line.positionCount = this.MinPaths[i].Length;
                        line.SetPositions(NodesToPos(this.MinPaths[i]));
                        line.enabled = true;
                        this.renderer.Add(lineTmp);
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
                        Color c = new Color(0, 1, 0, 1f);
                        SetLine(line, c);
                        line.positionCount = this.FinalPaths[i].Length;
                        line.SetPositions(NodesToPos(this.FinalPaths[i]));
                        line.enabled = true;
                        this.renderer.Add(lineTmp);
                    }

                }
            }
        
    }
    private void OnDrawGizmos() {        
        Gizmos.DrawWireCube(GridPostion.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        if (nodes != null)
        {
            for (int i = 0; i < grid.GetLength(0); i++)
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    tmpNode = grid[i, j];
                    //if (grid[i, j].IsWallAround)
                    //{
                    //    Gizmos.color = new Color(0, 1, 0, 1f);
                    //    Gizmos.DrawCube(grid[i, j].Position, Vector3.one * (nodeDiameter - distance));
                    //}
                    if (grid[i, j].IsCorner)
                    {
                        Gizmos.color = new Color(0, 0, 1, 1f);
                        Gizmos.DrawCube(grid[i, j].Position, Vector3.one * (nodeDiameter - distance));
                    }
                    if (grid[i, j].weight != 0)
                    {
                        Gizmos.color = new Color(grid[i, j].weight / 10, 1 - grid[i, j].FCost / 100f, 0, 1f);
                        Gizmos.DrawCube(grid[i, j].Position, Vector3.one * (nodeDiameter - distance));
                    }

                    if (grid[i, j].IsDanger)
                    {
                        //Gizmos.color = new Color(1, 0, 0, 1f);
                        Gizmos.DrawCube(grid[i, j].Position, Vector3.one * (nodeDiameter - distance));
                    }
                }
            
            if (this.Sensors != null)
            {
                Dictionary<int, Node>.KeyCollection keys = this.Sensors.Keys;
                foreach (int k in keys)
                {
                    int i = this.Sensors[k].gridX;
                    int j = this.Sensors[k].gridY;
                    //Gizmos.color = new Color(grid[i, j].weight / 10, 1 - grid[i, j].FCost / 100f, 0, 1f);
                    Gizmos.DrawCube(grid[i, j].Position, Vector3.one * (nodeDiameter - distance));
                }
            }
            // Ray
            for (int i = 0; i < nodes.Count; i++)
            {
                //if (nodes[i].Parents[0] != null)
                    //Handles.Label(nodes[i].Position, Mathf.Floor(nodes[i].Parents[0].GetRegidualDistance()).ToString());


            }

            
            if (FinalPaths != null)
                for (int p = 0; p < FinalPaths.Count; p++)
                    for (int i = 1; i < FinalPaths[p].Length; i++)
                    {
                        //    Gizmos.
                        {
                            //Gizmos.color = new Color(1, 0, 0, 0.5f);
                            //Gizmos.DrawCube(FinalPath[i].Position, Vector3.one * (nodeDiameter - distance));
                            Gizmos.DrawLine(FinalPaths[p][i - 1].Position, FinalPaths[p][i].Position);
                        }

                    }
            /*
            if (MinPaths != null)
                for (int p = 0; p < MinPaths.Count; p++)
                    for (int i = 1; i < MinPaths[p].Length; i++)
                    {
                        //    Gizmos.
                        {
                            Gizmos.color = new Color(0, 1, 0, 1f);
                            //Gizmos.DrawCube(FinalPath[i].Position, Vector3.one * (nodeDiameter - distance));
                            Gizmos.DrawLine(MinPaths[p][i - 1].Position, MinPaths[p][i].Position);
                        }

                    }
                    */
        }
    }
    public void AddNode(Node _node)
    {
        nodes.Add(_node);
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            if (IsRayPath(_node, nodes[i]))
            {
                // Link
                nodes[i].AddAdjNode(_node);
                _node.AddAdjNode(nodes[i]);
            }
                
        }

    }

    public void AddTargetNode(Node _node)
    {
        this.TargetNodes.Add(_node);
    }
    public Node[] GetTargetNodes()
    {
        return this.TargetNodes.ToArray();
    }
    private bool CheckCorner(Node _node)
    {
        int i, j;
        int pi, pj;
        pi = -1;
        pj = 0;
        if (!_node.IsWallAround) return false;
        Node pre = grid[_node.gridX - 1, _node.gridY];
        Node prepre = grid[_node.gridX - 1, _node.gridY + 1];
        for (i = -1; i < 2; i+=2)
        {
            for (j = -1; j < 2; j+=2)
            {
                if (!(grid[_node.gridX + i, _node.gridY].IsWallAround ^ grid[_node.gridX, _node.gridY + j].IsWallAround))
                {
                    if (!grid[_node.gridX + i, _node.gridY + j].IsWallAround) return true;
                } 
            }
        }
        return false;
    }

    public bool IsRayPath(Node _n1, Node _n2)
    {
        Ray ray = new Ray(_n1.Position, (_n2.Position - _n1.Position));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Vector3.Distance(_n1.Position, _n2.Position)))
        {//, (1 << LayerMask.NameToLayer("Obstacle")))) {
            return false;
        }
        return true;
    }
}

