using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class RouteRenderer : MonoBehaviour
{
    public delegate void RenderCallback(EvacuateRoute[] routes);
    public GameObject lineRendererObject;

    private static RouteRenderer singleTon = null;
    private static EvacuateRoute[] routes = null;
    private static RenderCallback callback = null;

    public static bool IsRendering()
    {
        return routes != null;
    }

    public static void Render(EvacuateRoute[] routes, RenderCallback callback)
    {
        if (singleTon == null) throw new Exception("Renderer is not initialized yet.");
        if (IsRendering()) throw new Exception("It is already rendering.");
        RouteRenderer.callback = callback;
        RouteRenderer.routes = routes;
    }

    private void Start()
    {
        if (singleTon != null) throw new Exception("Cannot initialize singleton object twicke");
        singleTon = this;
    }

    private void SetLineAttrs(LineRenderer lineRenderer, Color c, float size = 0.3f)
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.endColor = new Color(c.r, c.g, c.b, 1);
        lineRenderer.startColor = c;
        lineRenderer.material.color = c;
        lineRenderer.startWidth = size;
        lineRenderer.endWidth = size;
    }

    GameObject currentRoute;
    int screenShotIndex = -1;

    // Implement rendering process with finite state machine
    const int STATE_IDLE = 0x01;
    const int STATE_RENDER = 0x02;
    const int STATE_CAPTURE = 0x04;
    const int STATE_WAIT = 0x08;
    int state = STATE_IDLE;

    void Update()
    {
        switch (state)
        {
            case STATE_IDLE:
                {
                    if (routes == null) return;
                    screenShotIndex = 0;
                    state = STATE_RENDER;
                }
                break;
            case STATE_RENDER:
                {
                    // Render route
                    GameObject lineObject = Instantiate(lineRendererObject);
                    LineRenderer renderer = lineObject.GetComponent<LineRenderer>();

                    SetLineAttrs(renderer, new Color(0, 0, 1, 1f));

                    Vector3[] routePositions = routes[screenShotIndex].Route;
                    renderer.positionCount = routePositions.Length;
                    renderer.SetPositions(routePositions);
                    renderer.enabled = true;
                    currentRoute = lineObject;
                    state = STATE_CAPTURE;
                }
                break;
            case STATE_CAPTURE:
                {
                    ScreenshotManager.ScreenShot(this, texture =>
                    {
                        Destroy(currentRoute);
                        routes[screenShotIndex].Screenshot = texture;
                        screenShotIndex++;
                        if (screenShotIndex == routes.Length)
                        {
                            // All screenshot finished.
                            state = STATE_IDLE;
                            screenShotIndex = -1;
                            callback(routes);
                            routes = null;
                            callback = null;
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
                }
                break;
            default:
                throw new Exception("Unexpected state");
        }
    }
}