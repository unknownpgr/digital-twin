using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class disaster_buttons_action : MonoBehaviour
{

    GameObject fire_button;
    GameObject earthquake_button;
    GameObject flood_button;
    public int disaster_num = -1;
    public GameObject disaster_tab;
    /*
     1 : fire
     2 : earthquake
     3 : flood
         */
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void on_button_click() {
        
        if (this.gameObject.name == "fire")
        {
            disaster_num = 1;
            
        }
        else if (this.gameObject.name == "earthquake")
        {
            disaster_num = 2;
        }
        else if (this.gameObject.name == "flood") {
            disaster_num = 3;
        }
        SceneManager.LoadScene("main");
        
    }
    void unactive_disas_panel() {
        Transform[] disaster_tab_child = disaster_tab.gameObject.GetComponentsInChildren<Transform>();
        for (int x = 0; x < disaster_tab_child.Length; x++) {
            disaster_tab_child[x].gameObject.SetActive(false);
        }
    }
    
}
