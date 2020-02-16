using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class object_button : MonoBehaviour
{
    GameObject new_object;
    public int mode_num = 0;//모든 class에서 받아가기(scene상의 빈 gameobject에다가 붙여놓기)
    SideGUI SideGUI;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.Find("FloorBtns"))
        {
            SideGUI = GameObject.Find("FloorBtns").GetComponent<SideGUI>();
            SideGUI.InitBuildingInfo();// this.building);
            //SideGUI.HideFloor(sm.LastUpdatedFloor);
        }
    }

    public void mk_new_object(GameObject ori_object, string object_tag, Vector3 mouse_pos)
    {
        if (mode_num == 1)
        {
            new_object = Instantiate(ori_object, new Vector3(9, 2, -10), Quaternion.identity);
            new_object.tag = object_tag;
            new_object.transform.position = new Vector3(mouse_pos.x, 2.5f, mouse_pos.z);
        }
    }
}
