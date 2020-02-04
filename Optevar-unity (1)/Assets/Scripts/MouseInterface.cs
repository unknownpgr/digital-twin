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
    float panningValue = 5f;
    bool isMouseMovingMode;
    int PanRotateFlag; // 0 1 2

    float rotY, rotX = 0;
    bool ZoomFlag;
    Transform transform;
    mouse_button mb = new mouse_button();

    private Camera camera;


    private void Awake()
    {
        transform = this.GetComponent<Transform>();
        if (InformWindows == null) InformWindows = GameObject.Find("InformationWindow").gameObject;
        if (InformWindows != null)
        {
            SensorWindow = GameObject.Find("SensorInform").GetComponent<SensorWindow>();
            AreaSensorWindow = GameObject.Find("AreaSensorInform").GetComponent<AreaSensorWindow>();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        isMouseMovingMode = false;
        PanRotateFlag = 0;
        ZoomFlag = false;
        rotX = transform.localEulerAngles.y;
        rotY = -transform.localEulerAngles.x;
        camera = Camera.main.GetComponent<Camera>();
        mb = GameObject.Find("mouse_button").GetComponent<mouse_button>();
    }

    // Update is called once per frame
    void Update()
    {
        isMouseMovingMode = mb.IsClicked();

        if (isMouseMovingMode)
        {
            // Panning & Rotating
            if (PanRotateFlag == 0 & Input.GetMouseButtonDown(2) | Input.GetMouseButtonDown(1))
            {
                if (Input.GetMouseButtonDown(2)) PanRotateFlag = 1;
                else if (Input.GetMouseButtonDown(1)) PanRotateFlag = 2;
            }
            if (!Input.GetMouseButton(2) & !Input.GetMouseButton(1)) PanRotateFlag = 0;

            MoveCam(Input.mousePosition);
        }
        if (Input.GetMouseButtonDown(0)) LeftClick();
    }

    void LeftClick()
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

    float wheelValue = 0f;
    void MoveCam(Vector3 pos)
    {
        if (PanRotateFlag == 1)
        {
            // Panning
            transform.position -= (transform.right.normalized * Input.GetAxis("Mouse X") + transform.up.normalized * Input.GetAxis("Mouse Y")) * panningValue;
        }

        else if (PanRotateFlag == 2)
        {
            // Rotating
            //Debug.Log("Rotating");
            rotX += Input.GetAxis("Mouse X") * 100.0f * Time.deltaTime;
            rotY += Input.GetAxis("Mouse Y") * 100.0f * Time.deltaTime;
            //rotY = Mathf.Clamp(rotY, -45f, 45f);
            transform.localEulerAngles = new Vector3(-rotY, rotX, 0);
        }

        wheelValue = Input.GetAxis("Mouse ScrollWheel") * wheelConstant;
        //Zoom
        if (wheelValue != 0)
        {
            Debug.Log("Wheel" + camera.fieldOfView);
            camera.fieldOfView -= wheelValue;
        }
    }
}
