using System;
using System.Collections.Generic;
using UnityEngine;

public class RouteRenderer : MonoBehaviour
{
    public delegate void RenderCallback();
    public GameObject lineRendererObject;

    private static bool isCreated = false;
    private static EvacuateRouter.EvacuateScenario[] senarios = null;
    private static RenderCallback callback = null;
    private static readonly List<GameObject> currentRoutes = new List<GameObject>();
    private static int screenShotIndex = -1;

    // Implement rendering process with finite state machine
    private const int STATE_IDLE = 0x01;
    private const int STATE_RENDER = 0x02;
    private const int STATE_CAPTURE = 0x04;
    private const int STATE_WAIT = 0x08;
    private static int state = STATE_IDLE;

    private static bool IsRendering { get { return senarios != null; } }

    public static void InitRenderer()
    {
        // All screenshot finished.
        state = STATE_IDLE;
        screenShotIndex = -1;
        senarios = null;
        callback = null;
        foreach (GameObject route in currentRoutes) Destroy(route);
    }

    public static void Render(EvacuateRouter.EvacuateScenario[] senarios, RenderCallback callback)
    {
        if (!isCreated) throw new Exception("Route renderer is not created.");
        if (IsRendering) throw new Exception("It is already rendering.");
        InitRenderer();
        RouteRenderer.callback = callback;
        RouteRenderer.senarios = senarios;
    }

    private void RenderRoute(Vector3[] routePositions, float size)
    {
        GameObject lineObject = Instantiate(lineRendererObject);
        Color color = new Color(0, 0, 1, 1f);

        LineRenderer renderer = lineObject.GetComponent<LineRenderer>();
        renderer.useWorldSpace = true;
        renderer.endColor = color;
        renderer.startColor = color;
        renderer.material.color = color;
        renderer.startWidth = size;
        renderer.endWidth = size;
        renderer.positionCount = routePositions.Length;
        renderer.SetPositions(routePositions);
        renderer.enabled = true;
        currentRoutes.Add(lineObject);
    }

    private void RenderRoutes(Vector3[][] routes, float size = 1.0f)
    {
        foreach (var route in routes)
        {
            RenderRoute(route, size);
        }
    }

    private void Start()
    {
        if (isCreated) throw new Exception("There cannot be more than one routerenderer.");
        isCreated = true;        
    }

    void Update()
    {
        switch (state)
        {
            case STATE_IDLE:
                {
                    if (senarios == null) return;
                    if (senarios.Length == 0) throw new Exception("Senario list has been set but it does not contains any senario.");

                    // Senario list is set and it contains at least 1 senario.
                    foreach (GameObject route in currentRoutes) Destroy(route); // Remove all existing routes
                    screenShotIndex = 0; // Set screenshot index
                    state = STATE_RENDER; // Go to next state at next frame
                }
                break;
            case STATE_RENDER:
                {
                    // Render route
                    RenderRoutes(senarios[screenShotIndex].Routes);
                    state = STATE_CAPTURE;
                }
                break;
            case STATE_CAPTURE:
                {
                    // Take screenshot
                    ScreenshotManager.ScreenShot(this, texture =>
                    {
                        foreach (GameObject route in currentRoutes) Destroy(route);
                        senarios[screenShotIndex].Screenshot = texture;
                        screenShotIndex++;
                        if (screenShotIndex == senarios.Length)
                        {
                            // Backup callback and best senario before initialize
                            RenderCallback cb = callback;
                            EvacuateRouter.EvacuateScenario bestSenario = senarios[0];

                            // Initialize renderer
                            InitRenderer();

                            // Call callback
                            cb();

                            // Render best senario
                            RenderRoutes(bestSenario.Routes);
                        }
                        else
                        {
                            state = STATE_RENDER;
                        }
                    });
                    state = STATE_WAIT;
                }
                break;
            case STATE_WAIT:
                {
                    // Do nothing; just wait.
                    // Callback in STATE_CAPUTER will asynchronously update the state.
                }
                break;
            default:
                throw new Exception("Unexpected state");
        }
    }
}