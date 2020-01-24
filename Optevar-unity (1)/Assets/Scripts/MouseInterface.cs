using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseInterface : MonoBehaviour
{
    public GameObject InformWindows;
    SensorWindow SensorWindow;
    AreaSensorWindow AreaSensorWindow;
    public float wheelConstant = 30f;
    float wheelValue = 0f;
    float panningValue = 5f;
    bool MouseInterfaceFlag;
    int PanRotateFlag; // 0 1 2
   
    Vector3 preCamPos;
    float rotY, rotX = 0;
    Vector3 preScreenPos; // for panning
    bool ZoomFlag;
    new Transform transform;
    mouse_button mb = new mouse_button();


    private void Awake()
    {
        transform = this.GetComponent<Transform>();
        Init();
    }
    // Start is called before the first frame update
    void Start()
    {
        
        MouseInterfaceFlag = false;
        PanRotateFlag = 0;
        ZoomFlag = false;
        rotX = transform.localEulerAngles.y;
        rotY = -transform.localEulerAngles.x;
        
        mb = GameObject.Find("mouse_button").GetComponent<mouse_button>();
    }

    // Update is called once per frame
    void Update()
    {

        //print(Input.mousePosition);
        if (Input.GetKeyDown(KeyCode.Q)) 
        {
            preCamPos = transform.position;
            preScreenPos = Input.mousePosition;
            //Debug.Log("Init");
        }
        MouseInterfaceFlag = Input.GetKey(KeyCode.Q);

        if (MouseInterfaceFlag || mb.is_clicked==1)
        {
            
            if (mb != null)
            {

                if (mb.is_clicked == 0 & !MouseInterfaceFlag)
                    return;
                mb.mouse_ob.GetComponent<Image>().color = new Color(239f / 255f, 112f / 255f, 106f / 255f, 255f / 255f);
            }
            // Panning & Rotating
            if (PanRotateFlag == 0 & Input.GetMouseButtonDown(2) | Input.GetMouseButtonDown(1))
            {
                //Debug.Log("ee");
                preCamPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                preScreenPos = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y);
                if (Input.GetMouseButtonDown(2))
                    PanRotateFlag = 1;
                else if (Input.GetMouseButtonDown(1))
                    PanRotateFlag = 2;
            }
            if (!Input.GetMouseButton(2) & !Input.GetMouseButton(1)) PanRotateFlag = 0;

            // Zooming
            wheelValue = Input.GetAxis("Mouse ScrollWheel") * wheelConstant;
            MoveCam(Input.mousePosition);
        }
        else if (mb != null)
            if (mb.is_clicked == 0 )
            {
                //Debug.Log("QQ");
                mb.mouse_ob.GetComponent<Image>().color = new Color(38f / 255f, 43f / 255f, 53f / 255f, 255f / 255f);
            }//mb.mouse_ob.gameObject.GetComponent<Renderer>().material.color = new Color(38f, 43f, 53f, 255f);
        if (Input.GetMouseButtonDown(0)) Test();
    }
    public void Init()
    {
        if (InformWindows == null) InformWindows = GameObject.Find("InformationWindow").gameObject;
        if (InformWindows != null)
        {
            SensorWindow = GameObject.Find("SensorInform").GetComponent<SensorWindow>();
            AreaSensorWindow = GameObject.Find("AreaSensorInform").GetComponent<AreaSensorWindow>();
        }
    }
    void Test()
    {
        Ray cast_point = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(cast_point, out hit, Mathf.Infinity))
        {
            string tag = hit.collider.gameObject.tag;
            Debug.Log(tag);
            if (tag == "sensor1")
            {
                if (!InformWindows.activeSelf) InformWindows.SetActive(true);
                SensorWindow.gameObject.SetActive(true);
                sensor_attribute tmp = hit.collider.gameObject.GetComponent<sensor_attribute>();
                SensorWindow.SetGUI(Vector3.zero, tmp);
                
            }
            else if (tag == "area1")
            {
                if (!InformWindows.activeSelf) InformWindows.SetActive(true);
                AreaSensorWindow.gameObject.SetActive(true);
                areasensor_attribute tmp = hit.collider.gameObject.GetComponent<areasensor_attribute>();
                AreaSensorWindow.SetGUI(Vector3.zero, tmp);
            }
        }
    }
    void MoveCam(Vector3 pos)
    {
       
        if (PanRotateFlag == 1)
        {
            // Panning
            //Debug.Log("Panning");
            transform.position -= (transform.right.normalized * Input.GetAxis("Mouse X") + transform.up.normalized * Input.GetAxis("Mouse Y")) * panningValue;
        } else if (PanRotateFlag == 2)
        {
            // Rotating
            //Debug.Log("Rotating");
            rotX += Input.GetAxis("Mouse X") * 100f * Time.deltaTime;
            rotY += Input.GetAxis("Mouse Y") * 100f * Time.deltaTime;
            //rotY = Mathf.Clamp(rotY, -45f, 45f);
            transform.localEulerAngles = new Vector3(-rotY, rotX, 0);
        }
        //if (RotateFlag)
        {

        }

        //Zoom
        if (wheelValue != 0)
        {
            
            Camera.main.fieldOfView -= wheelValue;
            Camera.main.transform.GetChild(0).GetComponent<Camera>().fieldOfView = Camera.main.fieldOfView;
        }
    }


}
