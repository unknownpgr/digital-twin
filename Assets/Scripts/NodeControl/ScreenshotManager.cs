using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotManager
{
    public delegate void ScreenshotCallBack(Texture2D screenShot);

    // 2. Wrap it.
    public static void ScreenShot(MonoBehaviour context, ScreenshotCallBack callback)
    {
        context.StartCoroutine(ScreenShotCoroutine(callback));
    }

    // 1. Make a async function.
    private static IEnumerator ScreenShotCoroutine(ScreenshotCallBack callback)
    {
        // Initialize subcamera
        Camera subCamera = GameObject.Find("SubCamera").GetComponent<Camera>();

        Vector3 buildingSize = BuildingManager.BuildingBound.size;
        float cameraViewSize = Mathf.Max(buildingSize.x, buildingSize.z) / 2;
        subCamera.orthographicSize = cameraViewSize;

        Vector3 cameraPosition = BuildingManager.BuildingBound.center;
        cameraPosition.y = 100;
        subCamera.transform.position = cameraPosition;

        // Enable camera
        subCamera.enabled = true;

        // Wait for some frames
        for (int i = 0; i < 3; i++) yield return new WaitForEndOfFrame();

        // Prepare texture
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24); //depth : 일단 24

        // Set target texture of the subCamera
        subCamera.targetTexture = renderTexture;

        // Render
        subCamera.Render();

        // Disable camera
        subCamera.enabled = false;

        //Get Texture
        RenderTexture.active = renderTexture;
        Texture2D screenShotTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
        screenShotTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        screenShotTexture.Apply();

        // Call callback
        callback(screenShotTexture);
    }
}
