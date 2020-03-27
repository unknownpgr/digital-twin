using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Linq;
using System;

public class Sensors
{
    public GameObject gameObject { get; set; }
    public NavMeshObstacle navMeshObstacle { get; set; }
    public sensor_attribute sensor_Attribute { get; set; }
    public Material Material { get; set; }
    public Material Effect { get; set; }

    bool preDanger = false;
    float trans = .3f;
    float noTrans = 0f;
    public void Init()
    {
        navMeshObstacle.carving = false;
        Material.color = Color.white;
        Effect.color = new Color(Effect.color.r, Effect.color.g, Effect.color.b, noTrans);
    }

    // 왠진 모르겠으나 상태가 변했는지를 반환한다.
    bool SetSensor()
    {
        bool _thres = sensor_Attribute.one_sensor.disaster;

        if (_thres) Effect.color = new Color(Effect.color.r, Effect.color.g, Effect.color.b, trans);
        else Effect.color = new Color(Effect.color.r, Effect.color.g, Effect.color.b, noTrans);

        navMeshObstacle.carving = _thres;
        bool isTrans = preDanger ^ _thres;
        preDanger = _thres;
        return isTrans;
    }

    // 이것도 danger에 변화가 있는지를 반환한다. 왜....?
    public bool SensorValue()
    {
        bool ret = true;

        switch (sensor_Attribute.one_sensor.nodeType)
        {
            case 33://16진수 21
                //fire
                Material.color = new Color(1, 1 - sensor_Attribute.one_sensor.value1 * 3 / 255f, 1 - sensor_Attribute.one_sensor.value1 * 3 / 255f);
                //SetSensor(sensor_Attribute.one_sensor.value1 > 70);
                ret = SetSensor();
                break;
            case 2:
                //water
                Material.color = new Color(1 - sensor_Attribute.one_sensor.value1 * 3 / 255f, 1 - sensor_Attribute.one_sensor.value1 * 3 / 255f, 1);
                //SetSensor(sensor_Attribute.one_sensor.value1 > 0);
                ret = SetSensor();
                break;
            case 3:
                // earthquake
                Material.color = new Color(1 - sensor_Attribute.one_sensor.value1 * 3 / 255f, 1, 1 - sensor_Attribute.one_sensor.value1 * 3 / 255f);
                //SetSensor(sensor_Attribute.one_sensor.value1 > 0);
                ret = SetSensor();
                break;
            case 39://기존 39
                // 방향 지시
                // 이때 value1는 방향을 나타낸다. 0 ~ 3: 나가는 방향 지시, 4 ~ 7: 들어오는 방향 지시.
                DirectionSensorScript dir = gameObject.GetComponent<DirectionSensorScript>();
                if (sensor_Attribute.one_sensor.value1 < 4)
                    dir.OnOff((int)sensor_Attribute.one_sensor.value1, false, true);
                else if (sensor_Attribute.one_sensor.value1 >= 4)
                    dir.OnOff((int)sensor_Attribute.one_sensor.value1 - 4, true, true);
                break;
        }
        return ret;
    }

}
public class ScenarioManager3 : MonoBehaviour
{
    // Singleton obejct for static call
    public static ScenarioManager3 singleTon;

    MQTTManager3 mQTTManager;
    public Grid3 grid;


    //MQTT, Json Interface elements
    public List<SensorNodeJson> sensorNodeJsons;

    //Self data
    NavMeshPath p;
    public SimulationManager3 simulationManager = null;
    int floor;
    int pathSize = 0;
    int LastUpdatedFloor = 0;
    public bool isSensorUpdated;
    public bool isAreaChanged;
    public bool isSimulating = false;
    bool isDanger = false;

    /********for GUI********/
    float time;

    Camera sub_camera;
    Camera main_camera;
    private Texture2D scrshot_tecture;
    public GameObject image_panel;
    GameObject content;
    Text time_text;
    Image image_ob1;
    SideGUI SideGUI;
    Text WarnText;
    AudioSource musicPlayer;


    List<screenshot_attr> paths = new List<screenshot_attr>();
    screenshot_attr temp_path;

    public Dictionary<string, Sensors> SensorDictionary;

    List<GameObject> sensorObjs = new List<GameObject>();

    public void Start()
    {
        singleTon = this;
    }

