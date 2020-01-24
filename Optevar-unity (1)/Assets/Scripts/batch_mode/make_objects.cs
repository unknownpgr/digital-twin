using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using UnityEngine.EventSystems;
//scene에 새로운 객체를 생성할 때 사용하는 script
public class make_objects : MonoBehaviour
{
    object_button object_button1;//mode_num받아옴
    mode_change_UI mode_cha;
    
    public GameObject ori_person;
    public GameObject ori_sensor;
    public GameObject ori_sensor_dir;
    public GameObject ori_exit;
    public GameObject area_num;
    public int sensorType = 0;

    GameObject new_object;
    GameObject ori_object;
    int set = 0;
    

    public List<GameObject> persons_ob = new List<GameObject>();
    public List<EvacuaterNodeJson> person_list = new List<EvacuaterNodeJson>();
    public int person_id = 0;

    public List<GameObject> sensor_ob = new List<GameObject>();
    public List<SensorNodeJson> sensor_list = new List<SensorNodeJson>();
    public int sensor_id = 0;
    
    public List<GameObject> disaster_ob = new List<GameObject>();
    public List<DisasterNodeJson> disaster_list = new List<DisasterNodeJson>();
    public int disaster_id = 0;
    

    public List<GameObject> exit_ob = new List<GameObject>();
    public List<ExitNodeJson> exit_list = new List<ExitNodeJson>();
    public int exit_id = 0;
    public List<GameObject> area_nums = new List<GameObject>();


    private GameObject chosen_object;
    private Vector3 mouse_pos;
    
    public int final_age;
    public int final_speed;

    public InputField age;
    public InputField speed;
    public InputField number;


    public List<AreaPositions> area_positions = new List<AreaPositions>();
    private AreaPositions new_position;
    private objects_batch pb;
    GameObject all_objects;


    private void Awake()
    {
        object_button1 = GameObject.Find("all_objects").GetComponent<object_button>();

    }
    void Start()
    {
        pb = GameObject.Find("objects_button").GetComponent<objects_batch>();
        all_objects = GameObject.Find("all_objects");
        mode_cha = GameObject.Find("batch_button").GetComponent<mode_change_UI>();
    }

    // Update is called once per frame
    void Update()
        
