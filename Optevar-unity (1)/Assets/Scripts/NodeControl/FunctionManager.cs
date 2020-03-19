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
    public static string BuildingName;

    // Popup associated values
    private static Vector3 POPUP_SHOW = new Vector2(0, -100);
    private static Vector3 POPUP_HIDE = new Vector2(0, 200);
    private GameObject popup;                   // Popup message box
    private RectTransform popupTransform;       // Transform
    private static Text popupText;              // Text
    private static float popupLifetime = 0;     // Popup lifetime. hide if 0
    public static float POPUP_DURATION = 5;     // Popup default lifetime.

    // Dictionary of uis
    public Canvas canvas;
    private static Dictionary<string, Transform> uis = new Dictionary<string, Transform>();
    private List<GameObject> floorButtons = new List<GameObject>();

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

    // Start is called before the first frame update
    void Start()
    {
        // Initialize UI.
        RecursiveRegisterChild(canvas.transform, uis);
        // Now ui can be accessed with it's name.

        // Initialize popup
        popup = uis["popup"].gameObject;
        popupTransform = popup.GetComponent<RectTransform>();
        popupText = popupTransform.GetChild(0).GetComponent<Text>();
        popupTransform.anchoredPosition = POPUP_HIDE;

        // Set building name
        uis["text_building_name"].GetComponent<Text>().text = BuildingName;

        // Initialize mouse
        MouseManager.ToNormalMode();

        // Remove all existing nodes
        NodeManager.DestroyAll();

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
                GameObject floorButton = uis["button_floor"].gameObject;
                Transform plainFloors = uis["panel_floor"];

                // Create floor buttons
                GameObject newButton;
                for (int i = 0; i < BuildingManager.FloorsCount; i++)
                {
                    newButton = Instantiate(floorButton);
                    newButton.transform.SetParent(plainFloors, false);
                    newButton.transform.localPosition = Vector3.zero;
                    newButton.transform.GetChild(0).GetComponent<Text>().text = "F" + (i + 1);

                    // var i must be copied, or always it would be FloorsCount-1 as it is used from inline functions that share context.
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
                uis["scrollview_floors"].gameObject.SetActive(false);
            }
        }

        // Load node data from database
        // 이 코드 대신에 DB에서 로드하는 코드가 들어가야만 한다.
        // 만약 노드가 제대로 초기화되지 않았다면, 먼저 deactivate한 후 버튼 색을 다르게 표시해볼 수 있다.
        foreach (string nodeString in NodeManager.__TEST__GetTestNodes(5))
        {
            NodeManager.Instantiate(nodeString);
        }

        // Initialize buttons and create existing node
        GameObject sensorButton = uis["button_sensor_ID"].gameObject;
        Transform sensorPanel = uis["panel_sensor"];
        GameObject newSensorBtn;
        foreach (string physicalID in NodeManager.GetNodeNames())
        {
            Debug.Log(physicalID);
            newSensorBtn = Instantiate(sensorButton);
            newSensorBtn.transform.SetParent(sensorPanel, false);
            newSensorBtn.transform.localPosition = Vector3.zero;
            newSensorBtn.transform.GetChild(0).GetComponent<Text>().text = physicalID;
            newSensorBtn.GetComponent<Button>().onClick.AddListener(() => OnSensorSelected(physicalID));
        }

        Destroy(sensorButton);
    }

    // floor starts from 0. 1st floor = 0
    void OnSetFloor(int floor)
    {
        for (int i = 0; i < BuildingManager.FloorsCount; i++)
        {
            if (i <= floor) BuildingManager.Floors[i].SetActive(true);
            else BuildingManager.Floors[i].SetActive(false);

            if (i != floor) floorButtons[i].GetComponent<Image>().color = new Color(200, 200, 200);
            else floorButtons[i].GetComponent<Image>().color = Color.white;
        }
    }


    // Recursively stored for later access from the dictionary.
    public static void RecursiveRegisterChild(Transform parent, Dictionary<string, Transform> dict)
    {
        if (!dict.ContainsKey(parent.name)) dict.Add(parent.name, parent);
        foreach (Transform child in parent) RecursiveRegisterChild(child, dict);
    }

    // Update is called once per frame
    float t;
    void Update()
    {
        UpdatePopup();
        UpdateDateAndTime();
    }

    // Show popup message
    public static void Popup(string text)
    {
        if (popupLifetime > 0) popupLifetime = POPUP_DURATION - 1.0f;
        else popupLifetime = POPUP_DURATION;        // Set lifetime to 5 sec
        popupText.text = text;                      // Set text fo popup message
    }

    // Update date and time Text
    private void UpdateDateAndTime()
    {
        // 매 프레임마다 이런 함수를 호출하면  심할 수 있다. 코루틴을 사용하여 고쳐봅시다.
        // 스레드도 괜찮다.
        System.DateTime dateTime = System.DateTime.Now;
        dateText.text =
            dateTime.ToString("yyyy") + "/" + dateTime.ToString("MM") + "/" + dateTime.ToString("dd");
        timeText.text = dateTime.ToString("hh시 mm분 ss초");
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

    //=======[ Callback functions ]=========================================================

    // Called when sensor create button clicked
    public void OnCreateSensor()
    {
        MouseManager.ToNormalMode();
        WindowManager sensorWindow = WindowManager.GetWindow("window_sensor");
        sensorWindow.SetVisible(true);
    }

    public void OnSensorSelected(string nodeID)
    {
        NodeManager sensor = NodeManager.GetNodeByName(nodeID);
        Debug.Log(nodeID);
    }

    public void OnModeChange()
    {
        WindowManager.CloseAll();

        if (IsPlacingMode)
        {
            // Placing mode to monitoring mode
            uis["text_mode"].GetComponent<Text>().text = "모니터링 모드";
            uis["layout_buttons"].gameObject.SetActive(false);
        }
        else
        {
            // Monitoring mode to placing mode
            uis["text_mode"].GetComponent<Text>().text = "배치 모드";
            uis["layout_buttons"].gameObject.SetActive(true);
        }

        isPlacingMode = !isPlacingMode;
    }
}