    // Scenario ScenarioFromJson;
    public void Init()////////////////////
    {
        //Camera initiation
        main_camera = Camera.main;
        if (sub_camera == null)
            sub_camera = main_camera.transform.GetChild(0).GetComponent<Camera>();

        if (grid == null) grid = transform.GetComponent<Grid3>();
        NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
        grid.CreateGrid(tri.vertices, BuildingManager.FloorsCount);

        if (mQTTManager == null) mQTTManager = GetComponent<MQTTManager3>();
        mQTTManager.Init();

        // Register callback listener
        mQTTManager.OnSensorUpdated = OnSensorUpdated;
        Debug.Log("MQTTManager is loaded.");

        isDanger = false;
        musicPlayer = gameObject.GetComponent<AudioSource>();

        SetSensorNodes();
        SetSensorValue();

        simulationManager = ScriptableObject.CreateInstance<SimulationManager3>();
        simulationManager.SetGrid(grid);
    }

    public void SetDefault()
    {
        // ToDo : Init sensor value
        // Such as disaster mode setting of some sensors or area number of areas

        mQTTManager?.Close();
    }

    void OnSensorUpdated(MQTTManager3.MQTTMsgData data)
    {
        Debug.Log(data);

        if (isAreaChanged)
        {
            foreach (NodeArea area in NodeManager.GetNodesByType<NodeArea>()) ;
        }
        if (isSensorUpdated)
        {
            // SetSensorValue가 뭔가 중요해보이는데.
            if (!SetSensorValue())
            {
                // 위험 지역이 변화했을 때만 initEvacs == true;
                simulationManager.initEvacs = false;
                simulationManager.isSimEnd = false;
                return;
            }
        }

        if (isDanger)
            OnDanger();
        else
            OffDanger();

        // if문 아래는 시뮬레이션 과정으로, 한 번 반복할 때 하나의 경로에 대한 시뮬레이션을 진행함.
        // 추후 쓰레드를 활용한 모듈로 분리하여 최적화할 수 있음.

        if (isSimulating && !simulationManager.isSimEnd)
        {
            Debug.Log(simulationManager.isSimEnd);
            Siren();
            if (!simulationManager.initEvacs)
            {
                InitSimulation();
            }
            if (simulationManager.EvacuatersList.Count > 0)
            {
                if (simulationManager.Progress())
                {
                    // 모든 경로에 대해 시뮬레이션이 완료됨. isSimEnd = true.
                    grid.InitWeight();
                    SetDirectionSensor();
                    grid.ViewMinPath = true;
                    grid.Liner();
                    isSimulating = false;
                    simulationManager.PrintOut("");
                    simulationManager.initEvacs = false;
                    ScreenShot(simulationManager.delayList[simulationManager.delayList.Count - 1]);
                }
                else
                {
                    ScreenShot(simulationManager.delayList[simulationManager.delayList.Count - 1]);
                }
            }
        }
    }

    // Currently not used. just for memo.
    private Type GetNodeType(int nodeType)
    {
        // ToDo: Implement some other sensors including area.
        switch (nodeType)
        {
            case 33:    // Fire, 16진수 21
                return typeof(NodeFireSensor);
            case 2:     // Water
                return typeof(NodeFireSensor);
            case 3:     // Eearthquake
                return typeof(NodeFireSensor);
            case 39:    // Direction
                return typeof(NodeFireSensor);
            default:
                return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
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
            simulationManager.initEvacs = false;
            simulationManager.isSimEnd = false;
        }
    }

