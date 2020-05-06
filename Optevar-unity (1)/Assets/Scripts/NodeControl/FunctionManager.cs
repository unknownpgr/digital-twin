using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class FunctionManager : MonoBehaviour
{
    // Building information
    public static string BuildingPath;
    public static string BuildingName = "다층건물"; // Set default building to ETRI

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

        dtPanel = Find("date_and_time");
        dateText = dtPanel.GetChild(0).GetComponent<Text>();
        timeText = dtPanel.GetChild(1).GetComponent<Text>();

        // Start clock
        StartCoroutine(UpdateDateAndTime());

        // Add callback listener
        MouseManager.OnNodeClicked += OnNodeSelected;
    }

    // floor starts from 0. 1st floor = 0
    private void OnSetFloor(int floor)
    {
        for (int i = 0; i < BuildingManager.FloorsCount; i++)
        {
            if (i <= floor) BuildingManager.Floors[i].SetActive(true);
            else BuildingManager.Floors[i].SetActive(false);

            if (BuildingManager.FloorsCount - i - 1 > floor) floorButtons[i].GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
            else floorButtons[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
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

    public void OnModeChange()
    {
        WindowManager.CloseAll();

        if (IsPlacingMode)
        {
            // Placing mode to monitoring mode
            Find("text_mode").GetComponent<Text>().text = "배치 모드";
            Find("layout_buttons").gameObject.SetActive(false);
            OnSetFloor(BuildingManager.FloorsCount - 1);
            ScenarioManager.singleTon.Init();
        }
        else
        {
            // Monitoring mode to placing mode
            Find("text_mode").GetComponent<Text>().text = "모니터링 모드";
            Find("layout_buttons").gameObject.SetActive(true);
            Find("warning_box").gameObject.SetActive(false);
            ScenarioManager.singleTon.SetDefault();

            // Initialize
            DataManager dataManager = GetComponent<DataManager>();
            dataManager.LoadDataFromDB();
        }

        isPlacingMode = !isPlacingMode;
    }

    private void OnNodeSelected(NodeManager node)
    {
        Debug.Log("Node is clicked : " + node.PhysicalID);
    }
}