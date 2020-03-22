﻿using System.Collections;
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

    public static class MouseState
    {
        public static bool IsLeftClicked { get; private set; }
        public static bool IsRightClicked { get; private set; }
        public static bool IsDoubleClicked { get; private set; }
        public static bool IsHit { get; private set; }
        public static bool IsOverUI { get; private set; }
        public static bool IsOverGameObject { get; private set; }
        private static RaycastHit hit;
        public static RaycastHit Hit { get => hit; private set { } }
        public static GameObject Target { get => hit.collider.gameObject; }
        public static Vector3 Point { get => hit.point; private set { } }
        public static Vector3 Normal { get => hit.normal; private set { } }

        public static float doubleClickDuraction = .5f;
        private static float doubleClickTimer = 0;

        public static void UpdateMouseState()
        {
            // Detect double click
            if (doubleClickTimer > 0) doubleClickTimer -= Time.deltaTime;

            IsLeftClicked = Input.GetMouseButtonDown(0);
            IsRightClicked = Input.GetMouseButton(1);
            IsDoubleClicked = false;

            IsOverUI = EventSystem.current.IsPointerOverGameObject();

            if (IsLeftClicked)
            {
                // Check doubleclick
                if (IsDoubleClicked = (doubleClickTimer > 0)) doubleClickTimer = 0;
                else doubleClickTimer = doubleClickDuraction;
            }

            // Get clicked item
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            IsHit = Physics.Raycast(ray, out hit, Mathf.Infinity);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.GetComponent<Transform>();

        // Initialize rotation to current rotation
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
    void Update()
    {
        MouseState.UpdateMouseState();

        // Mouse mode state machine
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
        // Do not modify any UI elementes except targetMrk here.

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

    void NodePlacing()
    {
        bool isPlaceable = false;
        if (MouseState.IsHit) isPlaceable = (MouseState.Target.layer == placableLayer) && EventSystem.current.currentSelectedGameObject == null;

        if (isPlaceable)
        {
            // Show targetMark
            targetMark.SetActive(true);
            // Move target marker to the hit point, but lift a little from surface.
            targetMark.transform.position = MouseState.Point + MouseState.Normal * 0.1f;
            // Set the direction of mark.
            targetMark.transform.rotation = Quaternion.LookRotation(MouseState.Normal, Vector3.one);
        }
        else targetMark.SetActive(false);

        // Place
        if (MouseState.IsLeftClicked)
        {
            if (isPlaceable)
            {
                // Activate node
                placingNode.SetActive(true);

                // Place given node
                placingNode.Position = MouseState.Point;

                // Check if given surface is wall
                float angleCosine = Mathf.Abs(Vector3.Dot(Vector3.up, MouseState.Normal));
                if (angleCosine < 0.5f) FunctionManager.Popup("Warning : " + placingNode.DisplayName + " placed on wall.");
                else FunctionManager.Popup(placingNode.DisplayName + " placed.");

                // Change to normal mode. Initialize placing node to null.
                placingNode = null;

                ToNormalMode();
            }
            else
            {
                FunctionManager.Popup("Cannot place node here.");
            }
        }

        // Cancel
        else if (MouseState.IsRightClicked)
        {
            ToNormalMode();
        }
    }

    float wheelValue = 0f;
    private Vector3 velocity = Vector3.zero;
    private void CameraMove()
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
}
