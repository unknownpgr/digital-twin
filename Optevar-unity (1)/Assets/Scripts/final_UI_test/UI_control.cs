using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class UI_control : MonoBehaviour
{
    public GameObject batch_panel;
    public GameObject simulation_panel;
    public GameObject header;
    public GameObject InformWindows;
    public Transform[] batch_UI;
    Transform[] batch_UI_copy;
    Transform[] header_UI;
    Transform[] batch_button_UI = new Transform[5];
    Transform[] InformWindows_UI;

    public Transform[] simulation_UI;
    public Transform[] simulation_UI_copy;
    public Transform[] path_children;
    private Transform[] path_children_copy;
    public GameObject path;
    public GameObject building;
    
    bool isLoaded = false;
    bool isSecondFrame = false;
    

    public Transform person_window;
    public Transform save_file_win;
    public Transform load_file_win;
    public Transform sensor_window;

    LoadBuilding loadBuilding; 
    void Awake()
    {
        loadBuilding = new LoadBuilding();
        batch_UI = batch_panel.gameObject.GetComponentsInChildren<Transform>();
        
        save_file_win = batch_panel.transform.GetChild(0).GetChild(1);
        load_file_win = batch_panel.transform.GetChild(0).GetChild(2);
        sensor_window = batch_panel.transform.GetChild(0).GetChild(3);
        Debug.Log(save_file_win.name + load_file_win.name);

        header_UI = header.gameObject.GetComponentsInChildren<Transform>();
        path_children = path.gameObject.GetComponentsInChildren<Transform>();
        if (building == null) building = GameObject.Find("Building");
        int batch_button_index = 0;
        for (int x = 0; x < header_UI.Length; x++) {
            if (header_UI[x].gameObject.name == "building_button" ||
               header_UI[x].gameObject.name == "objects_button" ||
               header_UI[x].gameObject.name == "question_button"
               ) {

                batch_button_UI[batch_button_index] = header_UI[x];
                batch_button_index++;
            }
        }
        
        Dropdown drops = GameObject.Find("building_button").GetComponent<Dropdown>();
        drops.ClearOptions();
        List<string> tmpStrs = new List<string>();
        DirectoryInfo folder = new DirectoryInfo(Application.dataPath + "/Resources/Models/");
        tmpStrs.Add("Default");

        foreach (var dir in folder.GetFiles())
        {
            if (dir.Extension == ".skp")
            tmpStrs.Add(dir.Name.Replace(".skp", ""));
        }
        drops.AddOptions(tmpStrs);
        drops.onValueChanged.AddListener(delegate
        {
            isLoaded = true;
            
            batch_panel.transform.Find("multi_floor").Find("FloorBtns").GetComponent<SideGUI>().InitBuildingInfo(loadBuilding.LoadSkp(drops.options[drops.value].text));
        });

        //
        simulation_UI = simulation_panel.gameObject.GetComponentsInChildren<Transform>();
        
        int temp_length = batch_UI.Length;
        Array.Resize(ref batch_UI, batch_UI.Length + batch_button_UI.Length);
        Array.Copy(batch_button_UI, 0,
            batch_UI, temp_length,
            batch_button_UI.Length);
        
        batch_UI_copy = batch_UI;
        for (int x = 0; x < batch_UI.Length; x++)
        {
            if (batch_UI_copy[x] != null) {
                
                batch_UI_copy[x].gameObject.SetActive(false);
            }
        }
        simulation_UI_copy = simulation_UI;
        for (int x = 0; x < simulation_UI.Length; x++)
        {
            if (simulation_UI_copy[x] != null)
            {
                simulation_UI_copy[x].gameObject.SetActive(false);
            }
        }
        path_children_copy = path_children;
        for (int x = 0; x < path_children.Length; x++)
        {
            if (path_children_copy[x] != null)
            {
                path_children_copy[x].gameObject.SetActive(false);
            }
        }
        

        InformWindows = GameObject.Find("InformationWindow").gameObject;
        InformWindows_UI = InformWindows.GetComponentsInChildren<Transform>();

    }
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < InformWindows_UI.Length; i++)
            InformWindows_UI[i].gameObject.SetActive(false);
        InformWindows.SetActive(true);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isLoaded)
        {
            if (!isSecondFrame)
            {
                isSecondFrame = true;
                return;
            }
            loadBuilding.SetNavMesh();
            isLoaded = false;
            isSecondFrame = false;
        }
    }

}
