using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Linq;
using System;

public class ScenarioManager : MonoBehaviour
{
    // Singleton obejct for static call
    public static ScenarioManager singleTon;

    MQTTManager mQTTManager;
    public Grid3 grid;

    // Current state.
    // true = at least one sensor is in disaster mode.
    // false = no sensor is in disaster mode.
    private bool isDisaster = false;

    //Self data
    NavMeshPath p;
    public SimulationManager3 simulationManager = null;
    int pathSize = 0;
    public bool isSensorUpdated;
    public bool isAreaChanged;
    public bool isSimulating = false;
    bool isDanger = false;


    Camera subCamera;
    public GameObject image_panel;
    GameObject content;
    Text time_text;
    Image image_ob1;
    AudioSource musicPlayer;

    List<screenshot_attr> paths = new List<screenshot_attr>();
    screenshot_attr temp_path;
    List<GameObject> sensorObjs = new List<GameObject>();

    // ???
    bool initEvacs = false;

    public void Start()
    {
        singleTon = this;
    }

    public void Init()////////////////////
    {
        //Camera initiation
        if (subCamera == null) subCamera = GameObject.Find("SubCamera").GetComponent<Camera>();
        Vector3 buildingSize = BuildingManager.BuildingBound.size;
        float cameraViewSize = Mathf.Max(buildingSize.x, buildingSize.z) / 2;
        subCamera.orthographicSize = cameraViewSize;

        Vector3 cameraPosition = BuildingManager.BuildingBound.center;
        cameraPosition.y = 100;
        subCamera.transform.position = cameraPosition;


        if (grid == null) grid = transform.GetComponent<Grid3>();
        NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
        grid.CreateGrid(tri.vertices, BuildingManager.FloorsCount);

        if (mQTTManager == null) mQTTManager = GetComponent<MQTTManager>();
        mQTTManager.Init();

        // Register callback listener
        mQTTManager.OnNodeUpdated = OnNodeUpdated;

        isDanger = false;
        musicPlayer = gameObject.GetComponent<AudioSource>();

        content = GameObject.Find("scr_shot_panel");
        if (image_panel == null)
            image_panel = GameObject.Find("path_window_region");
        if (image_ob1 == null)
        {
            Transform tmpOb = GameObject.Find("panel_imgshow").transform;
            content = tmpOb.parent.gameObject;
            time_text = tmpOb.GetChild(1).GetComponent<Text>();
            image_ob1 = tmpOb.GetChild(0).GetComponent<Image>();
        }

        simulationManager = ScriptableObject.CreateInstance<SimulationManager3>();
        simulationManager.SetGrid(grid);

        foreach (NodeExit exit in NodeManager.GetNodesByType<NodeExit>())
        {
            Debug.Log(exit.PhysicalID);
        }

        foreach (NodeArea area in NodeManager.GetNodesByType<NodeArea>())
        {
            Debug.Log(area.PhysicalID);
        }
    }

    public void SetDefault()
    {
        // ToDo : Init sensor value
        // Such as disaster mode setting of some sensors or area number of areas
        mQTTManager?.Close();
    }

