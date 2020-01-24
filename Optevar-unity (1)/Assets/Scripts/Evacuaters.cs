using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;





public class Evacuaters
{
    int number;//
    public int CurNum;
    int pathIdx;
    CustomGrid grid;
    Node[][] path;
    Vector3 startPosition;//
    List<Evacuater> evacList;
    public float DistancesOfPath;
    public Evacuaters(int n)
    {
        number = n;
        CurNum = n;
        evacList = new List<Evacuater>(n);
        for (int i = 0; i < n; i++)
            evacList.Add(new Evacuater());
    }

    public float GetDistance()
    {
        float ret = 0f;
        for (int i = 0; i < path[pathIdx].Length - 1; i++)
        {
            ret += Vector3.Distance(path[pathIdx][i].Position, path[pathIdx][i + 1].Position);
        }
        return ret;
    }
    public void SetPaths(Node[][] p)
    {
        path = p;
        pathIdx = 0;
        for (int i = 0; i < number; i++)
        {
            evacList[i].SetParams(this.startPosition, grid, new Vector3());
            evacList[i].InitPath(path[pathIdx]);
        }
        CurNum = number;
    }
    public void SetPath(int idx)
    {
        pathIdx = idx;
        for (int i = 0; i < number; i++)
        {
            evacList[i].SetParams(this.startPosition, grid, new Vector3());
            evacList[i].InitPath(path[pathIdx]);
        }
        CurNum = number;

    }
    public void SetPath()
    {
        for (int i = 0; i < number; i++)
        {
            evacList[i].SetParams(this.startPosition, grid, new Vector3());
            evacList[i].InitPath(path[pathIdx]);
        }
        CurNum = number;

    }
    void InitEvacs()
    {
        for (int i = 0; i < evacList.Count; i++)
        {
            evacList[i].SetParams(this.startPosition, grid, new Vector3());
        }
    }
    public bool NextPath()
    {
        pathIdx++;
        if (pathIdx == path.Length)
        {
            pathIdx = 0;
            return true; // carry
        } else
        {
            //SetPath(pathIdx);
            return false; // not carry
        }
    }
    public Node[] GetPath(int idx)
    {
        return this.path[idx];
    }
    public Node[] GetPath()
    {
        return this.path[pathIdx];
    }
    public Vector3 GetPosition()
    {
        return startPosition;
    }
    public int Update(float deltaTime)
    {
        bool tmp = true;
        int nums = 0;
        for (int i = 0; i < evacList.Count; i++)
        {
            nums += evacList[i].UpdatePosition(grid, deltaTime);
        }
        if (this.CurNum > nums)
            if (nums >= 0)
                this.CurNum = nums;
        return nums;
        if (nums > 0) return -1;
        else return 1;
    }

    public void SetParams(Vector3 startPos, CustomGrid _grid, Vector3 _direction)
    {
        grid = _grid;
        startPosition = startPos;
        InitEvacs();
    }
    public void SetVelocity(float velo)
    {
        for (int i = 0; i < evacList.Count; i++)
        {
            evacList[i].SetVelocity(velo);
        }
    }

}
public class Evacuater
{
    float weight;
    float length;
    float velocity;
    Node[] path;
    List<Node> line;
    Vector3 direction;
    Node FrontNode;
    Node BackNode;
    //GameObject obj;
    //new Transform transform;
    //NodeNode parameters
    //Node StartNode, EndNode;
    int pathIdx; // path node index.
    float distanceOnPath;
    float currentDistance;
    bool IsMoving;

    //Type of evauaters

    private void Awake()
    {
        //transform = this.GetComponent<Transform>();
    }
    public void SetParams(Vector3 startPos, CustomGrid _grid, Vector3 _direction)
    {
        
        FrontNode = _grid.NodeFromWorldPosition(startPos);
        BackNode = FrontNode;
        direction = _direction;

        // Can GameObject implementate collision?
        //obj = GameObject.Instantiate(GameObject.Find("Weight"));
        
        //transform.position = FrontNode.Position;
        FrontNode.weight++;
        //transform.forward = direction;
        IsMoving = true;
        //objTransform.localScale.z = length;

        // NodeNode
        

        line = new List<Node>();
    }

    public Vector3 GetPosition()
    {
        return FrontNode.Position;
    }
    public void SetVelocity(float velo)
    {
        velocity = velo;
    }
    public void InitPath(Node[] path)
    {
        this.path = path;
        pathIdx = 0;
        currentDistance = 0f;
        if (path != null)
        {
            distanceOnPath = Vector3.Distance(path[pathIdx].Position, path[pathIdx + 1].Position);
            direction = (path[pathIdx + 1].Position - path[pathIdx].Position) / distanceOnPath;
        }

        //transform.forward = direction;

    }


    /*
    public void SetLines()
    {
        if (direction != null)
        {
            line.Clear();
            Node tmp = FrontNode;
            line.Add(tmp);
            while (BackNode != tmp)
            {
                tmp = grid.NodeFromWorldPosition(tmp.Position + 2 * grid.nodeRadius * direction);
                line.Add(tmp);
            }
        }
    }
    */
    // time -> distance -> t
    // distance of present path range
    float GetNextT(float dt)
    {
        //currentDistance += dt * velocity;
        return (currentDistance + dt * velocity) / distanceOnPath;
    }
    public int UpdatePosition(CustomGrid grid, float dt)
    {
        if (IsMoving)
        {
            if (path == null) return 1;
            float t = GetNextT(dt);
            if (t < 1)
            {
                
                Node tmp = grid.LerpNode(path[pathIdx], path[pathIdx + 1], t);
                if (tmp == FrontNode) currentDistance += dt * velocity;

                else if (tmp.weight==0)
                {
                    FrontNode.weight--;
                    FrontNode = tmp;
                    //transform.position = FrontNode.Position;
                    FrontNode.weight++;
                    currentDistance += dt * velocity;
                }

            }
            else
            {
                if (pathIdx == path.Length - 2)
                {
                    FrontNode.weight--;
                    IsMoving = false;
                    return -1;
                }
                pathIdx++;
                currentDistance = 0f;
                distanceOnPath = Vector3.Distance(path[pathIdx].Position, path[pathIdx + 1].Position);
                direction = (path[pathIdx + 1].Position - path[pathIdx].Position) / distanceOnPath;
                //transform.forward = direction;


            }
            return 1;
        }
        
        return 0;
        /*
        // 1. Init pre weights
        if (line.Count > 0)
        {
            for (int i = 0; i < line.Count; i++)
            {
                line[i].weight -= this.weight;
            }
        }

        // 2. Set next position and line
        direction = _direction;
        preFrontNode = FrontNode;
        //FrontNode = grid.NodeFromWorldPosition(FrontNode.Position + direction * (dt * this.velocity));
        if (FrontNode.DistanceTo(BackNode) > length)
        {
            preBackNode = BackNode;
            //BackNode = 
        }
        //// OBJ 
        objTransform.position = FrontNode.Position;
        objTransform.forward = direction;
        
        // 3. Update weights along line

        // 4.
        */
    }

        
    List<Node> GetNodesAround(Node node)
    {
        List<Node> ret = new List<Node>();

        ret.Add(node);
        return ret;
    }
}

