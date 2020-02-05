using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class objects_batch : MonoBehaviour
{
    private object_button ob_button;//mode_num받아옴   
    GameObject DBManager;
    public Transform ob_dropdown;
    int ob_index;


    int window_chilren_size;
    GameObject save_file_win;
    GameObject load_file_win;
    GameObject sensor_window;
    Transform[] save_win_children;
    Transform[] load_win_children;

    make_objects make_objects;
    //json다루기 위한 변수들
    JsonParser jp = new JsonParser();
    NodePositions json_file;

    GameObject ori_person;//4가지 종류 있다 person, sensor, fire, exit(각자 받아와야함)
    GameObject new_person;

    GameObject ori_sensor;
    GameObject new_sensor;
    GameObject ori_sensor_dir;

    GameObject ori_exit;
    GameObject new_exit;
    GameObject new_disaster;
    GameObject ori_area_num;
    GameObject new_area_num;
    private int person_num = 0;
    Scenario total_array = new Scenario();

    public int area_index = 0;

    public InputField save_file_name;
    public Button file_save_button;
    public Button file_load_button;
    localFileTest_4 win_ob;
    GameObject all_objects;


    private void Awake()
    {
        ob_button = GameObject.Find("all_objects").GetComponent<object_button>();
        DBManager = GameObject.Find("DBManager");

    }
    void Start()
    {
        UI_control UIc = GameObject.Find("Canvas").GetComponent<UI_control>();
        save_file_win = UIc.save_file_win.gameObject;
        load_file_win = UIc.load_file_win.gameObject;
        sensor_window = UIc.sensor_window.gameObject;
        save_win_children = save_file_win.gameObject.GetComponentsInChildren<Transform>();
        load_win_children = load_file_win.gameObject.GetComponentsInChildren<Transform>();

        all_objects = GameObject.Find("all_objects");
        make_objects = GameObject.Find("person_attri").GetComponent<make_objects>();
    }


    void Update()
    {

    }

    public void objects_button_click()
    {

        ob_index = ob_dropdown.GetComponent<Dropdown>().value;
        List<Dropdown.OptionData> ob_munu_options = ob_dropdown.GetComponent<Dropdown>().options;
        switch (ob_index)
        { // New senser, New Area, New Exit, Load, Save, DB Load, DB Save, Clear

            case 1:
                close_all_windows();
                sensor_window.SetActive(true);
                ob_button.mode_num = 4;
                ob_dropdown.GetComponent<Dropdown>().value = 0;
                break;
            case 2:
                close_all_windows();
                make_objects.area_positions = new List<AreaPositions>();
                ob_button.mode_num = 7;
                ob_dropdown.GetComponent<Dropdown>().value = 0;
                break;
            case 3:
                close_all_windows();
                ob_button.mode_num = 6;
                ob_dropdown.GetComponent<Dropdown>().value = 0;
                break;
            case 4:
                close_all_windows();
                active_win(load_file_win, load_win_children);
                ob_button.mode_num = 3;
                ob_dropdown.GetComponent<Dropdown>().value = 0;
                file_load_button.onClick.AddListener(load_button_clicked);
                break;
            case 5:
                close_all_windows();
                active_win(save_file_win, save_win_children);
                ob_button.mode_num = 2;
                ob_dropdown.GetComponent<Dropdown>().value = 0;
                file_save_button.onClick.AddListener(save_clicked);
                break;
            case 6:
                // DB Load

                break;
            case 7:
                // DB Save
                Dictionary<string, object> dics = new Dictionary<string, object>();

                for (int i = 0; i < make_objects.sensor_ob.Count; i++)
                    dics.Add(make_objects.sensor_ob[i].GetComponent<sensor_attribute>().one_sensor.nodeId
                        , make_objects.sensor_ob[i].GetComponent<sensor_attribute>().one_sensor);//이게 타입이 SensorNodeJson임
                for (int i = 0; i < make_objects.area_nums.Count; i++)
                {
                    if (dics.ContainsKey(make_objects.area_nums[i].GetComponent<areasensor_attribute>().one_sensor.areaId))
                    {
                        Debug.Log("The node ID have to unique.");
                        return;
                    }
                    dics.Add(make_objects.area_nums[i].GetComponent<areasensor_attribute>().one_sensor.areaId, make_objects.area_nums[i].GetComponent<areasensor_attribute>().one_sensor);
                }
                DBManager dbm = DBManager.GetComponent<DBManager>();
                foreach (string d in dics.Keys)//<string, object>
                {
                    //Debug.Log("센서타입 : " + dics[d].GetType());
                    if (dics[d].GetType() == typeof(SensorNodeJson))// 이거 sensor_attribute로 되어있었음
                        dbm.SensorSave((SensorNodeJson)dics[d]);//타입변환오류

                    else if (dics[d].GetType() == typeof(areasensor_attribute))
                        dbm.SensorSave((areasensor_attribute)dics[d]);
                }
                /*
                foreach(string d in dics.Keys)
                {
                    //Debug.Log("센서타입 : "+d.GetType());
                    if (d.GetType() == typeof(sensor_attribute))// 이거 
                        dbm.SensorSave((sensor_attribute)dics[d]);

                    else if (d.GetType() == typeof(areasensor_attribute))
                        dbm.SensorSave((areasensor_attribute)dics[d]);
                }
                 */
                break;
            case 8:
                ob_dropdown.GetComponent<Dropdown>().value = 0;
                clear();
                break;


        }

    }

    void clear()
    {
        ob_button.mode_num = 0;
        total_array = new Scenario();
        for (int m = 0; m < make_objects.persons_ob.Count; m++)
        {
            Destroy(make_objects.persons_ob[m]);
        }
        make_objects.persons_ob.Clear();
        make_objects.person_list.Clear();
        make_objects.person_id = 0;

        for (int m = 0; m < make_objects.sensor_ob.Count; m++)
        {
            Destroy(make_objects.sensor_ob[m]);
        }
        make_objects.sensor_ob.Clear();
        make_objects.sensor_list.Clear();
        make_objects.sensor_id = 0;
        for (int m = 0; m < make_objects.exit_ob.Count; m++)
        {
            Destroy(make_objects.exit_ob[m]);
        }
        for (int m = 0; m < make_objects.area_nums.Count; m++)
        {
            Destroy(make_objects.area_nums[m]);
        }
        make_objects.exit_ob.Clear();
        make_objects.exit_list.Clear();
        make_objects.exit_id = 0;

        make_objects.area_positions.Clear();
        area_index = 0;

        make_objects.disaster_ob.Clear();
        make_objects.disaster_list.Clear();
        make_objects.disaster_id = 0;

    }
    public Scenario get_scenario()
    {
        total_array = new Scenario();
        if (make_objects.person_list != null)
        {
            total_array.evacuaterNodeJsons = make_objects.person_list.ToArray();
        }
        else
        {
            total_array.evacuaterNodeJsons = null;
        }
        if (make_objects.sensor_list != null)
        {
            total_array.sensorNodeJsons = make_objects.sensor_list.ToArray();
        }
        else
        {
            total_array.sensorNodeJsons = null;
        }
        if (make_objects.disaster_list != null)
        {
            total_array.disasterNodeJsons = make_objects.disaster_list.ToArray();
        }
        else
        {
            total_array.disasterNodeJsons = null;
        }
        if (make_objects.exit_list != null)
        {
            total_array.exitNodeJsons = make_objects.exit_list.ToArray();
        }
        else
        {
            total_array.exitNodeJsons = null;
        }
        if (make_objects.area_positions != null)
        {
            total_array.areaPositionJsons = make_objects.area_positions.ToArray();
        }
        else
        {
            total_array.areaPositionJsons = null;
        }
        return total_array;
    }
    void save_clicked()
    {
        jp.Save3<Scenario>(get_scenario(), save_file_name.text);
        Debug.Log(save_file_name.text);
        Debug.Log("save완료");
        unactive_win(save_file_win, save_win_children);
    }
    void load_button_clicked()
    {

        ob_button = GameObject.Find("all_objects").GetComponent<object_button>();
        win_ob = load_file_win.GetComponent<localFileTest_4>();
        string file_path = win_ob.get_clicked_file_name();
        Debug.Log("file path : " + file_path);

        Scenario json_file = jp.Load3<Scenario>(file_path);
        clear();
        total_array = json_file;

        ori_person = make_objects.ori_person;
        ori_sensor = make_objects.ori_sensor;
        ori_sensor_dir = make_objects.ori_sensor_dir;
        ori_exit = make_objects.ori_exit;
        ori_area_num = make_objects.area_num;

        //person객체 생성하기
        for (int m = 0; m < json_file.evacuaterNodeJsons.Length; m++)//Vector3 object_pos in objects_pos)
        {
            new_person = Instantiate(ori_person, json_file.evacuaterNodeJsons[m].positions, Quaternion.identity);//****************여기에 ori_person이 할당이 안되어있다?
            new_person.transform.SetParent(all_objects.transform);
            new_person.tag = "person1";
            EvacuaterNodeJson temp_person = new EvacuaterNodeJson();
            temp_person = json_file.evacuaterNodeJsons[m];
            temp_person.nodeId = make_objects.person_id;
            make_objects.person_id++;
            make_objects.person_list.Add(temp_person);
            new_person.GetComponent<person_attribute>().one_person = temp_person;

            make_objects.persons_ob.Add(new_person);
        }//센서객체 생성하기
        for (int m = 0; m < json_file.sensorNodeJsons.Length; m++)
        {

            if (json_file.sensorNodeJsons[m].nodeType == 39)
            {
                new_sensor = Instantiate(ori_sensor_dir, json_file.sensorNodeJsons[m].positions, Quaternion.identity);
                new_sensor.tag = "sensor1";
            }
            else
            {
                new_sensor = Instantiate(ori_sensor, json_file.sensorNodeJsons[m].positions, Quaternion.identity);//****************여기에 ori_person이 할당이 안되어있다?
                new_sensor.tag = "sensor1";
            }
            new_sensor.transform.SetParent(all_objects.transform);
            SensorNodeJson temp_sensor = new SensorNodeJson();
            temp_sensor = json_file.sensorNodeJsons[m];

            temp_sensor.nodeId = json_file.sensorNodeJsons[m].nodeId;
            make_objects.sensor_id++;

            new_sensor.GetComponent<sensor_attribute>().one_sensor = temp_sensor;

            if (temp_sensor.disaster)
            {
                new_sensor.GetComponent<Renderer>().material.color = Color.red;
            }
            make_objects.sensor_list.Add(temp_sensor);


            make_objects.sensor_ob.Add(new_sensor);
        }
        //재난이 발생한 곳에 있는 sensor
        /*
        for (int m = 0; m < json_file.disasterNodes.Length; m++)//Vector3 object_pos in objects_pos)
        {
            //new_sensor = Instantiate(ori_sensor, json_file.sensorNodes[m].positions, Quaternion.identity);//****************여기에 ori_person이 할당이 안되어있다?
            //new_sensor.tag = "sensor1";
            DisasterNodeJson temp_disaster = new DisasterNodeJson();
            //age, speed. id, position등 받아옴
            temp_disaster = json_file.disasterNodes[m];

            temp_disaster.nodeId = make_objects.disaster_id;
            make_objects.disaster_id++;

            for (int y = 0; y < json_file.sensorNodes.Length; y++)
            {
                if (temp_disaster.sensoId == make_objects.sensor_list[y].nodeId) {

                }

            }
            //make_objects.person_list.Add(new_person.transform.position);
            make_objects.disaster_list.Add(temp_disaster);
            //new_sensor.GetComponent<person_attribute>().one_person = temp_person; //sensor이거 필요하나??

            //make_objects.senso_ob.Add(new_sensor);
        }*/
        //exit객체 생성하기

        for (int m = 0; m < json_file.exitNodeJsons.Length; m++)
        {
            new_exit = Instantiate(ori_exit, json_file.exitNodeJsons[m].positions, ori_exit.transform.rotation);//****************여기에 ori_person이 할당이 안되어있다?
            new_exit.transform.SetParent(all_objects.transform);
            new_exit.tag = "exit1";
            ExitNodeJson temp_exit = new ExitNodeJson();
            temp_exit = json_file.exitNodeJsons[m];
            temp_exit.nodeId = make_objects.exit_id;
            make_objects.exit_id++;
            make_objects.exit_list.Add(temp_exit);
            make_objects.exit_ob.Add(new_exit);
        }
        //area_num생성하기
        for (int m = 0; m < json_file.areaPositionJsons.Length; m++)
        {
            make_objects.area_positions.Add(json_file.areaPositionJsons[m]);

            new_area_num = Instantiate(ori_area_num, json_file.areaPositionJsons[m].position, ori_area_num.transform.rotation);
            new_area_num.GetComponent<areasensor_attribute>().one_sensor = json_file.areaPositionJsons[m];
            new_area_num.transform.position = new Vector3(new_area_num.transform.position.x, new_area_num.transform.position.y + 0.1f, new_area_num.transform.position.z);
            new_area_num.transform.SetParent(all_objects.transform);
            new_area_num.GetComponent<TextMesh>().text = "Room" + json_file.areaPositionJsons[m].areaId;

            new_area_num.name = "area_num1";
            new_area_num.tag = "area1";
            area_index++;
            make_objects.area_nums.Add(new_area_num);
        }
        unactive_win(load_file_win, load_win_children);

    }

    public void unactive_win(GameObject window, Transform[] win_children)
    {
        Transform[] window_copy = win_children;
        window_chilren_size = win_children.Length;
        for (int y = 0; y < window_chilren_size; y++)
        {
            window_copy[y].gameObject.SetActive(false);
        }
        ob_button.mode_num = 0;

    }
    public void active_win(GameObject window, Transform[] win_children)
    {

        Transform[] window_copy = win_children;
        window_chilren_size = win_children.Length;
        for (int y = 0; y < window_chilren_size; y++)
        {
            window_copy[y].gameObject.SetActive(true);
        }
        if (window.name == "load_window")
        {
            localFileTest_4 win_ob = load_file_win.GetComponent<localFileTest_4>();
            win_ob.GetPathList();
        }
    }
    public void close_all_windows()
    {
        /*
        if (person_window.activeSelf)
        {
            unactive_win(person_window, persons_children);
        }*/
        if (sensor_window.activeSelf)
        {
            sensor_window.SetActive(false);
        }
        if (save_file_win.activeSelf)//.activeInHierarchy)
        {
            unactive_win(save_file_win, save_win_children);
        }
        if (load_file_win.activeSelf)
        {
            unactive_win(load_file_win, load_win_children);
        }
    }
    public List<GameObject> GetSensorObjects()
    {
        return make_objects.sensor_ob;
    }
}