    void OnNodeUpdated(MQTTManager.MQTTMsgData data)
    {
        Debug.Log(data);

        // Check disaster.
        // false = every node is not in disaster mode
        // true = at least one node is in disaster mode
        bool newDisasterState = false;

        // Check if area number has been changed.
        bool isAreaChanged = false;

        // Apply changes on node and check current disaster state.
        if (data.NodeType == typeof(NodeFireSensor))
        {
            NodeFireSensor node = (NodeFireSensor)NodeManager.GetNodeByID(data.PhysicalID);
            node.IsDisaster = data.IsDisaster;
            newDisasterState |= data.IsDisaster;
        }

        // Check if number of people in area changed.
        else if (data.NodeType == typeof(NodeArea))
        {
            NodeArea node = (NodeArea)NodeManager.GetNodeByID(data.PhysicalID);
            if (node.Num != data.Value)
            {
                node.Num = (int)data.Value;
                isAreaChanged = true;
            }
        }

        // If state changed
        if ((isDisaster != newDisasterState) | isAreaChanged)
        {
            isDisaster = newDisasterState;
            if (isDisaster)
            {
                // Disaster is started, or the number of area changed.
                FunctionManager.Find("window_screenshot").gameObject.SetActive(true);
                InitSimulation();
                if (IsInvoking("ReSearch"))
                    CancelInvoke("ReSearch");
                Invoke("ReSearch", 60);
                isSimulating = true;
            }
            else
            {
                // Disaster has been finished.
                grid.InitWeight();
                grid.ViewMinPath = false;
                grid.InitLiner();
                if (musicPlayer != null) musicPlayer.Stop();
                // Image panel is not implemented yet.
                //mQTTManager.PubPeriod(360);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isSimulating)
        {
            if (simulationManager.EvacuatersList.Count > 0)
            {
                if (simulationManager.Progress())
                {
                    // 모든 경로에 대해 시뮬레이션이 완료됨.
                    grid.InitWeight();
                    grid.ViewMinPath = true;
                    grid.Liner();
                    isSimulating = false;
                    simulationManager.PrintOut("");
                    initEvacs = false;
                    SetDirectionSensor();
                    ScreenShot(simulationManager.delayList[simulationManager.delayList.Count - 1]);
                }
                else
                {
                    ScreenShot(simulationManager.delayList[simulationManager.delayList.Count - 1]);
                }
            }
        }
    }

    void OnDanger()
    {
        if (!this.image_panel.activeSelf)
        {
            // Off -> On으로 전환되었을 때
            //mQTTManager.PubPeriod(10);
            this.image_panel.SetActive(true);
            // 60초 후 자동 재 검색
            if (IsInvoking("ReSearch"))
                CancelInvoke("ReSearch");
            Invoke("ReSearch", 60);
        }
        isSimulating = true;
    }

    void ReSearch()
    {
        Debug.Log("Research...");
        if (isDanger)
        {
            initEvacs = false;
        }
    }

    void Siren()
    {
        //MusicPlayer.loop = true;
        if (!musicPlayer.isPlaying)
            musicPlayer.Play();
    }

    void InitSimulation()
    {
        // Init simulation manager
        simulationManager = ScriptableObject.CreateInstance<SimulationManager3>();
        simulationManager.SetGrid(grid);
        grid.ResetFinalPaths();
        grid.ViewMinPath = false;
        simulationManager.EvacuatersList.Clear();

        // Store paths
        foreach (NodeArea area in NodeManager.GetNodesByType<NodeArea>())
        {
            Debug.Log(area.PhysicalID);

            // Pass empty area
            if (area.Num == 0) continue;

            //simulationManager.SetEvacuaters(this.areaJsons, this.areaNums);
            List<Node3[]> paths = new List<Node3[]>();

            // for targets
            foreach (NodeExit exit in NodeManager.GetNodesByType<NodeExit>())
            {
                Debug.Log(area.PhysicalID + "/" + exit.PhysicalID);

                // Calculate path
                List<Node3> path = new List<Node3>();
                p = new NavMeshPath();

                // Calculate path from every area to every exit
                NavMesh.CalculatePath(area.Position, exit.Position, -1, p);
                for (int o = 0; o < p.corners.Length - 1; o++)
                {
                    path.AddRange(grid.GetNodesFromLine(p.corners[o], p.corners[o + 1]));
                }

                // Remove duplicated location
                for (int o = 0; o < path.Count - 1; o++)
                {
                    if (path[o] == path[o + 1])
                    {
                        while (path[o] == path[o + 1])
                            path.Remove(path[o + 1]);
                    }
                }

                // Add to paths
                if (path.Count > 0) paths.Add(path.ToArray());
            }

            this.pathSize = paths.Count;
            simulationManager.AddEvacuater(area, paths);
        }
        Debug.Log(this.pathSize);
        simulationManager.InitSimParam(this.pathSize);
        paths.Clear();
        initEvacs = true;
    }

    IEnumerator screen_pixels()
    {
        subCamera.enabled = true;
        temp_path.scrshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);

        yield return new WaitForEndOfFrame();
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);//depth : 일단 24
        subCamera.targetTexture = rt;
        //texture1
        subCamera.Render();
        RenderTexture.active = rt;
        temp_path.scrshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        temp_path.scrshot.Apply();
    }

