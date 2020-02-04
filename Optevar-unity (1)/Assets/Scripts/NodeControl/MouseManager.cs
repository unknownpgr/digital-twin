using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseManager : MonoBehaviour
{
    public enum MouseMode
    {
        NORMAL, NODE_PLACING
    }

    static MouseMode mouseMode = MouseMode.NORMAL;
    static NodeManager placingNode;
    int placableLayer;

    public float wheelConstant = 30f;
    float panningValue = 5f;

    float rotY, rotX = 0;
    Transform transform;
    mouse_button mb = new mouse_button();

    private Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main.GetComponent<Camera>();
        transform = Camera.main.GetComponent<Transform>();

        rotX = transform.localEulerAngles.y;
        rotY = -transform.localEulerAngles.x;

        placableLayer = UnityEngine.LayerMask.NameToLayer("building");
    }

    // Update is called once per frame
    void Update()
    {
        switch (mouseMode)
        {
            case MouseMode.NORMAL:
                if (Input.GetMouseButtonDown(0))
                {
                    Ray cast_point = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(cast_point, out hit, Mathf.Infinity))
                    {
                        string tag = hit.collider.gameObject.tag;
                        // Do something with clicked object
                    }
                    else
                    {
                        // Nothing clicked.
                        // WindowManager.CloaseAll();
                    }
                }
                break;
            case MouseMode.NODE_PLACING:
                NodePlacing();
                break;
        }

        CameraMove();
    }

    public static void NodePlace(NodeManager obj)
    {
        Debug.Log("Place node");
        placingNode = obj;
        mouseMode = MouseMode.NODE_PLACING;
    }

    public static void ToNormalMode()
    {
        Debug.Log("Normal mode");
        placingNode = null;
        mouseMode = MouseMode.NORMAL;
    }

    float wheelValue = 0f;
    void CameraMove()
    {
        // Panning
        if (Input.GetMouseButton(2))
        {
            transform.position -= (transform.right.normalized * Input.GetAxis("Mouse X") + transform.up.normalized * Input.GetAxis("Mouse Y")) * panningValue;
        }

        // Rotating
        if (Input.GetMouseButton(1))
        {
            rotX += Input.GetAxis("Mouse X") * 100.0f * Time.deltaTime;
            rotY += Input.GetAxis("Mouse Y") * 100.0f * Time.deltaTime;
            transform.localEulerAngles = new Vector3(-rotY, rotX, 0);
        }

        wheelValue = Input.GetAxis("Mouse ScrollWheel") * wheelConstant;
        //Zoom
        if (wheelValue != 0)
        {
            camera.fieldOfView -= wheelValue;
        }
    }

    void NodePlacing()
    {
        Ray cast_point = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool isHit = Physics.Raycast(cast_point, out hit, Mathf.Infinity);
        if (isHit) isHit = (hit.collider.gameObject.layer == placableLayer);

        if (isHit)
        {
            placingNode.transform.position = hit.point;
        }
        else
        {
            placingNode.transform.position = transform.position;
        }

        // Place
        if (Input.GetMouseButton(0))
        {
            if (isHit)
            {
                FunctionManager.Popup(placingNode.GetType() + " placed.");
                mouseMode = MouseMode.NORMAL;
                placingNode = null;
            }
            else
            {
                FunctionManager.Popup("Cannot place node here.");
            }
        }

        // Cancel
        else if (Input.GetMouseButton(1))
        {
            Destroy(placingNode);
            placingNode = null;
            mouseMode = MouseMode.NODE_PLACING;
        }
    }
}
