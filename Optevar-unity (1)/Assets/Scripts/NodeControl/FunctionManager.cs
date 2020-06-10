using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using System;

public class FunctionManager : MonoBehaviour
{
    // Building information
    public static string BuildingPath;
    public static string BuildingName = "다중이용시설(다층)"; // Set default building to ETRI

    // Current canvas
    public Canvas canvas;
    // Dictionary of uis
    private static Dictionary<string, Transform> uis = new Dictionary<string, Transform>();
    // Floor side display buttons
    private List<GameObject> floorButtons = new List<GameObject>();
    // Sensor buttons of sensor window

    // Texts of date and time UI
    private Transform dtPanel;
    private Text dateText;
    private Text timeText;

    // elements of node information window
    private Transform nodeInfos;
    private InputField nodeID;
    private InputField nodeType;
    private Text nodeValue;
    private Transform fireInfos;


    private Button placingBtn;
    private Button moniteringBtn;
    private Image placingBtnImage;
    private Image moniteringBtnImage;
    private CanvasGroup placingBtnCG;
    private CanvasGroup moniteringBtnCG;

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

        // Set building name
        Find("text_building_name").GetComponent<Text>().text = BuildingName;

        // Initialize mouse
        MouseManager.ToNormalMode();

        // Remove all existing nodes
        NodeManager.ResetAll();

        // Load builing
        GameObject building = BuildingManager.LoadSkp(BuildingName);

        if (!building) Popup.Show("That is not a valid building.");
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

