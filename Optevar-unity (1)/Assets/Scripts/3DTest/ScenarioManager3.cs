using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Linq;

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

    bool SetSensor(bool _thres)
    {
        bool isTrans = true;
        if (_thres)
        {
            navMeshObstacle.carving = true;
            Effect.color = new Color(
                Effect.color.r, Effect.color.g, Effect.color.b, trans);
            // Set danger
            if (preDanger == false) isTrans = false;
            preDanger = true;
        }

        else
        {
            if (preDanger == true) isTrans = false;
            preDanger = false;
            navMeshObstacle.carving = false;
            Effect.color = new Color(
                    Effect.color.r, Effect.color.g, Effect.color.b, noTrans);
        }

        return isTrans;
    }
    public bool SensorValue()
    {
        bool ret = true;

        switch (sensor_Attribute.one_sensor.nodeType)
        {
            case 33://16진수 21
                //fire
                Material.color = new Color(1, 1 - sensor_Attribute.one_sensor.value1 * 3 / 255f, 1 - sensor_Attribute.one_sensor.value1 * 3 / 255f);
                //SetSensor(sensor_Attribute.one_sensor.value1 > 70);
                ret = SetSensor(sensor_Attribute.one_sensor.disaster);
                break;
            case 2:
                //water
                Material.color = new Color(1 - sensor_Attribute.one_sensor.value1 * 3 / 255f, 1 - sensor_Attribute.one_sensor.value1 * 3 / 255f, 1);
                //SetSensor(sensor_Attribute.one_sensor.value1 > 0);
                ret = SetSensor(sensor_Attribute.one_sensor.disaster);
                break;
            case 3:
                // earthquake
                Material.color = new Color(1 - sensor_Attribute.one_sensor.value1 * 3 / 255f, 1, 1 - sensor_Attribute.one_sensor.value1 * 3 / 255f);
                //SetSensor(sensor_Attribute.one_sensor.value1 > 0);
                ret = SetSensor(sensor_Attribute.one_sensor.disaster);
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
    MQTTManager3 mQTTManager;
    public Grid3 grid;
    public GameObject building;

    public List<Evacuaters3> EvacuatersList;
    /// Test values
    public GameObject TargetObj;
    public GameObject AreaObj;
    public GameObject SensorObj;
    List<GameObject> TestTargets;

    public List<SensorNodeJson> TestSensorJsons;

    // For experiment
    public string expName = "";
    public float stdVelo = 4f;


    //MQTT, Json Interface elements
    public List<EvacuaterNodeJson> evacuaterNodeJsons;
    public List<SensorNodeJson> sensorNodeJsons;
    public List<ExitNodeJson> exitNodeJsons;
    public List<AreaPositions> areaJsons;
    public Dictionary<string, int> areaNums;
    public Dictionary<string, float> areaVelos;

    //Self data
    List<Node3> Targets;
    NavMeshPath p;
    public SimulationManager3 simulationManager = null;
    int floor;
    List<float> floorHeights;
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
    AudioSource MusicPlayer;


    int mouseInterfaceFlag; // 0=defalut, 1=create, 2=simulate
    List<screenshot_attr> paths = new List<screenshot_attr>();
    screenshot_attr temp_path;

    public Dictionary<string, Sensors> SensorDictionary;

    List<GameObject> areaNumObjs = new List<GameObject>(); // MQTT를 통해 얻은 인원수 가시화를 위한 객체
    List<GameObject> sensorObjs = new List<GameObject>();
    //List<Material> SensorsMaterials;
    //List<NavMeshObstacle> SensorsObs;
    //List<Material> SensorEffects;
    List<Object> CreatedForGUI = new List<Object>();

    // Scenario ScenarioFromJson;
    public void Initiation()////////////////////
    {
        //Canvas initiation
        main_camera = Camera.main;
        if (sub_camera == null)
            sub_camera = main_camera.transform.GetChild(0).GetComponent<Camera>();
        //sub_camera = main_camera.GetComponentInChildren<Camera>();
        content = GameObject.Find("scr_shot_panel");
        if (image_panel == null)
            image_panel = GameObject.Find("path_window_region");
        if (image_ob1 == null)
        {
            Transform tmpOb = image_panel.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
            content = tmpOb.parent.gameObject;
            time_text = tmpOb.GetChild(1).GetComponent<Text>();
            image_ob1 = tmpOb.GetChild(0).GetComponent<Image>();
        }
        if (SideGUI == null)
            SideGUI = GameObject.Find("FloorBtns").GetComponent<SideGUI>();

        if (WarnText == null)
            WarnText = GameObject.Find("WarnText").GetComponent<Text>();
        InitGrid();

        InitMQTT();
        isDanger = false;
        SideGUI.InitBuildingInfo(this.building);
        this.floorHeights = SideGUI.FloorHeights;
        MusicPlayer = this.gameObject.GetComponent<AudioSource>();
    }
    public void SetDefault()/////////////////////////////
    {
        InitAreaGUI();
        if (SensorDictionary != null)
            foreach (string i in SensorDictionary.Keys)
                SensorDictionary[i].Init();
        if (mQTTManager != null)
            mQTTManager.Close();
    }

    // Update is called once per frame
    void Update()
    {
        if (simulationManager != null)
        {
            Moniter();
        }
    }

    void InitAreaGUI()
    {
        for (int i = 0; i < areaNumObjs.Count; i++)
            Destroy(areaNumObjs[i]);
        areaNumObjs.Clear();
    }

    // Batch mode interface에서 Monitering mode로 넘어올 때 호출해주면 됨.
    public void SetLists(List<EvacuaterNodeJson> evacuaterNodeJsons, List<SensorNodeJson> sensorNodeJsons,
        List<ExitNodeJson> exitNodeJsons, List<AreaPositions> areaJsons)
    {
        this.evacuaterNodeJsons = evacuaterNodeJsons;
        this.sensorNodeJsons = sensorNodeJsons;
        this.exitNodeJsons = exitNodeJsons;
        this.areaJsons = areaJsons;

    }
    public void SetLists(Scenario scene)
    {
        if (scene == null)
        {
            Debug.Log("The Scenario is null.");
            return;
        }
        this.evacuaterNodeJsons = new List<EvacuaterNodeJson>();
        evacuaterNodeJsons.AddRange(scene.evacuaterNodeJsons);
        this.sensorNodeJsons = new List<SensorNodeJson>();
        sensorNodeJsons.AddRange(scene.sensorNodeJsons);
        for (int i = 0; i < sensorNodeJsons.Count; i++)
            sensorNodeJsons[i].value1 = 0;
        this.exitNodeJsons = new List<ExitNodeJson>();
        exitNodeJsons.AddRange(scene.exitNodeJsons);
        this.areaJsons = new List<AreaPositions>();
        areaJsons.AddRange(scene.areaPositionJsons);
    }
    public void SetLists(Scenario scene, List<GameObject> sensorList)//센서 오브젝트들을 받아***
    {
        if (scene == null)
        {
            Debug.Log("The Scenario is null.");
            return;
        }
        this.evacuaterNodeJsons = new List<EvacuaterNodeJson>();
        evacuaterNodeJsons.AddRange(scene.evacuaterNodeJsons);
        this.sensorNodeJsons = new List<SensorNodeJson>();
        sensorNodeJsons.AddRange(scene.sensorNodeJsons);
        for (int i = 0; i < sensorNodeJsons.Count; i++)
            if (sensorNodeJsons[i].nodeType != 39) sensorNodeJsons[i].value1 = 0;
            else sensorNodeJsons[i].value1 = -1;

        this.exitNodeJsons = new List<ExitNodeJson>();
        exitNodeJsons.AddRange(scene.exitNodeJsons);
        this.areaJsons = new List<AreaPositions>();
        areaJsons.AddRange(scene.areaPositionJsons);

        this.sensorObjs = sensorList;
    }
    void InitGrid()
    {
        if (grid == null)
        {
            grid = GetComponent<Grid3>();
        }
        else
        {
            Debug.Log("CustomGrid is loaded.");
        }
        if (building == null)
            building = GameObject.Find("Building");
        NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
        InitBuildingInfo();
        grid.CreateGrid(tri.vertices, floor);

    }
    void InitBuildingInfo()
    {
        floor = building.transform.childCount;
    }

    public void InitMoniteringMode()
    {

        isAreaChanged = false;
        // Set targets
        SetTargetNodes(this.exitNodeJsons);
        // Set sensors
        SetSensorNodes();
        SetSensorValue();
        // Set area
        SetArea(this.areaJsons);
        // Start MQTT 
        mQTTManager.SetLists(this);
        //grid.StorePaths(grid.GetTargetNodes());
        simulationManager = ScriptableObject.CreateInstance<SimulationManager3>();
        simulationManager.SetGrid(grid);
        //simulationManager = null;

    }


    public void Moniter()
    {
        if (isAreaChanged)
        {
            // MQTT 메시지를 통해 대피자 수가 수정되었을 경우에만 isAreaChanged == true
            InitAreaGUI();
            for (int i = 0; i < this.areaJsons.Count; i++)
                SetMQTTNum(areaNums[areaJsons[i].areaId], areaJsons[i].position);
            isAreaChanged = false;
            if (!isSimulating)
            {
                // 시뮬레이션이 진행중이지 않을 때만 업데이트.
                simulationManager.initEvacs = false;
                simulationManager.isSimEnd = false;
                return;
            }

        }
        if (isSensorUpdated)
        {
            // 위험 지역이 변화했을 때만 initEvacs == true;
            // 센서값이 수정되더라도 시뮬레이션을 꼭 돌리지 않아도 되는 경우가 있기 때문에 SetSensorValue() 에서 처리
            if (!SetSensorValue())
            {
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

                if (simulationManager.Progress())
                {
                    // 모든 경로에 대해 시뮬레이션이 완료됨. isSimEnd = true.
                    grid.InitWeight();
                    SetDirectionSensor();
                    grid.ViewMinPath = true;
                    grid.Liner();
                    isSimulating = false;
                    simulationManager.PrintOut(expName);
                    simulationManager.initEvacs = false;
                    ScreenShot(simulationManager.delayList[simulationManager.delayList.Count - 1]);

                }

                else
                {
                    ScreenShot(simulationManager.delayList[simulationManager.delayList.Count - 1]);
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
        if (MusicPlayer != null) MusicPlayer.Stop();
    }
    void Siren()
    {
        //MusicPlayer.loop = true;
        if (!MusicPlayer.isPlaying)
            MusicPlayer.Play();
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
        for (int i = 0; i < this.areaJsons.Count; i++)
        {
            if (areaNums[areaJsons[i].areaId] > 0)
            {
                //simulationManager.SetEvacuaters(this.areaJsons, this.areaNums);
                List<Node3[]> paths = new List<Node3[]>();
                // for targets
                for (int j = 0; j < Targets.Count; j++)
                {
                    List<Node3> path = new List<Node3>();
                    p = new NavMeshPath();
                    NavMesh.CalculatePath(areaJsons[i].position, Targets[j].position, -1, p);
                    for (int o = 0; o < p.corners.Length - 1; o++)
                    {
                        path.AddRange(grid.GetNodesFromLine(p.corners[o], p.corners[o + 1]));
                    }
                    for (int o = 0; o < path.Count - 1; o++)
                    {
                        if (path[o] == path[o + 1])
                        {
                            while (path[o] == path[o + 1])
                                path.Remove(path[o + 1]);
                        }
                    }

                    if (path.Count > 0)
                        paths.Add(path.ToArray());
                }
                this.pathSize *= paths.Count;
                if (areaVelos.ContainsKey(areaJsons[i].areaId))
                {
                    simulationManager.AddEvacuater(areaJsons[i].position, areaNums[areaJsons[i].areaId], paths, areaVelos[areaJsons[i].areaId]);
                }
                else
                {
                    simulationManager.AddEvacuater(areaJsons[i].position, areaNums[areaJsons[i].areaId], paths);
                }
            }
        }
        Debug.Log(this.pathSize);
        // Init simQ
        // DEP simulationManager.InitSimQueue();
        //this.pathSize = (int)(Mathf.Pow(this.Targets.Count, simulationManager.EvacuatersList.Count));
        // DEP simulationManager.InitPathSize(this.pathSize);
        simulationManager.InitSimParam(this.pathSize);
        // Init GUI values
        paths.Clear();
        simulationManager.initEvacs = true;
    }

    void InitMQTT()
    {
        if (this.mQTTManager == null)
        {
            this.mQTTManager = this.GetComponentInChildren<MQTTManager3>();
        }
        this.mQTTManager.Init();
        Debug.Log("MQTTManager is loaded.");
    }
    public void MQTTAreaNums()
    {

    }
    void SetGrid()
    {
        //TODO
        // Set targets and sensors and calc & store 'default shortest paths'.

    }
    void SetGrid(List<ExitNodeJson> _targets)
    {
        //TODO
        // Set targets and sensors and calc & store 'default shortest paths'.

    }

    public void SetTargetNodes(List<ExitNodeJson> _targets)
    {
        this.Targets = new List<Node3>();

        for (int i = 0; i < _targets.Count; i++)
        {
            Targets.Add(grid.GetNodeFromPosition(_targets[i].positions));
        }
    }

    public void SetTargetNodes(List<ExitNodeJson> _targets, int _floor)
    {
        if (_targets == null) _targets = this.exitNodeJsons;
        this.Targets = new List<Node3>();

        for (int i = 0; i < _targets.Count; i++)
        {
            if (SideGUI.HeightToFloor(_targets[i].positions) == _floor)
                Targets.Add(grid.GetNodeFromPosition(_targets[i].positions));
        }
    }

    public void SetArea(List<AreaPositions> _area)
    {
        areaNums = new Dictionary<string, int>();
        this.areaVelos = new Dictionary<string, float>();

        for (int i = 0; i < _area.Count; i++)
        {
            areaNums.Add(_area[i].areaId, 0);
            areaVelos.Add(_area[i].areaId, 4);
        }
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

    public void SetMQTTNum(int number, Vector3 startPosition)//areaId, position 씬에 area별 사람 수 띄우기
    {
        GameObject ori_cube = GameObject.Find("area_cube");
        GameObject new_cube = Instantiate(ori_cube, startPosition, ori_cube.transform.rotation);
        new_cube.transform.position = new Vector3(startPosition.x, startPosition.y + .1f, startPosition.z - 1);
        new_cube.GetComponent<TextMesh>().text = number.ToString();
        new_cube.transform.SetParent(GameObject.Find("all_objects").transform);
        areaNumObjs.Add(new_cube);
        Debug.Log("Done");
    }
    public void InitSensors(int idx, Vector3 position)
    {
        GameObject origin = GameObject.Find("sensor_cube");
        GameObject newo = Instantiate(origin, position, origin.transform.rotation);
        newo.transform.position = new Vector3(position.x, 1.5f, position.z);
        //newo.GetComponent<>
    }
    public void SetSensorValue(int idx, float value)
    {

    }
    public bool SetSensorValue()
    {

        List<int> dangerFloors = new List<int>();
        int TargetFloor = 0;
        isDanger = false;
        bool ret = true;

        foreach (string k in SensorDictionary.Keys)
        {
            ret &= SensorDictionary[k].SensorValue();///////////////////////////

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
                            LastUpdatedFloor = SideGUI.HeightToFloor(SensorDictionary[k].sensor_Attribute.one_sensor.positions);
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
                            LastUpdatedFloor = SideGUI.HeightToFloor(SensorDictionary[k].sensor_Attribute.one_sensor.positions);
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
                            LastUpdatedFloor = SideGUI.HeightToFloor(SensorDictionary[k].sensor_Attribute.one_sensor.positions);
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


        if (isDanger)
            SetTargetNodes(null, TargetFloor);
        else
            ret = true;
        this.isSensorUpdated = false;

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
            for (int i = 0; i < CreatedForGUI.Count; i++)
                Destroy(CreatedForGUI[i]);
            CreatedForGUI.Clear();

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
                    CreatedForGUI.Add(new_image_ob.gameObject);
                    CreatedForGUI.Add(new_text.gameObject);
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
