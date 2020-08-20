using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;

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

    //=============================================================
    // Pre-loaded UI
    //=============================================================

    // Elements of information window
    Transform infoWindowPanelParent;

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
    private Text placingBtnText;
    private Text moniteringBtnText;

    // Transform of menu
    private Transform menu;
    private RectTransform menuRectTransform;
    // GameObject of open menu button
    private GameObject openMenuButton;
    // GameObject of hide menu
    private GameObject hiddenMenu;

    // Transform of parent of menu buttons
    Transform menuButtonParent;
    // Array of menu button images
    private Image[] menuButtonImages;

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
                    newButton.GetComponent<Button>().onClick.AddListener(() => { SetFloorVisibility(k); });
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
            SetFloorVisibility(BuildingManager.FloorsCount - 1);
        }

        // Get transform of information window
        infoWindowPanelParent = Find("window_information").GetChild(3);

        // Get elements of node information window
        nodeInfos = Find("window_node_info").GetChild(1);
        nodeID = nodeInfos.GetChild(0).GetChild(1).GetComponent<InputField>();
        nodeType = nodeInfos.GetChild(1).GetChild(1).GetComponent<InputField>();
        nodeValue = nodeInfos.GetChild(2).GetChild(1).GetComponentInChildren<Text>();
        fireInfos = nodeInfos.GetChild(3);

        // Getelements of mode change button
        Transform modeBtnTransform = Find("button_mode");
        GameObject placingPart = modeBtnTransform.GetChild(0).gameObject;
        GameObject moniteringPart = modeBtnTransform.GetChild(1).gameObject;
        placingBtn = placingPart.GetComponent<Button>();
        moniteringBtn = moniteringPart.GetComponent<Button>();
        placingBtnImage = placingPart.GetComponent<Image>();
        moniteringBtnImage = moniteringPart.GetComponent<Image>();
        placingBtnText = placingPart.GetComponentInChildren<Text>();
        moniteringBtnText = moniteringPart.GetComponentInChildren<Text>();        

        // Set mode change button color
        SetModeButtonColor(IsPlacingMode);

        // Get transform of menu
        menu = Find("menu");
        menuRectTransform = menu.GetComponent<RectTransform>();
        openMenuButton = menu.GetChild(0).gameObject;
        hiddenMenu = menu.GetChild(1).gameObject;

        // Set state of button about menu
        openMenuButton.SetActive(true);
        hiddenMenu.SetActive(false);

        // Get transform of parent of menu buttons 
        menuButtonParent = Find("layout_buttons");
        // Get all image of menu buttons
        menuButtonImages = menuButtonParent.GetComponentsInChildren<Image>();

        // Add callback listener
        MouseManager.OnNodeClicked -= OnNodeSelected; // Remove exsiting callback to prevent duplicated call
        MouseManager.OnNodeClicked += OnNodeSelected;

        // Take a screenshot of the building
        ScreenshotManager.ScreenShot(this, texture =>
         {
             byte[] _bytes = texture.EncodeToPNG();
             File.WriteAllBytes(Application.dataPath + "/Textures/" + BuildingName + ".png", _bytes);
         });
    }

    // floor starts from 0. 1st floor = 0
    public static void SetFloorVisibility(int floor)
    {
        // If given building has only one floor, ignore floor setting.
        if (BuildingManager.FloorsCount == 1) return;

        for (int i = 0; i < BuildingManager.FloorsCount; i++)
        {
            if (i <= floor) BuildingManager.Floors[i].SetVisible(true);
            else BuildingManager.Floors[i].SetVisible(false);

            if (((BuildingManager.FloorsCount - floor) - 1) == i)
            {
                // #3036aa
                self.floorButtons[i].GetComponent<Image>().color = new Color(0.1882353f, 0.2117647f, 0.6666667f, 0.84f);
            }
            else
            {
                self.floorButtons[i].GetComponent<Image>().color = new Color(0, 0, 0, 0.84f);
            }
            // if (BuildingManager.FloorsCount - i - 1 > floor) self.floorButtons[i].GetComponent<Image>().color = new Color(0.1882353f, 0.2117647f, 0.6666667f, 0.84f);
            // else self.floorButtons[i].GetComponent<Image>().color = new Color(0, 0, 0, 0.84f);
        }

        foreach (NodeManager node in NodeManager.GetAll())
        {
            node.Hide = BuildingManager.GetFloor(node.Position) > floor;
        }
    }

    private static void RecursiveRegisterChild(Transform parent, Dictionary<string, Transform> dict)
    {
        if (!dict.ContainsKey(parent.name)) dict.Add(parent.name, parent);
        foreach (Transform child in parent) RecursiveRegisterChild(child, dict);
    }

    private Vector2 MENU_SHOW = new Vector2(0.0f, -61.7f);
    private Vector2 MENU_HIDE = new Vector2(-310, -61.7f);
    private bool isMenuHidden;
    // Move menu and Show hidden menu 
    // or Move menu and Hide hidden menu
    private void MoveMenu(bool isMenuHidden)
    {
        // If menu is hidden
        if (isMenuHidden == true)
        {
            // then move menu to show hidden menu
            menuRectTransform.anchoredPosition = MENU_SHOW;
            isMenuHidden = false;
        }
        // If menu is shown
        else if (isMenuHidden == false)
        {
            // then move menu to hide hidden menu
            menuRectTransform.anchoredPosition = MENU_HIDE;
            isMenuHidden = true;
        }
    }


    //=======[ Callback functions ]=========================================================

    // Called when sensor create button clicked

    public void OnOpenMenu()
    {
        // Unactivate open menu Button
        // and Activate hidden menu 
        openMenuButton.SetActive(false);
        hiddenMenu.SetActive(true);

        // Move menu to show
        MoveMenu(true);
    }

    public void OnCloseMenu()
    {
        // Activate open menu Button
        // and unactivate hidden menu 
        openMenuButton.SetActive(true);
        hiddenMenu.SetActive(false);

        // Move menu to hide
        MoveMenu(false);

    }

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

    public void OnClickInformationIcon()
    {
        // Move transform of building information panel to the end
        // to be seen first when information window popped out
        infoWindowPanelParent.Find("panel_building_info").SetAsLastSibling();

        WindowManager informationWindow = WindowManager.GetWindow("window_information");
        informationWindow.SetVisible(true);
    }

    public void OnClickInformationWindowMenu()
    {
        // Transform of panel about clicked button
        Transform selectedPanelTransform;
        // Name of clicked button
        string buttonName = EventSystem.current.currentSelectedGameObject.name;
        /*
         * button name : button_****
         * panel name : panel_****
         */
        string[] parsed = buttonName.Split('_');
        string panelName = "panel";

        // Make panel name with parsed button name
        for (int i=1;i<parsed.Length;i++)
        {
            string temp = "_" + parsed[i];
            panelName += temp;
        }

        // Find panel with panel name using parent transform
        selectedPanelTransform = infoWindowPanelParent.Find(panelName);
        // Set panel transform to the end of the transform list
        selectedPanelTransform.SetAsLastSibling();
    }

    public void OnModeChange()
    {
        // ToDo : 
        // terminateAll은 임시로 설정해둔 변수다.
        // 만약 terminate all이 참이면 싹다 꺼버리고, 아니면 그대로 두고 종료만 한다.

        WindowManager.CloseAll();

        if (IsPlacingMode)
        {
            // Placing mode to monitoring mode

            // Set mode change button color
            SetModeButtonColor(!IsPlacingMode);
            
            // Unactiavate menu
            if (isMenuHidden == false)
            {
                OnCloseMenu();
            }
            menu.gameObject.SetActive(false);

            // Show all floors
            SetFloorVisibility(BuildingManager.FloorsCount - 1);
            ScenarioManager.singleTon.Init();

            // Show graph
            WindowManager graphManager = WindowManager.GetWindow("window_graph");
            graphManager.SetVisible(true);
        }
        else
        {

            // Monitoring mode to placing mode

            // Set mode change button color
            SetModeButtonColor(!IsPlacingMode);

            // Activate menu
            menu.gameObject.SetActive(true);

            Find("warning_box").gameObject.SetActive(false);

            ScenarioManager.singleTon.EndSimulation();

            // Initialize
            // DataManager dataManager = GetComponent<DataManager>();
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

            nodeType.text = Constants.NODE_SENSOR_TEMP + "(온도)";
            fireInfos.GetChild(0).GetChild(1).GetComponent<InputField>().text = Constants.NODE_SENSOR_FIRE + "(불꽃)";
            fireInfos.GetChild(2).GetChild(1).GetComponent<InputField>().text = Constants.NODE_SENSOR_SMOKE + "(연무)";

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
        if (selectedNode is NodeFireSensor nodeFireSensor)
        {
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
                return Constants.NODE_DIRECTION + "(방향지시등)";
            case "수재해 센서":
                return Constants.NODE_SENSOR_FLOOD + "(수재해)";
            case "지진 센서":
                return Constants.NODE_SENSOR_EARTHQUAKE + "(지진)";
            default:
                return nodeType;
        }
    }

    // When mode change button clicked
    // Set clicked button color to #3036aa, another to #FFFFFF
    private void SetModeButtonColor(bool isPlacingMode)
    {
        if (isPlacingMode)
        {
            placingBtn.interactable = false;
            moniteringBtn.interactable = true;

            placingBtnImage.color = new Color(0.1882353f, 0.2117647f, 0.6666667f, 0.84f);
            moniteringBtnImage.color = new Color(0f, 0f, 0f, 0.84f);

            placingBtnText.color = new Color(1, 1, 1);
            moniteringBtnText.color = new Color(1, 1, 1, 0.7f);
            
        }
        else
        {
            placingBtn.interactable = true;
            moniteringBtn.interactable = false;

            placingBtnImage.color = new Color(0f, 0f, 0f, 0.84f);
            moniteringBtnImage.color = new Color(0.1882353f, 0.2117647f, 0.6666667f, 0.84f);

            placingBtnText.color = new Color(1, 1, 1, 0.7f);
            moniteringBtnText.color = new Color(1, 1, 1);
        }
    }

    // Close system
    public void CloseApp()
    {
        Application.Quit();
    }

    // When certain button of menu clicked,
    // Set it's color to #F99774
    // and Set 'A' value of other to zero
    public void SetMenuButtonColors(int index)
    {
        for (int i = 0; i< menuButtonImages.Length; i++)
        {
            if (index == i)
            {
                menuButtonImages[i].color = new Color(0.9764706f, 0.5921569f, 0.454902f, 1f);
            }
            else
            {
                menuButtonImages[i].color = new Color(0f, 0f, 0f, 0f);
            }
        }
    }
}