    void ScreenShot(float tmpTime)
    {
        temp_path = new screenshot_attr();
        temp_path.time = tmpTime;
        StartCoroutine(screen_pixels());
        subCamera.enabled = false;
        paths.Add(temp_path);

        if (pathSize == paths.Count)
        {
            // for (int i = 0; i < CreatedForGUI.Count; i++)
            // Destroy(CreatedForGUI[i]);
            // CreatedForGUI.Clear();

            paths.Sort(delegate (screenshot_attr x, screenshot_attr y)
            {
                if (x.time > y.time) return 1;
                else if (x.time < y.time) return -1;
                return 0;
            });
            int maxList = 10;
            if (maxList > paths.Count) maxList = paths.Count;
            for (int r = 0; r < maxList; r++)
            {
                Image new_image_ob;
                Text new_text;
                if (r == 0)
                {
                    new_image_ob = image_ob1;
                    new_text = time_text;

                }
                else
                {
                    new_image_ob = Instantiate(image_ob1, image_ob1.transform.position, Quaternion.identity);
                    new_text = Instantiate(time_text, image_ob1.transform.position, Quaternion.identity);
                    // CreatedForGUI.Add(new_image_ob.gameObject);
                    // CreatedForGUI.Add(new_text.gameObject);
                }

                new_image_ob.transform.SetParent(content.transform);
                new_image_ob.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, image_ob1.rectTransform.rect.width);
                new_image_ob.transform.localPosition = new Vector3(image_ob1.transform.localPosition.x, -(35) * r, image_ob1.transform.localPosition.z);
                new_image_ob.transform.rotation = image_ob1.transform.rotation;
                new_image_ob.transform.localScale = image_ob1.transform.localScale;
                new_image_ob.GetComponent<Image>().sprite = Sprite.Create(paths[r].scrshot, new Rect(0, 0, Screen.width, Screen.height), new Vector2(0.5f, 0.5f));

                new_text.transform.SetParent(content.transform);
                new_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, time_text.rectTransform.rect.width);
                new_text.transform.localPosition = new Vector3(time_text.transform.localPosition.x, image_ob1.rectTransform.rect.height - (35) * r, time_text.transform.localPosition.z);
                new_text.transform.rotation = time_text.transform.rotation;
                new_text.transform.localScale = time_text.transform.localScale;

                new_text.GetComponent<Text>().text = "Time : " + paths[r].time.ToString();
            }
        }
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

    void SetDirectionSensor()//최적경로에 따른 대피유도신호로 바꾸기()
    {
        // 타겟에 따른 최적 경로, 
        // 모든 방향 지시등 센서마다.
        for (int i = 0; i < sensorObjs.Count; i++)
        {
            if (sensorObjs[i].GetComponent<sensor_attribute>().one_sensor.nodeType == 39) // 방향 지시등 일 때
            {
                DirectionSensorScript dir = sensorObjs[i].GetComponent<DirectionSensorScript>();
                dir.Init();
                Vector3 target = GetDirectionTarget(sensorObjs[i].transform.position);
                NavMeshPath p = new NavMeshPath();
                NavMesh.CalculatePath(sensorObjs[i].transform.position, target, -1, p);
                if (p.status == NavMeshPathStatus.PathComplete)
                {
                    mQTTManager.PubDirectionOperation(sensorObjs[i].GetComponent<sensor_attribute>().one_sensor.nodeId, VectorToDirection(
                        p.corners[1] - p.corners[0]));
                }
            }
        }
    }

    Vector3 GetDirectionTarget(Vector3 origin)
    {
        float mD = 100000f;
        Vector3 target = Vector3.zero;
        for (int j = 0; j < grid.MinPaths.Count; j++)
        {
            float tmp = Vector3.Distance(origin, grid.MinPaths[j][0].position);
            if (tmp < mD)
            {
                mD = tmp;
                target = grid.MinPaths[j].Last().position;
            }
        }
        return target;
    }
    string VectorToDirection(Vector3 dir)
    {
        // 동서남북 중 가장 가까운
        // In? Out?
        string ret = "left";
        Vector3 tmp = dir.normalized;
        if (Mathf.Abs(tmp.x) > Mathf.Abs(tmp.z))
        {
            if (tmp.x > 0) ret = "right";
            else ret = "left";
        }
        else
        {
            if (tmp.z > 0) ret = "up";
            else ret = "down";
        }
        Debug.Log(tmp.ToString() + ret);
        return ret;
    }
}