            // Show floors
            OnSetFloor(BuildingManager.FloorsCount - 1);
        }

        // Set date and time texts
        dtPanel = Find("date_and_time");
        dateText = dtPanel.GetChild(0).GetComponent<Text>();
        timeText = dtPanel.GetChild(1).GetComponent<Text>();

        // Get elements of node information window
        nodeInfos = Find("window_node_info").GetChild(1);
        nodeID = nodeInfos.GetChild(0).GetChild(1).GetComponent<InputField>();
        nodeType = nodeInfos.GetChild(1).GetChild(1).GetComponent<InputField>();
        nodeValue = nodeInfos.GetChild(2).GetChild(1).GetComponentInChildren<Text>();
        fireInfos = nodeInfos.GetChild(3);


        Transform modeBtnTransform = Find("button_mode");
        GameObject placingPart = modeBtnTransform.GetChild(0).gameObject;
        GameObject moniteringPart = modeBtnTransform.GetChild(1).gameObject;

        placingBtn = placingPart.GetComponent<Button>();
        moniteringBtn = moniteringPart.GetComponent<Button>();
        placingBtnImage = placingPart.GetComponent<Image>();
        moniteringBtnImage = moniteringPart.GetComponent<Image>();
        placingBtnCG = placingPart.GetComponent<CanvasGroup>();
        moniteringBtnCG = moniteringPart.GetComponent<CanvasGroup>();

        SetModeButtonColor(IsPlacingMode);

        // Start clock
        StartCoroutine(UpdateDateAndTime());

        // Add callback listener
        MouseManager.OnNodeClicked -= OnNodeSelected; // Remove exsiting callback to prevent duplicated call
        MouseManager.OnNodeClicked += OnNodeSelected;
    }

    // floor starts from 0. 1st floor = 0
    private void OnSetFloor(int floor)
    {
        // If given building has only one floor, ignore floor setting.
        if (BuildingManager.FloorsCount == 1) return;

        for (int i = 0; i < BuildingManager.FloorsCount; i++)
        {
            if (i <= floor) BuildingManager.Floors[i].SetActive(true);
            else BuildingManager.Floors[i].SetActive(false);

            if (BuildingManager.FloorsCount - i - 1 > floor) floorButtons[i].GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
            else floorButtons[i].GetComponent<Image>().color = new Color(1, 1, 1, 0.9f);
        }

        foreach (NodeManager node in NodeManager.GetAll())
        {
            node.Hide = BuildingManager.GetFloor(node.Position) > floor;
        }
    }

    // Update date and time Text of UI
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

    public void OnCreateArea()
    {
        WindowManager areaWindow = WindowManager.GetWindow("window_area");
        areaWindow.SetVisible(true);
    }

    public void OnCreateExit()
    {
        WindowManager exitWindow = WindowManager.GetWindow("window_exit");
        exitWindow.SetVisible(true);
    }

    public void OnInitialize()
    {
        WindowManager initWindow = WindowManager.GetWindow("window_init");
        initWindow.SetVisible(true);
    }

    public void OnInitializeNode()
    {
        NodeManager.ResetAll();
        WindowManager initWindow = WindowManager.GetWindow("window_init");
        initWindow.SetVisible(false);
        Popup.Show("초기화되었습니다.");
    }

    public void OnCreateVirtualNode()
    {
        WindowManager createVirtualWindow = WindowManager.GetWindow("window_create_virtual_node");
        createVirtualWindow.SetVisible(true);
    }

    public void OnClickInformation()
    {
        WindowManager informationWindow = WindowManager.GetWindow("window_information");
        informationWindow.SetVisible(true);
    }

    public void OnLoadBuildingInfo()
    {

    }

    public void OnLoadMQTTInfo()
    {

    }

    public void OnLoadSystemInfo()
    {

    }

    public void OnModeChange(bool terminateAll)
    {
        // ToDo : 
        // terminateAll은 임시로 설정해둔 변수다.
        // 만약 terminate all이 참이면 싹다 꺼버리고, 아니면 그대로 두고 종료만 한다.

        WindowManager.CloseAll();

        if (IsPlacingMode)
        {
            SetModeButtonColor(!IsPlacingMode);

            // Placing mode to monitoring mode
            Find("layout_buttons").gameObject.SetActive(false);
            // Find("window_graph").gameObject.GetComponent<WindowManager>().SetVisible(true);
            Find("button_end_simulation").gameObject.SetActive(true);
            OnSetFloor(BuildingManager.FloorsCount - 1);
            ScenarioManager.singleTon.Init();
        }
        else
        {
            SetModeButtonColor(!IsPlacingMode);

            // Monitoring mode to placing mode
            Find("layout_buttons").gameObject.SetActive(true);
            Find("warning_box").gameObject.SetActive(false);
            Find("button_end_simulation").gameObject.SetActive(false);

            // 초기화는 terminateAll일 때에만 한다.
            if (terminateAll) ScenarioManager.singleTon.SetDefault();

            // Initialize
            DataManager dataManager = GetComponent<DataManager>();
            // dataManager.LoadDataFromDB();
        }

        isPlacingMode = !isPlacingMode;
    }

    NodeManager selectedNode = null;
    private void OnNodeSelected(NodeManager node)
    {
        Debug.Log("Node is clicked : " + node.PhysicalID);
        selectedNode = node;
        nodeID.text = node.PhysicalID;

        bool isFireSensor = node is NodeFireSensor;

        fireInfos.gameObject.SetActive(isFireSensor);

        if (!isFireSensor) nodeType.text = GetSensorTypeString(node.DisplayName);
        else
        {
            NodeFireSensor nodeFireSensor = (NodeFireSensor)node;

            nodeType.text = Const.NODE_SENSOR_TEMP + "(온도)";
            fireInfos.GetChild(0).GetChild(1).GetComponent<InputField>().text = Const.NODE_SENSOR_FIRE + "(불꽃)";
            fireInfos.GetChild(2).GetChild(1).GetComponent<InputField>().text = Const.NODE_SENSOR_SMOKE + "(연무)";

            UpdateNodeInfoWindow();
        }

        MQTTManager.OnNodeUpdated -= OnNodeUpdated; // Remove exsiting callback to prevent duplicated call
        MQTTManager.OnNodeUpdated += OnNodeUpdated;
        WindowManager nodeInfoWindow = WindowManager.GetWindow("window_node_info");
        nodeInfoWindow.SetVisible(true);
    }

    // Called when node updated with MQTT data.
    private void OnNodeUpdated(MQTTManager.MQTTMsgData data)
    {
        if (selectedNode == null) return;
        if (!selectedNode.PhysicalID.Equals(selectedNode.PhysicalID)) return;
        UpdateNodeInfoWindow();
    }

    private void UpdateNodeInfoWindow()
    {
        if (selectedNode is NodeFireSensor)
        {
            NodeFireSensor nodeFireSensor = (NodeFireSensor)selectedNode;

            nodeValue.text = nodeFireSensor.ValueTemp.ToString();
            fireInfos.GetChild(1).GetChild(1).GetComponentInChildren<Text>().text = nodeFireSensor.ValueFire.ToString();
            fireInfos.GetChild(3).GetChild(1).GetComponentInChildren<Text>().text = nodeFireSensor.ValueSmoke.ToString();
        }
    }

    private string GetSensorTypeString(string displayName)
    {
        string[] temp = displayName.Split(':');
        string nodeType = temp[0];

        //sensorWindow.cs 기준
        switch (nodeType)
        {
            case "화재 센서":
                return nodeType;
            case "방향지시등":
                return Const.NODE_DIRECTION + "(방향지시등)";
            case "수재해 센서":
                return Const.NODE_SENSOR_FLOOD + "(수재해)";
            case "지진 센서":
                return Const.NODE_SENSOR_EARTHQUAKE + "(지진)";
            default:
                return nodeType;
        }
    }

    private void SetModeButtonColor(bool isPlacingMode)
    {
        if (isPlacingMode)
        {
            placingBtn.interactable = false;
            moniteringBtn.interactable = true;

            placingBtnImage.color = new Color(27 / 255f, 103 / 255f, 255 / 255f);
            moniteringBtnImage.color = new Color(157 / 255f, 157 / 255f, 157 / 255f);

            placingBtnCG.alpha = 1.0f;
            moniteringBtnCG.alpha = 0.67f;
        }
        else
        {
            placingBtn.interactable = true;
            moniteringBtn.interactable = false;

            placingBtnImage.color = new Color(157 / 255f, 157 / 255f, 157 / 255f);
            moniteringBtnImage.color = new Color(27 / 255f, 103 / 255f, 255 / 255f);

            placingBtnCG.alpha = 0.67f;
            moniteringBtnCG.alpha = 1.0f;
        }
    }
}