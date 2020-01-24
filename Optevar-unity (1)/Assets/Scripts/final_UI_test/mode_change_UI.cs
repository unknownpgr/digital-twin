using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class mode_change_UI : MonoBehaviour
{
    GameObject path;
    Transform[] path_children;
    UI_control ui_cont;
    object_button object_button;
    win_close pwc = new win_close();
    objects_batch pb;
    Scenario scene;
    ScenarioManager3 sm;
    // Start is called before the first frame update
    private void Awake()
    {
        ui_cont = GameObject.Find("Canvas").GetComponent<UI_control>();
        object_button = GameObject.Find("all_objects").GetComponent<object_button>();//mode_num
       
        path = ui_cont.path;
        path_children = ui_cont.path.GetComponentsInChildren<Transform>();
        
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void on_clicked_batch_button()
    {
        object_button.mode_num = 0;
        pb = new objects_batch();

        Transform[] simulation_UI_copy = ui_cont.simulation_UI;
        Transform[] path_children_copy = path_children;
        
        for (int x = 0; x < ui_cont.simulation_UI.Length; x++)
        {
            if (simulation_UI_copy[x] != null)
            {
                sm = ui_cont.path.GetComponent<ScenarioManager3>();
                sm.SetDefault();
                simulation_UI_copy[x].gameObject.SetActive(false);
            }
            
        }
       

        for (int x = 0; x < ui_cont.batch_UI.Length; x++)
        {
            if (ui_cont.batch_UI[x] != null)
            {
                
                if (ui_cont.batch_UI[x].name != "person_window"&&
                    ui_cont.batch_UI[x].name != "save_window"&&
                    ui_cont.batch_UI[x].name != "load_window"&&
                    ui_cont.batch_UI[x].name != "sensor_window"
                    ) {

                    ui_cont.batch_UI[x].gameObject.SetActive(true);

                }
            }
        }
        for (int x = 0; x < path_children.Length; x++)
        {
            if (path_children_copy[x] != null)
            {
                path_children_copy[x].gameObject.SetActive(false);
                
            }
           
        }
        


    }
    public void on_clicked_simulation_button()
    {
        if (ui_cont.building == null) ui_cont.building = GameObject.Find("Building");

        if (ui_cont.building == null)
        {
            Debug.Log("Building is not loaded.");
            return;
        }
        Transform[] batch_UI_copy = ui_cont.batch_UI;
        Transform[] path_children_copy = path_children;
        if (GameObject.Find("objects_button")) {
            pb = GameObject.Find("objects_button").GetComponent<objects_batch>();
            if (pb.isActiveAndEnabled)
            {
                this.scene = pb.get_scenario();
            }
        }
        for (int x = 0; x < ui_cont.batch_UI.Length; x++)
        {
            if (batch_UI_copy[x] != null)
            {
                batch_UI_copy[x].gameObject.SetActive(false);
            }
        }
        
        for (int x = 0; x < ui_cont.simulation_UI.Length; x++)
        {
            if (ui_cont.simulation_UI[x] != null)
            {
                ui_cont.simulation_UI[x].gameObject.SetActive(true);
            }
        }
        for (int x = 0; x < path_children.Length; x++)
        {
            if (path_children_copy[x] != null)
            {
                path_children_copy[x].gameObject.SetActive(true);
            }
        }
        object_button.mode_num = 0;
        sm = ui_cont.path.GetComponent<ScenarioManager3>();
        sm.Initiation();
        sm.image_panel.SetActive(false);
        sm.SetLists(this.scene, pb.GetSensorObjects());
        sm.InitMoniteringMode();
    }
    
    public void active_disas_panel()
    {
        SceneManager.LoadScene("choose_disaster");
    }
}