    void OffDanger()
    {
        if (this.image_panel.activeSelf)
        {
            //mQTTManager.PubPeriod(360);
            this.image_panel.SetActive(false);
            grid.InitWeight();
            grid.ViewMinPath = false;
            grid.InitLiner();
        }
        if (musicPlayer != null) musicPlayer.Stop();
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
        SideGUI.HideFloor(LastUpdatedFloor);
        //SideGUI.HideFloor(1);

        simulationManager = ScriptableObject.CreateInstance<SimulationManager3>();
        simulationManager.SetGrid(grid);
        grid.ResetFinalPaths();
        grid.ViewMinPath = false;
        simulationManager.EvacuatersList.Clear();
        this.pathSize = 1;
        // Store paths
        foreach (NodeArea area in NodeManager.GetNodesByType<NodeArea>())
        {
            if (area.Num > 0)
            {
                //simulationManager.SetEvacuaters(this.areaJsons, this.areaNums);
                List<Node3[]> paths = new List<Node3[]>();
                // for targets
                foreach (NodeExit exit in NodeManager.GetNodesByType<NodeExit>())
                {
                    // Calculate path
                    List<Node3> path = new List<Node3>();
                    p = new NavMeshPath();
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

                this.pathSize *= paths.Count;
                simulationManager.AddEvacuater(area, paths);
            }
        }
        Debug.Log(this.pathSize);
        simulationManager.InitSimParam(this.pathSize);
        paths.Clear();
        simulationManager.initEvacs = true;
    }

    public void SetSensorNodes()
    {
        SensorDictionary = new Dictionary<string, Sensors>();

        for (int i = 0; i < sensorNodeJsons.Count; i++)
        {
            Sensors tmp = new Sensors
            {
                gameObject = sensorObjs[i],
                sensor_Attribute = sensorObjs[i].GetComponent<sensor_attribute>()
            };
            if (tmp.gameObject.GetComponent<MeshRenderer>() != null)
                tmp.Material = tmp.gameObject.GetComponent<MeshRenderer>().material;
            else
                tmp.Material = null;
            if (tmp.gameObject.GetComponent<NavMeshObstacle>() != null)
                tmp.navMeshObstacle = tmp.gameObject.GetComponent<NavMeshObstacle>();
            else
                tmp.navMeshObstacle = null;
            if (tmp.gameObject.transform.GetChild(0).GetComponent<MeshRenderer>() != null)
                tmp.Effect = tmp.gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material;
            else
                tmp.Effect = null;
            Debug.Log("add sensor : " + tmp);
            SensorDictionary.Add(sensorNodeJsons[i].nodeId, tmp);
        }
    }

    // 여기서 Target floor와 isDanger를 결정한다. 왜....?
    public bool SetSensorValue()
    {
        List<int> dangerFloors = new List<int>();
        int TargetFloor = 0;
        isDanger = false;
        bool ret = true;

        foreach (string k in SensorDictionary.Keys)
        {
            ret &= SensorDictionary[k].SensorValue();
            if (SensorDictionary[k].sensor_Attribute.one_sensor.disaster)//DisasterEvent일 때
            {
                //scenarioManager.SensorDictionary[md.nodeId].sensor_Attribute.one_sensor.nodeType = md.sensorType;
                switch (SensorDictionary[k].sensor_Attribute.one_sensor.nodeType)
                {
                    case 33://16진수 21
                        //fire
                        //if (SensorDictionary[k].sensor_Attribute.one_sensor.value1 > 70)
                        {
                            Debug.Log("fire 21");
                            TargetFloor = 1;
                            LastUpdatedFloor = BuildingManager.GetFloor(SensorDictionary[k].sensor_Attribute.one_sensor.positions);
                            WarnText.text = "화재 발생";
                            isDanger = true;
                        }

                        break;
                    case 2:
                        //water
                        //if (SensorDictionary[k].sensor_Attribute.one_sensor.value1 > 0)
                        {
                            // select floor
                            Debug.Log("water 2");
                            TargetFloor = floor;
                            WarnText.text = "수재해 발생";
                            LastUpdatedFloor = BuildingManager.GetFloor(SensorDictionary[k].sensor_Attribute.one_sensor.positions);
                            isDanger = true;
                        }
                        break;
                    case 3:
                        // earthquake
                        //if (SensorDictionary[k].sensor_Attribute.one_sensor.value1 > 0)
                        {
                            // select floor
                            Debug.Log("earthquake 3");
                            TargetFloor = 1;
                            WarnText.text = "지진 발생";
                            LastUpdatedFloor = BuildingManager.GetFloor(SensorDictionary[k].sensor_Attribute.one_sensor.positions);
                            isDanger = true;
                        }

                        break;
                    case 39://39
                        {
                            // 방향 지시
                            Debug.Log("39 방향지시");
                            break;
                        }
                }
            }
        }

        if (!isDanger) ret = true;
        return ret;
    }

    IEnumerator screen_pixels()
    {
        sub_camera.enabled = true;
        temp_path.scrshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);

        yield return new WaitForEndOfFrame();
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);//depth : 일단 24
        sub_camera.targetTexture = rt;
        //texture1
        sub_camera.Render();
        RenderTexture.active = rt;
        temp_path.scrshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        temp_path.scrshot.Apply();

    }

    void ScreenShot(float tmpTime)
    {
        temp_path = new screenshot_attr();
        temp_path.time = tmpTime;
        StartCoroutine(screen_pixels());
        sub_camera.enabled = false;
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
