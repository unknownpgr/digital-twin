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
    private bool currentDisasterState = false;

    // Check if disaster occurred during simulation.
    private bool disasterOccurred = false;

    //Self data
    NavMeshPath p;
    public SimulationManager3 simulationManager = null;
    int pathSize = 0;
    public bool isSensorUpdated;
    public bool isAreaChanged;
    public bool isSimulating = false;

    Camera subCamera;

    // (New) Elements of path window UI
    GameObject defaultPathPanel;
    Transform content;

    // (New) Element of disaster warning UI
    GameObject warningBox;
    Image warningIcon;
    Text disatsterName;

    // (New) Game object of end simulation button
    GameObject endSimulBtn;

    List<ScreenshotAttr> pathImages = new List<ScreenshotAttr>();

    // ???
    bool initEvacs = false;

    public void Start()
    {
        singleTon = this;
    }

    public void Init()
    {
        //Camera initiation
        if (subCamera == null) subCamera = GameObject.Find("SubCamera").GetComponent<Camera>();
        Vector3 buildingSize = BuildingManager.BuildingBound.size;
        float cameraViewSize = Mathf.Max(buildingSize.x, buildingSize.z) / 2;
        subCamera.orthographicSize = cameraViewSize;

        Vector3 cameraPosition = BuildingManager.BuildingBound.center;
        cameraPosition.y = 100;
        subCamera.transform.position = cameraPosition;

        // << BLOCKING TASK 1 >>
        if (grid == null) grid = transform.GetComponent<Grid3>();
        NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
        grid.CreateGrid(tri.vertices, BuildingManager.FloorsCount);

        if (mQTTManager == null) mQTTManager = GetComponent<MQTTManager>();
        mQTTManager.Init();

        // Register callback listener
        MQTTManager.OnNodeUpdated = OnNodeUpdated;

        // (New) Get exsisting path panel and transform of parent
        defaultPathPanel = GameObject.Find("panel_path");
        content = GameObject.Find("window_path_content").transform;

        // (New) Get text of disaster warning UI
        warningBox = FunctionManager.Find("warning_box").gameObject;
        warningIcon = warningBox.transform.GetChild(0).GetComponent<Image>();
        disatsterName = warningBox.transform.GetChild(1).GetComponent<Text>();

        // (New) Get object of end simulation button
        endSimulBtn = FunctionManager.Find("button_end_simulation").gameObject;

        // << BLOCKING TASK 3 >>
        simulationManager = ScriptableObject.CreateInstance<SimulationManager3>();
        simulationManager.SetGrid(grid);

        // Initialize disaster state
        disasterOccurred = false;
    }

    public void SetDefault()
    {
        // Reset path
        grid.ResetFinalPaths();
        grid.InitWeight();
        grid.ViewMinPath = false;
        grid.InitLiner();

        // Clear simulationManager
        simulationManager.EvacuatersList.Clear();

        // Reset area number
        foreach (NodeArea nodeArea in NodeManager.GetNodesByType<NodeArea>()) nodeArea.Num = 0;
        foreach (NodeFireSensor fireSensor in NodeManager.GetNodesByType<NodeFireSensor>()) fireSensor.IsDisaster = false;

        // If a disaster has occurred, reset the sensor. else, skip this process.
        if (disasterOccurred)
        {
            // Initialize siren
            SetSiren(false);

            // Initialize direction sensor
            foreach (NodeDirection node in NodeManager.GetNodesByType<NodeDirection>()) mQTTManager.PubDirectionOperation(node.PhysicalID, "off");
        }

        // Close mqttManager
        mQTTManager?.Close();
    }

    void OnNodeUpdated(MQTTManager.MQTTMsgData data)
    {
        // false = every node is not in disaster mode
        // true = at least one node is in disaster mode
        bool newDisasterState = false;  // Check disaster.
        bool isAreaChanged = false;     // Check if area number has been changed.

        // Ignore wrong MQTT data
        if (NodeManager.GetNodeByID(data.PhysicalID) == null)
        {
            Debug.Log("Unknown node : " + data.PhysicalID);
            return;
        }

        NodeManager node = NodeManager.GetNodeByID(data.PhysicalID);

        // Apply changes on node and check current disaster state.
        if (node is NodeFireSensor)
        {
            NodeFireSensor nodeFire = (NodeFireSensor)node;
            nodeFire.IsDisaster = data.IsDisaster;

            switch (data.sensorType)
            {
                case Constants.NODE_SENSOR_TEMP:
                    nodeFire.ValueTemp = data.Value;
                    break;

                case Constants.NODE_SENSOR_FIRE:
                    nodeFire.ValueFire = data.Value;
                    break;

                case Constants.NODE_SENSOR_SMOKE:
                    nodeFire.ValueSmoke = data.Value;
                    break;
            }

            Debug.Log(nodeFire.ValueTemp);

            newDisasterState |= data.IsDisaster;
            if (newDisasterState) disatsterName.text = "화재 발생";
        }

        // Check if number of people in area changed.
        else if (node is NodeArea)
        {
            NodeArea nodeArea = (NodeArea)node;
            if (nodeArea.Num != data.Value)
            {
                nodeArea.Num = (int)data.Value;
                isAreaChanged = true;
            }
        }

        // Set direction sensor
        else if (node is NodeDirection)
        {
            NodeDirection nodeDirection = (NodeDirection)node;
            nodeDirection.Direction = data.Direction;

            // Do not change disaster state.
            newDisasterState = currentDisasterState;
        }

        // If state changed
        if ((currentDisasterState != newDisasterState) | isAreaChanged)
        {
            currentDisasterState = newDisasterState;
            if (currentDisasterState)
            {
                // Disaster is started, or the number of area changed.
                SetSiren(true);
                WindowManager.GetWindow("window_path").SetVisible(true);
                warningBox.SetActive(true);
                endSimulBtn.SetActive(true);

                StartCoroutine(SetTextOpacity());
                StartCoroutine(InitSimulation());
            }
            else
            {
                // Disaster has been finished.
                // ToDo : Consider to put 'SetDefault()' here.
                grid.InitWeight();
                grid.ViewMinPath = false;
                grid.InitLiner();
                SetSiren(false);
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
                Debug.Log("SIM...");
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
                }
                StartCoroutine(ScreenShot(simulationManager.delayList[simulationManager.delayList.Count - 1]));
            }
        }
    }

    private AudioSource sirenPlayer;
    public void SetSiren(bool play)
    {
        // If there are no 
        if (sirenPlayer == null) sirenPlayer = gameObject.GetComponent<AudioSource>();

        if ((!sirenPlayer.isPlaying) && play)
        {
            mQTTManager.PubSiren(true);
            sirenPlayer.Play();
        }
        else if (sirenPlayer.isPlaying && (!play))
        {
            mQTTManager.PubSiren(false);
            sirenPlayer.Stop();
        }
    }

    IEnumerator InitSimulation()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Init simulation manager
        simulationManager = ScriptableObject.CreateInstance<SimulationManager3>();
        simulationManager.SetGrid(grid);
        simulationManager.EvacuatersList.Clear();
        grid.ResetFinalPaths();
        grid.ViewMinPath = false;

        // Store paths
        foreach (NodeArea area in NodeManager.GetNodesByType<NodeArea>())
        {
            // Pass empty area
            if (area.Num == 0) continue;

            //simulationManager.SetEvacuaters(this.areaJsons, this.areaNums);
            List<Node3[]> paths = new List<Node3[]>();

            // for targets
            foreach (NodeExit exit in NodeManager.GetNodesByType<NodeExit>())
            {
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

            pathSize += paths.Count;
            simulationManager.AddEvacuater(area.Position, area.Num, paths, area.Velocity);
        }

        // Now, path calculating finished.
        simulationManager.InitSimParam(pathSize);
        pathImages.Clear();
        initEvacs = true;

        // Start simulating
        isSimulating = true;
    }

    IEnumerator ScreenShot(float time)
    {
        ScreenshotAttr screenShotData = new ScreenshotAttr
        {
            time = time
        };

        // Take picture
        subCamera.enabled = true;
        yield return new WaitForEndOfFrame();
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);//depth : 일단 24
        subCamera.targetTexture = rt;
        subCamera.Render();
        subCamera.enabled = false;
        RenderTexture.active = rt;

        Texture2D screenShotTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
        screenShotTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        screenShotTexture.Apply();

        screenShotData.scrshot = screenShotTexture;
        pathImages.Add(screenShotData);

        // If image is full
        if (pathSize == pathImages.Count)
        {
            pathImages.Sort(delegate (ScreenshotAttr x, ScreenshotAttr y)
            {
                if (x.time > y.time) return 1;
                else if (x.time < y.time) return -1;
                return 0;
            });

            // Show top-10 fastest pathes on panel
            int listCount = Math.Min(pathImages.Count, 10);
            for (int r = 0; r < listCount; r++)
            {
                // Create new image
                GameObject newPathPanel = Instantiate(defaultPathPanel);
                Transform newPanelTransform = newPathPanel.transform;
                newPanelTransform.SetParent(content, false);
                newPanelTransform.localPosition = Vector3.zero;

                // Set image
                Image evacpathImage = newPanelTransform.GetChild(0).GetComponent<Image>();
                evacpathImage.sprite = Sprite.Create(pathImages[r].scrshot, new Rect(0, 0, Screen.width - 1, Screen.height - 1), new Vector2(0.5f, 0.5f));

                // Set rank text
                Text evacRankText = newPanelTransform.GetChild(1).GetComponentInChildren<Text>();
                evacRankText.text = (r + 1).ToString();

                // Set time
                Text evacTimeText = newPanelTransform.GetChild(2).GetComponentInChildren<Text>();
                evacTimeText.text = "예상 시간 : " + string.Format("{0:F2}", pathImages[r].time) + "(초)";
                // evacTimeText.text = "Time : " + pathImages[r].time.ToString() + "(초)";
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

    void SetDirectionSensor() //최적경로에 따른 대피유도신호로 바꾸기
    {
        foreach (NodeDirection node in NodeManager.GetNodesByType<NodeDirection>())
        {
            // target = nearest path position
            Vector3 target = GetNearestPathPoint(node.Position);

            // Calculate direction
            NavMeshPath p = new NavMeshPath();
            NavMesh.CalculatePath(node.Position, target, -1, p);
            if (p.status == NavMeshPathStatus.PathComplete)
            {
                mQTTManager.PubDirectionOperation(node.PhysicalID, VectorToDirection(p.corners[1] - p.corners[0]));
            }
        }
    }

    // 가장 가까운 경로 위치를 가져옴.
    private Vector3 GetNearestPathPoint(Vector3 origin)
    {
        float minDistance = float.MaxValue;
        Vector3 target = Vector3.zero;
        foreach (Node3[] node in grid.MinPaths)
        {
            float tmp = Vector3.Distance(origin, node[0].position);
            if (tmp < minDistance)
            {
                minDistance = tmp;
                target = node.Last().position;
            }
        }
        return target;
    }

    // return = up(z), right(x), down(-z), left(-x)
    private string VectorToDirection(Vector3 dir)
    {
        string ret = "up";
        Vector3 tmp = dir.normalized;
        if (tmp.z < tmp.x)
        {
            if (tmp.z > -tmp.x) ret = "right";
            else ret = "down";
        }
        else
        {
            if (tmp.z > -tmp.x) ret = "up";
            else ret = "left";
        }
        return ret;
    }

    public IEnumerator SetTextOpacity()
    {
        warningIcon.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        disatsterName.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        yield return new WaitForSeconds(0.8f);
        while (warningBox.activeSelf == true)       // isDisater == true
        {
            warningIcon.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            disatsterName.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            yield return new WaitForSeconds(0.6f);

            warningIcon.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            disatsterName.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            yield return new WaitForSeconds(0.6f);
        }
    }
}
