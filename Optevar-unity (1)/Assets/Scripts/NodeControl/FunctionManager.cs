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
    public static string BuildingName;

    // Popup associated values
    private static Vector3 POPUP_SHOW = new Vector2(0, -100);
    private static Vector3 POPUP_HIDE = new Vector2(0, 200);
    private GameObject popup;                   // Popup message box
    private RectTransform popupTransform;       // Transform
    private static Text popupText;              // Text
    private static float popupLifetime = 0;     // Popup lifetime. hide if 0
    public static float POPUP_DURATION = 5;

    // Dictionary of uis
    public Canvas canvas;
    private static Dictionary<string, Transform> uis = new Dictionary<string, Transform>();
    private List<GameObject> floorButtons = new List<GameObject>();

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

                    // I must be copied, or always it would be FloorsCount-1.
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

    public static void RecursiveRegisterChild(Transform parent, Dictionary<string, Transform> dict)
    {
        if (!dict.ContainsKey(parent.name)) dict.Add(parent.name, parent);
        foreach (Transform child in parent) RecursiveRegisterChild(child, dict);
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
        else popupLifetime = POPUP_DURATION;        // Set lifetime to 5 sec
        popupText.text = text;                      // Set text fo popup message
    }

    //=======[ Callback functions ]=========================================================

    // Called when sensor create button clicked
    public void OnCreateSensor()
    {
        MouseManager.ToNormalMode();
        WindowManager sensorWindow = WindowManager.GetWindow("window_sensor");
        sensorWindow.SetVisible(true);
    }
}
