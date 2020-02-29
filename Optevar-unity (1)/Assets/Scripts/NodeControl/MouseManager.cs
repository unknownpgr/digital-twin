using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseManager : MonoBehaviour
{
    public enum MouseMode
    {
        NORMAL,         // Normal mode. User can do click object to see attribute or can use UI buttons.
        NODE_PLACING    // Node placing mode. User only can place node. If they click UI, node placing will be canceled. 
    }

    // Do not directly set mouseMode. mouseMode only can be set by NodePlace and ToNormal function.
    private static MouseMode mouseMode = MouseMode.NORMAL;
    private static NodeManager placingNode;
    int placableLayer;

    // Camera pan / rotation / zoom
    public float wheelConstant = 30f;
    float panningValue = 5f;
    float rotY, rotX = 0;

    // Camera / transfrom
    private Camera camera;
    private static Transform cameraTransform;

    // Camera moving. dest.y is some kind of flag. if dest.y>0, do tracking. or, do nothing.
    Vector3 dest = Vector3.down;

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
        targetMark.SetActive(false);

        // Set camera position
        cameraTransform.position = new Vector3(0, 100, -100);
        cameraTransform.rotation = Quaternion.Euler(45, 0, 0);
    }

    // Update is called once per frame
    private float doubleClickTimer = 0;
    private float doubleClickDuraction = 0.5f;
    private bool doubleClick = false;
    void Update()
    {
        if (doubleClickTimer > 0) doubleClickTimer -= Time.deltaTime;
        switch (mouseMode)
        {
            case MouseMode.NORMAL:
                if (Input.GetMouseButtonDown(0))
                {
                    // Double clicked. initialize timer to prevent tirple-click.
                    if (doubleClick = (doubleClickTimer > 0)) doubleClickTimer = 0;
                    else doubleClickTimer = doubleClickDuraction;

                    Ray cast_point = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(cast_point, out hit, Mathf.Infinity))
                    {
                        string tag = hit.collider.gameObject.tag;
                        // Do something with clicked object.
                        // For example, go nearby there.
                        if (doubleClick) dest = hit.point;
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
        if (placingNode)
        {
            placingNode.Destroy();
            FunctionManager.Popup("Node placing canceled.");
            placingNode = null;
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
                placingNode.transform.position = hit.point;

                // Check if given surface is wall
                float angleCosine = Mathf.Abs(Vector3.Dot(Vector3.up, hit.normal));
                if (angleCosine < 0.5f) FunctionManager.Popup("Warning : " + placingNode.Type + " placed on wall.");
                else FunctionManager.Popup(placingNode.Type + " placed.");

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
