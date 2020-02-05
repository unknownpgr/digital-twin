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

    // Camera pan / rotation / zoom
    public float wheelConstant = 30f;
    float panningValue = 5f;
    float rotY, rotX = 0;
    private static Transform cameraTransform;

    private Camera camera;

    private static GameObject targetMark;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main.GetComponent<Camera>();
        cameraTransform = Camera.main.GetComponent<Transform>();

        rotX = cameraTransform.localEulerAngles.y;
        rotY = -cameraTransform.localEulerAngles.x;

        placableLayer = UnityEngine.LayerMask.NameToLayer("building");

        targetMark = (GameObject)Instantiate((GameObject)Resources.Load("Prefabs/TargetMark"));
        targetMark.transform.position = cameraTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        switch (mouseMode)
        {
            case MouseMode.NORMAL:
                targetMark.transform.position = cameraTransform.position;
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
                        // WindowManager.CloseAll();
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
        placingNode = obj;
        placingNode.transform.position = cameraTransform.position;
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
            cameraTransform.position -= (cameraTransform.right.normalized * Input.GetAxis("Mouse X") + cameraTransform.up.normalized * Input.GetAxis("Mouse Y")) * panningValue;
        }

        // Rotating
        if (Input.GetMouseButton(1))
        {
            rotX += Input.GetAxis("Mouse X") * 100.0f * Time.deltaTime;
            rotY += Input.GetAxis("Mouse Y") * 100.0f * Time.deltaTime;
            cameraTransform.localEulerAngles = new Vector3(-rotY, rotX, 0);
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
            // Move target marker to the hit point, but lift a little from surface.
            targetMark.transform.position = hit.point + hit.normal * 0.1f;
            // Set the direction of mark.
            targetMark.transform.rotation = Quaternion.LookRotation(hit.normal, Vector3.one);
        }
        else
        {
            targetMark.transform.position = cameraTransform.position;
        }

        // Place
        if (Input.GetMouseButton(0))
        {
            if (isHit)
            {
                // Place given node
                placingNode.transform.position = hit.point;

                // Check if given surface is wall
                float angleCosine = Mathf.Abs(Vector3.Dot(Vector3.up, hit.normal));
                if (angleCosine < 0.5f) FunctionManager.Popup("Warning : " + placingNode.Type + " placed on wall.");
                else FunctionManager.Popup(placingNode.Type + " placed.");

                // Change to normal mode
                mouseMode = MouseMode.NORMAL;
                placingNode = null;

                // Uncomment here to continue placing.
                // NodePlace(NodeManager.GetNode(NodeManager.NodeType.SENSOR_FIRE));
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
            mouseMode = MouseMode.NORMAL;
            FunctionManager.Popup("Canceled.");
        }
    }
}