    {

        
        if (Input.GetMouseButtonDown(0) && object_button1.mode_num == 1)
        {
            Ray cast_point2 = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit2;
            if (Physics.Raycast(cast_point2, out hit2, Mathf.Infinity))
            {
                //input_field관련
                if (age.text != "")
                {//input값이 있을 때
                    final_age = int.Parse(age.text);
                }
                if (speed.text != "")
                {
                    final_speed = int.Parse(speed.text);
                }

                Ray cast_point = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                int layerMask = 1 << LayerMask.NameToLayer("building");//building만해당되어야함
                if (Physics.Raycast(cast_point, out hit, Mathf.Infinity, layerMask))
                {
                    ori_object = ori_person;
                    int temp = 1;
                    if (number.text == "")
                    {
                        temp = 1;
                        new_object = Instantiate(ori_object, new Vector3(9, 2, -10), Quaternion.identity);
                        new_object.transform.SetParent(all_objects.transform);
                        new_object.tag = "person1";
                        new_object.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);

                        if (age.text != "")
                        {

                            new_object.GetComponent<person_attribute>().one_person.nodeAge = final_age;
                        }
                        if (speed.text != "")
                        {
                            new_object.GetComponent<person_attribute>().one_person.nodeSpeed = final_speed;
                        }
                        
                        new_object.GetComponent<person_attribute>().one_person.positions = new_object.transform.position;
                        new_object.GetComponent<person_attribute>().one_person.nodeId = person_id;
                        person_id++;
                        person_list.Add(new_object.GetComponent<person_attribute>().one_person);
                        persons_ob.Add(new_object);
                        
                    }
                    else
                    {
                        temp = int.Parse(number.text);
                        for (int m = 0; m < temp; m++)
                        {
                            new_object = Instantiate(ori_object, new Vector3(9, 2, -10), Quaternion.identity);
                            new_object.transform.SetParent(all_objects.transform);
                            new_object.tag = "person1";
                            if (age.text != "")
                            {
                                new_object.GetComponent<person_attribute>().one_person.nodeAge = final_age;
                            }
                            if (speed.text != "")
                            {
                                new_object.GetComponent<person_attribute>().one_person.nodeSpeed = final_speed;
                            }

                            System.Random rd = new System.Random((int)DateTime.Now.Ticks);
                            System.Random rd2 = new System.Random((int)DateTime.Now.Ticks - 49);
                            double rdn = rd.NextDouble() - 0.5;
                            double rdn2 = rd2.NextDouble() - 0.5;
                            new_object.transform.position = new Vector3(hit.point.x + (float)rdn, hit.point.y, hit.point.z + (float)rdn2);
                           
                            new_object.GetComponent<person_attribute>().one_person.positions = new_object.transform.position;
                            new_object.GetComponent<person_attribute>().one_person.nodeId = person_id;
                            person_id++;
                            person_list.Add(new_object.GetComponent<person_attribute>().one_person);
                            persons_ob.Add(new_object);




                        }
                       
                    }

                }
            }
        }//sensor_button 클릭됨
        else if (Input.GetMouseButtonDown(0) && object_button1.mode_num == 4)
        {
            Ray cast_point = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = 1 << LayerMask.NameToLayer("building");
            if (Physics.Raycast(cast_point, out hit, Mathf.Infinity, layerMask) && hit.collider.name != "Plane")
            {
                if (sensorType == 39)
                    ori_object = ori_sensor_dir;
                else
                    ori_object = ori_sensor;
                new_object = Instantiate(ori_object, new Vector3(9, 2, -10), Quaternion.identity);
                new_object.transform.SetParent(all_objects.transform);
                new_object.tag = "sensor1";
                new_object.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                

                new_object.GetComponent<sensor_attribute>().one_sensor.positions = new_object.transform.position;
                new_object.GetComponent<sensor_attribute>().one_sensor.nodeId = sensor_id.ToString();
                new_object.GetComponent<sensor_attribute>().one_sensor.nodeType = sensorType;
                sensor_id++;

                sensor_list.Add(new_object.GetComponent<sensor_attribute>().one_sensor);
                sensor_ob.Add(new_object);
                
            }
        }//fire버튼 클릭됨
        else if (Input.GetMouseButtonDown(0) && object_button1.mode_num == 5)
        {
            Ray cast_point = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(cast_point, out hit, Mathf.Infinity) && hit.collider.gameObject.tag == "sensor1")
            {
                hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red;

                hit.collider.gameObject.GetComponent<sensor_attribute>().one_sensor.disaster = true;

                DisasterNodeJson temp_disaster = new DisasterNodeJson();
                temp_disaster.positions = hit.collider.gameObject.transform.position;
                temp_disaster.nodeId = disaster_id;
                disaster_id++;
                disaster_list.Add(temp_disaster);
                disaster_ob.Add(hit.collider.gameObject);

            }


        }//exit_button클릭됨
        else if (Input.GetMouseButtonDown(0) && object_button1.mode_num == 6)
        {
            Ray cast_point = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = 1 << LayerMask.NameToLayer("building");//building만해당되어야함
            if (Physics.Raycast(cast_point, out hit, Mathf.Infinity, layerMask))
            {
                ori_object = ori_exit;
                new_object = Instantiate(ori_object, new Vector3(9, 2, -10), Quaternion.identity);
                new_object.transform.SetParent(all_objects.transform);
                new_object.tag = "exit1";
                new_object.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                new_object.transform.rotation = ori_object.transform.rotation;

                ExitNodeJson temp_exit = new ExitNodeJson();
                temp_exit.positions = new_object.transform.position;
                temp_exit.nodeId = exit_id;
                exit_id++;

                exit_list.Add(temp_exit);
                exit_ob.Add(new_object);


            }
        }//set area
        else if (Input.GetMouseButtonDown(0) && object_button1.mode_num == 7)
        {

            Ray cast_point2 = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit2;
            if (Physics.Raycast(cast_point2, out hit2, Mathf.Infinity) && hit2.collider.name != "Plane")
            {
                
                Ray cast_point = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                int layerMask = 1 << LayerMask.NameToLayer("building");
                if (Physics.Raycast(cast_point, out hit, Mathf.Infinity, layerMask))
                {

                    

                    
                    Debug.Log("area_position Num : " + area_positions.Count);
                    Debug.Log("area_index : " + pb.area_index);
                    
                    GameObject new_area_num = Instantiate(area_num, new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z), Quaternion.identity);

                    new_position = new_area_num.GetComponent<areasensor_attribute>().one_sensor;
                    new_position.areaId = pb.area_index.ToString();
                    new_position.position = hit.point;
                    new_area_num.transform.SetParent(all_objects.transform);
                    new_area_num.tag = "area1";
                    new_area_num.GetComponent<TextMesh>().text = "Room" + pb.area_index;
                    new_area_num.transform.rotation = area_num.transform.rotation;
                    new_area_num.name = "area_num1";
                    area_nums.Add(new_area_num);
                    area_positions.Add(new_position);
                    pb.area_index++;
                }
            }

        }
        else if (Input.GetMouseButtonDown(1))
        {
            object_button1.mode_num = 0;
            Debug.Log("mode 끝");
        }
        //object_delete
        else if (Input.GetMouseButtonDown(0) && object_button1.mode_num == 0)
        {
            Ray cast_point1 = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit1;

            if (Physics.Raycast(cast_point1, out hit1, Mathf.Infinity))
            {
                if (hit1.collider.gameObject.tag == "person1" || hit1.collider.gameObject.tag == "sensor1" || hit1.collider.gameObject.tag == "exit1")
                {
                    set = 1;
                    chosen_object = hit1.collider.gameObject;
                   
                }
            }

        }//실제delete
        if (Input.GetKeyDown(KeyCode.Delete) && set == 1)
        {
            Destroy(chosen_object);
            set = 0;
        }




    }
    public void SetSenserType(int _a)
    {
        this.sensorType = _a;
        pb.close_all_windows();
    }
}
