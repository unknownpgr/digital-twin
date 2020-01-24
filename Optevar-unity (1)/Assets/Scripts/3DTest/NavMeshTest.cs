using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshTest : MonoBehaviour
{
    MeshFilter mf;
    NavMeshObstacle obs;
    List<GameObject> renderer = new List<GameObject>();
    public Grid3 grid;
    public GameObject lineClass;
    public GameObject start;
    public GameObject end;
    public GameObject building;

    public int[] dangerFloor;
    
    int floor;
    Dictionary<int, Vector3[]> targets;

    List<Node3> pathNodes;
    Vector3 pre;
    // Start is called before the first frame update
    void Start()
    {
        
        NavMeshTriangulation tri = NavMesh.CalculateTriangulation();

        InitBuildingInfo();
        grid.CreateGrid(tri.vertices, floor);
        pathNodes = new List<Node3>();
        pre = start.transform.position;
        GetTargetFromVectors(out targets);
        if (dangerFloor == null)
        {
            dangerFloor = new int[1];
            dangerFloor[0] = 3;
        }
        
    }

    void InitBuildingInfo()
    {
        floor = building.transform.childCount;
        
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
    // Update is called once per frame
    void Update()
    {
        //obs.carving = true;
        if (start.transform.position == pre) return;


        // INPUT: Vector3 startPosition, int[] dangerFloor, int floors, Grid3 grid
        // OUTPUT: startPosition에서 탈출하는 대피로의 배열. 목표 층의 탈출구 개수만큼 반환함.
        List<Node3[]> paths = new List<Node3[]>();
        // 1. 탈출할 층 찾기
        int targetFloor = GetTargetFloor(grid.GetFloorFromPosition(start.transform.position),
            this.dangerFloor, this.floor);
        if (targetFloor == -1) return;
        // 2. 탈출 층의 탈출구 Vector3[], 스타트 Vector3를 이용한 길을 찾고 추가하기
        grid.InitWeight();
        InitLiner();
        for (int t = 0; t < this.targets[targetFloor].Length; t++)
        {
            NavMeshPath p = new NavMeshPath();
            NavMesh.CalculatePath(start.transform.position, targets[targetFloor][t], -1, p);
            if (p.status == NavMeshPathStatus.PathComplete)
            {
                GameObject lineTmp = GameObject.Instantiate(lineClass);
                LineRenderer line = lineTmp.GetComponent<LineRenderer>();
                Color c = new Color(0, 1, 0, 1f);
                SetLine(line, c);
                line.positionCount = p.corners.Length;
                line.SetPositions((p.corners));
                line.enabled = true;
                this.renderer.Add(lineTmp);
                List<Node3> ret = new List<Node3>();
                for (int i = 0; i < p.corners.Length - 1; i++)
                {
                    ret.AddRange(grid.GetNodesFromLine(p.corners[i], p.corners[i + 1]));

                }
                paths.Add(ret.ToArray());
                

            }
        }
        for (int p = 0; p < paths.Count; p++)
        {
            for (int j = 0; j < paths[p].Length; j++)
            {
                if (paths[p][j] != null)
                    paths[p][j].weight = p + 1;
            }
            Debug.Log("Time: " + grid.CalcTime(paths[p], 1, 0.3f, 0.4f));
        }

        pre = start.transform.position;
    }
    int GetTargetFloor(int startFloor, int[] dangerFloor, int floor)
    {
        if (startFloor == -1) return -1;
        bool toDown, toUp;
        toDown = toUp = true;
        for (int i = 0; i < dangerFloor.Length; i++)
        {
            if (startFloor > dangerFloor[i])
                toDown &= false;
            if (startFloor < dangerFloor[i])
                toUp &= false;
        }
        if (toDown) return 1;
        if (toUp) return floor;
        return -1;
    }

    void GetTargetFromVectors(out Dictionary<int, Vector3[]> targets)
    {
        targets = new Dictionary<int, Vector3[]>();
        for (int i = 0; i < end.transform.childCount; i++)
        {
            List<Vector3> tmp = new List<Vector3>();
            for (int j = 0; j < end.transform.GetChild(i).childCount; j++)
            {
                tmp.Add(
                    end.transform.GetChild(i).GetChild(j).transform.position
                    );
            }
            targets.Add(i+1, tmp.ToArray());
        }
    }
    void UpdatePaths()
    {
        
    }

    
}
