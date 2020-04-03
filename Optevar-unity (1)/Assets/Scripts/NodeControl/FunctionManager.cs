﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class FunctionManager : MonoBehaviour
{
    // Building information
    public static string BuildingPath;
    public static string BuildingName = "ETRI"; // Set default building to ETRI

    // Popup associated values
    private static Vector3 POPUP_SHOW = new Vector2(0, -100);
    private static Vector3 POPUP_HIDE = new Vector2(0, 200);
    private GameObject popup;                   // Popup message box
    private RectTransform popupTransform;       // Transform
    private static Text popupText;              // Text
    private static float popupLifetime = 0;     // Popup lifetime. hide if 0
    public static float POPUP_DURATION = 5;     // Popup default lifetime.

    // Current canvas
    public Canvas canvas;
    // Dictionary of uis
    private static Dictionary<string, Transform> uis = new Dictionary<string, Transform>();
    // Floor side display buttons
    private List<GameObject> floorButtons = new List<GameObject>();
    // Sensor buttons of sensor window
    private Dictionary<string, GameObject> sensorButtons = new Dictionary<string, GameObject>();

    // Text of Date And Time UI
    public Text dateText;
    public Text timeText;

    // Program mode
    private static bool isPlacingMode = true;
    public static bool IsPlacingMode
    {
        get { return isPlacingMode; }
    }
    public static bool IsMonitoringMode
    {
        get { return !isPlacingMode; }
    }

    // Itself
    public static FunctionManager self;

    // Get UI by name
    // This function is required because GameObject.Find doesn't find inactive gameobject.
    public static Transform Find(string uiName)
    {
        return uis[uiName];
    }

    // Start is called before the first frame update
    void Start()
    {
        // Set itself
        self = this;

        // Initialzie ui
        RecursiveRegisterChild(canvas.transform, uis);

        // Initialize popup
        popup = Find("popup").gameObject;
        popupTransform = popup.GetComponent<RectTransform>();
        popupText = popupTransform.GetChild(0).GetComponent<Text>();
        popupTransform.anchoredPosition = POPUP_HIDE;

        // Set building name
        Find("text_building_name").GetComponent<Text>().text = BuildingName;

        // Initialize mouse
        MouseManager.ToNormalMode();

        // Remove all existing nodes
        NodeManager.ResetAll();

        // Load builing
        GameObject building = BuildingManager.LoadSkp(BuildingName);

        if (!building) Popup("That is not a valid building.");
        else
        {
            // Set building floors

            // If building has multiple floors
            if (BuildingManager.FloorsCount > 1)
            {
                // Get exsisting floor button
                GameObject floorButton = Find("button_floor").gameObject;
                Transform plainFloors = Find("panel_floor");

                // Create floor buttons
                GameObject newButton;
                for (int i = BuildingManager.FloorsCount - 1; i >= 0; i--)
                {
                    newButton = Instantiate(floorButton);
                    newButton.transform.SetParent(plainFloors, false);
                    newButton.transform.localPosition = Vector3.zero;
                    newButton.transform.GetChild(0).GetComponent<Text>().text = "F" + (i + 1);

                    // var i must be copied, or it would be always FloorsCount-1 as it is used from inline functions that share context.
                    int k = i;
                    newButton.GetComponent<Button>().onClick.AddListener(() => { OnSetFloor(k); });
                    floorButtons.Add(newButton);
                }

                // Destroy old button
                Destroy(floorButton);
            }
            else
            {
                // Hide floors view
                Find("scrollview_floors").gameObject.SetActive(false);
            }

            // Start clock
            StartCoroutine(UpdateDateAndTime());
        }

        // Load node data from database
        // 이 코드 대신에 DB에서 로드하는 코드가 들어가야 한다.
        string path = Application.dataPath + "/Resources/scenario_jsons/NEW.json";
        string nodeString = File.ReadAllText(path);
        // File.WriteAllText(path, nodeString);
        NodeManager.Instantiate(nodeString);

        // Initialize sensor buttons and create existing node
        GameObject sensorButton = Find("button_sensor_ID").gameObject;
        Transform sensorPanel = Find("panel_sensor");
        GameObject newSensorBtn;
        foreach (string physicalID in NodeManager.GetNodeIDs())
        {
            NodeManager nm = NodeManager.GetNodeByID(physicalID);
            newSensorBtn = Instantiate(sensorButton);
            newSensorBtn.transform.SetParent(sensorPanel, false);
            newSensorBtn.transform.localPosition = Vector3.zero;
            newSensorBtn.transform.GetChild(0).GetComponent<Text>().text = nm.DisplayName;
            newSensorBtn.GetComponent<Button>().onClick.AddListener(() => OnSensorSelected(physicalID));
            sensorButtons.Add(nm.PhysicalID, newSensorBtn);
        }
        Destroy(sensorButton);

        // Add callback listener
        NodeManager.OnNodeStateChanged += OnSensorStateUpdated;
        OnSensorStateUpdated();

        // localfiletext_4.cs
        GameObject jsonFileButton = Find("button_json_file").gameObject;
        Transform jsonFilePanel = Find("panel_json_file");
        GameObject newJsonFileBtn;

        // Close picture window
        Find("window_screenshot").gameObject.SetActive(false);
    }

    // floor starts from 0. 1st floor = 0
    private void OnSetFloor(int floor)
    {
        for (int i = 0; i < BuildingManager.FloorsCount; i++)
        {
            if (i <= floor) BuildingManager.Floors[i].SetActive(true);
            else BuildingManager.Floors[i].SetActive(false);

            if (i != floor) floorButtons[i].GetComponent<Image>().color = new Color(200, 200, 200);
            else floorButtons[i].GetComponent<Image>().color = Color.white;
        }

        foreach (NodeManager node in NodeManager.GetAll())
        {
            node.Hide = BuildingManager.GetFloor(node.Position) > floor;
        }
    }

    // Update is called once per frame
    float t;
    void Update()
    {
        UpdatePopup();
    }

    // Show popup message
    public static void Popup(string text)
    {
        if (popupLifetime > 0) popupLifetime = POPUP_DURATION - 1.0f;
        else popupLifetime = POPUP_DURATION;        // Set lifetime to 5 sec
        popupText.text = text;                      // Set text fo popup message
    }

    // Update date and time Text
    private IEnumerator UpdateDateAndTime()
    {
        while (true)
        {
            System.DateTime dateTime = System.DateTime.Now;
            dateText.text = dateTime.ToString("yyyy/MM/dd");
            timeText.text = dateTime.ToString("hh시 mm분 ss초");
            yield return new WaitForSeconds(1);
        }
    }

    private void UpdatePopup()
    {
        if (popupLifetime > 0)
        {
            // Showing
            if (popupLifetime > 1.0f)
            {
                t = POPUP_DURATION - popupLifetime;
                if (t > 1) t = 1;
            }
            // Hiding
            else
            {
                t = popupLifetime;
            }
            popupTransform.anchoredPosition = Vector2.Lerp(POPUP_HIDE, POPUP_SHOW, WindowManager.SmoothMove(t));
            popupLifetime -= Time.deltaTime;
        }
    }

    private static void RecursiveRegisterChild(Transform parent, Dictionary<string, Transform> dict)
    {
        if (!dict.ContainsKey(parent.name)) dict.Add(parent.name, parent);
        foreach (Transform child in parent) RecursiveRegisterChild(child, dict);
    }

    //=======[ Callback functions ]=========================================================

    // Called when sensor create button clicked
    public void OnCreateSensor()
    {
        WindowManager sensorWindow = WindowManager.GetWindow("window_sensor");
        sensorWindow.SetVisible(true);
    }

    public void OnSensorSelected(string nodeID)
    {
        NodeManager node = NodeManager.GetNodeByID(nodeID);
        MouseManager.ToNodePlaceMode(node);
    }

    public void OnLoadJson()
    {
        WindowManager loadJsonWindow = WindowManager.GetWindow("window_load_json");
        loadJsonWindow.SetVisible(true);
    }

    public void OnJsonFileSelected()
    {
        // 선택된 Json File 불러오기
    }

    public void OnSaveJson()
    {
        WindowManager loadJsonWindow = WindowManager.GetWindow("window_save_json");
        loadJsonWindow.SetVisible(true);
    }

    public void OnModeChange()
    {
        WindowManager.CloseAll();

        if (IsPlacingMode)
        {
            // Placing mode to monitoring mode
            Find("text_mode").GetComponent<Text>().text = "배치 모드";
            Find("layout_buttons").gameObject.SetActive(false);
            ScenarioManager.singleTon.Init();
        }
        else
        {
            // Monitoring mode to placing mode
            Find("text_mode").GetComponent<Text>().text = "모니터링 모드";
            Find("layout_buttons").gameObject.SetActive(true);
            ScenarioManager.singleTon.SetDefault();

            // Close picture window
            Find("window_screenshot").gameObject.SetActive(false);
        }

        isPlacingMode = !isPlacingMode;
    }

    public void OnSensorStateUpdated()
    {
        foreach (string physicalID in sensorButtons.Keys)
        {
            NodeManager nm = NodeManager.GetNodeByID(physicalID);
            Color color;
            switch (nm.State)
            {
                case NodeManager.NodeState.STATE_INITIALIZED:
                    color = new Color(.7f, .7f, 1);
                    break;
                case NodeManager.NodeState.STATE_PLACING:
                    color = new Color(.7f, .7f, .7f);
                    break;
                default: // NodeManager.NodeState.STATE_UNINITIALIZED
                    color = new Color(1, .7f, .7f);
                    break;
            }
            sensorButtons[physicalID].GetComponent<Image>().color = color;
        }

        string path = Application.dataPath + "/Resources/scenario_jsons/NEW.json";
        File.WriteAllText(path, NodeManager.Jsonfy());
    }
}