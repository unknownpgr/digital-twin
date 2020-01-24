using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioManager : MonoBehaviour
{
    MQTTManager mQTTManager;
    public CustomGrid grid;

    public List<Evacuaters> EvacuatersList;

    //MQTT, Json Interface elements
    public List<EvacuaterNodeJson> evacuaterNodeJsons;
    public List<SensorNodeJson> sensorNodeJsons;
    public List<ExitNodeJson> exitNodeJsons;
    public List<AreaPositions> areaJsons;
    public Dictionary<string, int> areaNums;

    public SimulationManager simulationManager = null;

    public bool isAreaChanged;
    public bool DEMOSTART = false;
    /****************/
    float time;

    Camera sub_camera;
    Camera main_camera;
    private Texture2D scrshot_tecture;
    public GameObject image_panel;
    GameObject contect;
    Text time_text;
    Image image_ob1;

    int mouseInterfaceFlag; // 0=defalut, 1=create, 2=simulate
    List<screenshot_attr> paths = new List<screenshot_attr>();
    screenshot_attr temp_path;
    List<GameObject> areaNumObjs = new List<GameObject>();

    // Scenario ScenarioFromJson;
    private void Awake()
    {
        main_camera = Camera.main;
        if (sub_camera == null)
            sub_camera = main_camera.transform.GetChild(0).GetComponent<Camera>();
        //sub_camera = main_camera.GetComponentInChildren<Camera>();
        contect = GameObject.Find("scr_shot_panel");
        if (image_panel == null)
            image_panel = GameObject.Find("path_window_region");
        if (image_ob1 == null)
        {
            Transform tmpOb = image_panel.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
            time_text = tmpOb.GetChild(1).GetComponent<Text>();
            image_ob1 = tmpOb.GetChild(0).GetComponent<Image>();
        }
        InitGrid();
        InitMQTT();

    }
    // Start is called before the first frame update
    void Start()
    {
        /****************/
        //time_text = GameObject.Find("time_text").GetComponent<Text>();//되는지 확인
        //image_ob1 = GameObject.Find("Image_tex").GetComponent<Image>();

        main_camera.enabled = true;
        sub_camera.enabled = false;
        image_panel.active = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (simulationManager != null)
        {
            Moniter();
        }
    }
    public void SetMQTTNum(int number, Vector3 startPosition)
    {
        GameObject ori_cube = GameObject.Find("area_cube");
        GameObject new_cube = Instantiate(ori_cube, startPosition, ori_cube.transform.rotation);
        new_cube.transform.position = new Vector3(startPosition.x, 1.5f, startPosition.z);
        new_cube.GetComponent<TextMesh>().text = number.ToString();
        areaNumObjs.Add(new_cube);
        Debug.Log("Done");
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
        this.evacuaterNodeJsons = new List<EvacuaterNodeJson>();
        evacuaterNodeJsons.AddRange(scene.evacuaterNodeJsons);
        this.sensorNodeJsons = new List<SensorNodeJson>();
        sensorNodeJsons.AddRange(scene.sensorNodeJsons);
        this.exitNodeJsons = new List<ExitNodeJson>();
        exitNodeJsons.AddRange(scene.exitNodeJsons);
        this.areaJsons = new List<AreaPositions>();
        areaJsons.AddRange(scene.areaPositionJsons);
    }

    void InitGrid()
    {
        grid = GetComponent<CustomGrid>();
        if (grid != null)
        {
            Debug.Log("CustomGrid is loaded.");
        }
        
    }
    public void InitMoniteringMode()
    {
        isAreaChanged = false;
        // Set targets
        SetTargetNodes(this.exitNodeJsons);
        // Set sensors

        // Set area
        areaNums = new Dictionary<string, int>();
        for (int i = 0; i < areaJsons.Count; i++)
        {
            areaNums.Add(areaJsons[i].areaId, 0);
        }
        // Start MQTT 
        //mQTTManager.SetLists(evacuaterNodeJsons, sensorNodeJsons, exitNodeJsons, areaJsons, areaNums);
        mQTTManager.SetLists(this);
        grid.StorePaths(grid.GetTargetNodes());
        simulationManager = ScriptableObject.CreateInstance<SimulationManager>();
        simulationManager.SetGrid(grid);
    }


    public void Moniter()
    {
        if (isAreaChanged)
        {
            simulationManager = ScriptableObject.CreateInstance<SimulationManager>();
            simulationManager.SetGrid(grid);
            simulationManager.SetEvacuaters(this.areaJsons, this.areaNums);
            for (int i = 0; i < areaNumObjs.Count; i++)
                Destroy(areaNumObjs[i]);
            areaNumObjs.Clear();
            for (int i = 0; i < this.areaJsons.Count; i++)
                SetMQTTNum(areaNums[areaJsons[i].areaId], areaJsons[i].position);
            isAreaChanged = false;
        }
        if (DEMOSTART)
        if (simulationManager.EvacuatersList.Count > 0)
            if (simulationManager.Progress())
            {
                // Simulation ends up.
                ScreenShot(simulationManager.delayList[simulationManager.delayList.Count-1]);
                grid.InitWeight();
                grid.InitDangerFlag();
                simulationManager.EvacuatersList.Clear();
                grid.ViewMinPath = true;
                grid.Liner();
                DEMOSTART = false;
                    simulationManager.PrintOut();
            }
            else
            {
                ScreenShot(simulationManager.delayList[simulationManager.delayList.Count - 1]);
            }
    }
    void InitMQTT()
    {
        if (this.mQTTManager == null)
        {
            this.mQTTManager = ScriptableObject.CreateInstance<MQTTManager>();
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

    void MQTTMsg()
    {

    }



    public void SetTargetNodes(List<ExitNodeJson> _targets)
    {
        grid.TargetNodes.Clear();
        for (int i = 0; i < _targets.Count; i++)
        {
            grid.AddNode(grid.NodeFromWorldPosition(_targets[i].positions));
            grid.AddTargetNode(grid.NodeFromWorldPosition(_targets[i].positions));
        }
    }

    /*
    // Reset
    void ResetEvacuaters()
    {
        grid.ResetFinalPaths();
        grid.InitWeight();
        EvacuatersList.Clear();
    }
    // Create EvacuaterList using json data
    void AddEvacuaters(List<Evacuaters> _list, NodePositions np)
    {
        for (int i = 0; i < np.positions.Length; i++) {
            Evacuaters sc = new Evacuaters(evacuaterNum);
            sc.SetParams(np.positions[i], grid, new Vector3(0, 0, 0));
            sc.SetVelocity(tmpVelocity);
            _list.Add(sc);
            //pfController.Calc(sc);
        }
    }

    void AddEvacuaters(List<Evacuaters> _list, EvacuaterNodeJson[] Evacs)
    {
        for (int i = 0; i < Evacs.Length; i++)
        {
            Evacuaters sc = new Evacuaters(evacuaterNum);
            sc.SetParams(Evacs[i].positions, grid, new Vector3(0, 0, 0));
            sc.SetVelocity(tmpVelocity);
            _list.Add(sc);
            //pfController.Calc(sc);
        }
    }

    

    void LoadScenarioFromJson(string _json)
    {
        JsonParser jp = new JsonParser();
        ScenarioFromJson = jp.Load<NodePositions>(_json);
        //ScenarioFromJson = jp.Load<Scenario>(_json);
        EvacuatersList = new List<Evacuaters>();
        AddEvacuaters(EvacuatersList, ScenarioFromJson);

    }

    */


    /****************/

    IEnumerator screen_pixels()
    {


        sub_camera.enabled = true;
        image_panel.active = true;
        //scrshot_tecture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);

        temp_path.scrshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);

        yield return new WaitForEndOfFrame();
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);//depth : 일단 24
        sub_camera.targetTexture = rt;
        //texture1
        sub_camera.Render();
        RenderTexture.active = rt;
        temp_path.scrshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        temp_path.scrshot.Apply();
        //Image_object1.GetComponent<MeshRenderer>().material.SetTexture("ScreenShot_texture1", Texture1);
        //Image_object1.GetComponent<Image>().material.SetTexture("ScreenShot_texture1", Texture1);
        //image_ob1.transform.SetParent(image_panel.transform, false);


        //image_panel
        //Debug.Log(image_panel.name);
        //Debug.Log("적용됨");

    }


    void ScreenShot(float tmpTime)
    {
        /****************/
        temp_path = new screenshot_attr();
        temp_path.time = tmpTime;
        StartCoroutine(screen_pixels());
        //screen_pixels();
        sub_camera.enabled = false;
        paths.Add(temp_path);
        //index++;
        if (Mathf.Pow(grid.GetTargetNodes().Length, simulationManager.EvacuatersList.Count) == paths.Count)
        {
            //여기에 띄우기

            //Debug.Log("총수" + paths.Count);

            paths.Sort(delegate (screenshot_attr x, screenshot_attr y) {
                if (x.time > y.time) return 1;
                else if (x.time < y.time) return -1;
                return 0;
            });

            for (int r = 0; r < paths.Count; r++)
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
                }
                new_image_ob.transform.SetParent(contect.transform);
                new_image_ob.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, image_ob1.rectTransform.rect.width);
                new_image_ob.transform.localPosition = new Vector3(image_ob1.transform.localPosition.x, +image_ob1.transform.localPosition.y - (image_ob1.rectTransform.rect.height + 35) * r, image_ob1.transform.localPosition.z);
                new_image_ob.transform.rotation = image_ob1.transform.rotation;
                new_image_ob.transform.localScale = image_ob1.transform.localScale;
                new_image_ob.GetComponent<Image>().sprite = Sprite.Create(paths[r].scrshot, new Rect(0, 0, Screen.width, Screen.height), new Vector2(0.5f, 0.5f));

                new_text.transform.SetParent(contect.transform);
                new_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, time_text.rectTransform.rect.width);
                new_text.transform.localPosition = new Vector3(time_text.transform.localPosition.x, +time_text.transform.localPosition.y - (image_ob1.rectTransform.rect.height + 35) * r, time_text.transform.localPosition.z);
                new_text.transform.rotation = time_text.transform.rotation;
                new_text.transform.localScale = time_text.transform.localScale;

                new_text.GetComponent<Text>().text = "Time : " + paths[r].time.ToString();
                //Debug.Log("r : " + r);
                //Debug.Log("걸린시간 : " + paths[r].time);
            }

        }
    }
}
