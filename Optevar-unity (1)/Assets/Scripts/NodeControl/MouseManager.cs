using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviour
{
    public enum MouseMode
    {
        NORMAL,         // Normal mode. User can click object to see attribute or can use UI buttons.
        NODE_PLACING,   // Node placing mode. User only can place node. If they click UI, node placing will be canceled.
        NODE_MOVING,    // Node moving mode
        NODE_EDITING    // Node editing mode
    }

    // Do not directly set mouseMode. mouseMode only can be set by NodePlace and ToNormal function.
    private static MouseMode mouseMode = MouseMode.NORMAL;
    private static NodeManager placingNode;
    int placableLayer;

    // Camera pan / rotation / zoom
    public float wheelConstant = 30f;
    float panningValue = 5f;
    float rotY, rotX;

    // Camera transfrom
    private static Transform cameraTransform;

    // Camera moving. dest.y is some kind of flag. if dest.y>0, do tracking. or, do nothing.
    Vector3 dest = Vector3.down;

    private static GameObject targetMark;


    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.GetComponent<Transform>();

        rotX = cameraTransform.localEulerAngles.y;
        rotY = -cameraTransform.localEulerAngles.x;

        placableLayer = UnityEngine.LayerMask.NameToLayer("building");

        targetMark = (GameObject)Instantiate((GameObject)Resources.Load("Prefabs/TargetMark"));
        targetMark.SetActive(false);

        // Set camera position
        cameraTransform.position = new Vector3(0, 100, -100);
        cameraTransform.rotation = Quaternion.Euler(45, 0, 0);
        rotX = 0;
        rotY = -45;
    }

    // Update is called once per frame
    private float doubleClickTimer = 0;
    private float doubleClickDuraction = 0.5f;
    void Update()
    {
        // Detect double click
        if (doubleClickTimer > 0) doubleClickTimer -= Time.deltaTime;

        bool isMouseOnUI = true;// EventSystems.EventSystem.IsPointerOverGameObject();
        bool isClicked = Input.GetMouseButtonDown(0);
        bool isDoubleClicked = false;
        GameObject target;
        if (isClicked)
        {
            // Check doubleclick
            if (isDoubleClicked = (doubleClickTimer > 0)) doubleClickTimer = 0;
            else doubleClickTimer = doubleClickDuraction;

            // Get clicked item
            RaycastHit hit;
            Ray cast_point = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cast_point, out hit, Mathf.Infinity)) target = hit.collider.gameObject;
        }

        switch (mouseMode)
        {
            case MouseMode.NORMAL:
                // if (isDoubleClicked) dest = hit.point;
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
        placingNode.SetActive(false);
        mouseMode = MouseMode.NODE_PLACING;
    }

    public static void ToNormalMode()
    {
        // Do nothing before initialization
        if (!targetMark) return;
        targetMark.SetActive(false);
        Debug.Log("Normal mode");

        // if placingNode is not null, it means node is not placed.
        if (placingNode != null)
        {
            placingNode.Destroy();
            FunctionManager.Popup("Node placing canceled.");
        }
        mouseMode = MouseMode.NORMAL;
    }

    float wheelValue = 0f;
    private Vector3 velocity = Vector3.zero;
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

        // Zoom
        wheelValue = Input.GetAxis("Mouse ScrollWheel");
        if (wheelValue != 0)
        {
            cameraTransform.position += cameraTransform.forward * wheelValue * wheelConstant;
        }

        // Tracking
        if (dest.y > 0)
        {
            // If near enough, stop.
            if ((cameraTransform.position - dest).magnitude < 10) dest.y = -1;
            else cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, dest, ref velocity, 0.3f);
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
            targetMark.SetActive(true);
            // Move target marker to the hit point, but lift a little from surface.
            targetMark.transform.position = hit.point + hit.normal * 0.1f;
            // Set the direction of mark.
            targetMark.transform.rotation = Quaternion.LookRotation(hit.normal, Vector3.one);
        }
        else
        {
            targetMark.SetActive(false);
        }

        // Place
        if (Input.GetMouseButton(0))
        {
            if (isHit)
            {
                // Activate node
                placingNode.SetActive(true);

                // Place given node
                placingNode.Position = hit.point;

                // Check if given surface is wall
                float angleCosine = Mathf.Abs(Vector3.Dot(Vector3.up, hit.normal));
                if (angleCosine < 0.5f) FunctionManager.Popup("Warning : " + placingNode.DisplayName + " placed on wall.");
                else FunctionManager.Popup(placingNode.DisplayName + " placed.");

                // Change to normal mode. Initialize placing node to null.
                placingNode = null;
                ToNormalMode();

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
            ToNormalMode();
        }
    }
}
