using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class FunctionManager : MonoBehaviour
{
    // Popup associated values
    private static Vector3 POPUP_SHOW = new Vector2(0, -100);
    private static Vector3 POPUP_HIDE = new Vector2(0, 200);
    public GameObject popup;                    // Popup message box
    private RectTransform popupTransform;       // Transform
    private static Text popupText;              // Text
    private static float popupLifetime = 0;           // Popup lifetime. hide if 0
    public static float POPUP_DURATION = 5;

    // Buttons on top bar
    public GameObject ButtonBuilding;
    public GameObject ButtonNode;

    // Start is called before the first frame update
    void Start()
    {
        popupTransform = popup.GetComponent<RectTransform>();
        popupText = popupTransform.GetChild(0).GetComponent<Text>();
        popupTransform.anchoredPosition = POPUP_HIDE;
    }

    // Update is called once per frame
    float t;
    void Update()
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

    // Show popup message
    public static void Popup(string text)
    {
        if (popupLifetime > 0) popupLifetime = POPUP_DURATION - 1.0f;
        else popupLifetime = POPUP_DURATION;                   // Set lifetime to 5 sec
        popupText.text = text;                  // Set text fo popup message
    }

    //=======[ Callback functions ]=========================================================

    // Mode selection buttons on top bar
    public void ButtonModeSelection(int mode)
    {
        MouseManager.ToNormalMode();

        // Placing mode
        if (mode == 0)
        {
            ButtonBuilding.SetActive(true);
            ButtonNode.SetActive(true);
        }
        // Monitoring mode
        else
        {
            ButtonBuilding.SetActive(false);
            ButtonNode.SetActive(false);
        }
    }

    // Object placing button on top bar
    public void ButtonObjectPlacing(Dropdown dropDown)
    {
        MouseManager.ToNormalMode();

        int buttonIndex = dropDown.value;
        dropDown.value = 0;
        switch (buttonIndex)
        {
            // Do nothing
            case 0:
                break;

            // Create sensor - show sensor window
            case 1:
                WindowManager sensorWindow = WindowManager.GetWindow("window_sensor");
                sensorWindow.SetVisible(true);
                break;

            // Create area
            case 2:
                break;

            // Create exit sign
            case 3:
                break;

            // Load
            case 4:
                break;

            // Save
            case 5:
                break;

            // DB Load
            case 6:
                break;

            // DB Save
            case 7:
                break;

            // Initialize
            case 8:
                break;

            // Default
            default:
                Debug.Log("Unregistered button");
                break;
        }
    }

    // Sensor mode selection button on sensor window
    public void ButtonSensorMode(int mode)
    {
        MouseManager.ToNormalMode();
        NodeManager newNode;
        switch (mode)
        {
            // Fire
            case 0:
                Popup("Fire sensor");
                newNode = NodeManager.GetNode(NodeManager.NodeType.SENSOR_FIRE);
                break;

            // Earthquake - Not important
            case 1:
                Popup("This function is not implemented yet.");
                break;

            // Flood - Not important
            case 2:
                Popup("This function is not implemented yet.");
                break;

            // Direction
            case 3:
                Popup("Direction sign");
                newNode = NodeManager.GetNode(NodeManager.NodeType.SIGN_DIRECTION);
                break;
        }
        if (!newNode) Popup("Cannot creaet node.");
        else MouseManager.NodePlace(newNode);
        WindowManager.CloseAll();
    }

    public void ButtonLoadSkpFiles(Dropdown dropdown)
    {
        MouseManager.ToNormalMode();

        List<string> skpFiles = new List<string>();
        skpFiles.Add("Default");

        DirectoryInfo folder = new DirectoryInfo(Application.dataPath + "/Resources/Models/");
        foreach (FileInfo file in folder.GetFiles())
        {
            if (file.Extension == ".skp") skpFiles.Add(file.Name.Replace(".skp", ""));
        }
        dropdown.options.Clear();
        dropdown.AddOptions(skpFiles);
    }

    public void ButtonLoadBuilding(Dropdown dropdown)
    {
        GameObject building = BuildingManager.LoadSkp(dropdown.options[dropdown.value].text);
        // batch_panel.transform.Find("multi_floor")
        // .Find("FloorBtns")
        // .GetComponent<SideGUI>()
        // .InitBuildingInfo(loadBuilding.LoadSkp(drops.options[drops.value].text));
        if (!building) Popup("That is not a valid building.");
    }
}
