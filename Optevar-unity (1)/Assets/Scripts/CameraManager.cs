using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class CameraManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Game Object of window to be active at the mouse position
    private GameObject videoWindow;
    // Rect Transform of window
    private RectTransform videoWindowRect;
    // Rect Transform of parent of window
    private RectTransform parentRect;
    // UI Camera
    private Camera mainCamera;

    // Video Player of video window
    private VideoPlayer videoPlayer;

    // video window index
    // Because monobehavior works in single thread, do not care about race condition.
    private static int videoWindowIndex = 1;

    private void Start()
    {
        // Get exsisting video window
        // videoWindow = GameObject.Find("window_video").gameObject;
        GameObject origin = GameObject.Find("window_video");
        videoWindow = Instantiate(origin, origin.transform.parent);
        videoWindow.name = "window_video_" + videoWindowIndex++;
        // Get Rect Transform of video window
        videoWindowRect = videoWindow.GetComponent<RectTransform>();
        // Get Rect Transform of 'body'
        parentRect = videoWindow.transform.parent.parent.GetComponent<RectTransform>();
        // Get exsiting main camera
        mainCamera = Camera.main;

        // Get exsisting Video Player of window video
        videoPlayer = videoWindow.GetComponent<VideoPlayer>();
    }

    // (TO DO) video window의 크기를 고려하여 위치를 조정할 것
    // Detect if the Cursor starts to pass over the camera object
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        Vector2 localPos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect
            , Input.mousePosition
            , mainCamera
            , out localPos))
        {
            Vector2 position = new Vector2(0f, videoWindowRect.rect.height);

            if (localPos.x + videoWindowRect.rect.width > parentRect.rect.x * (-1))
            {
                position += new Vector2(-videoWindowRect.rect.width, 0f);
            }

            if (localPos.y + videoWindowRect.rect.height > parentRect.rect.y * (-1))
            {
                position += new Vector2(0f, -videoWindowRect.rect.height);
            }

            // Set local position of video window to position of cursor
            videoWindowRect.localPosition = localPos + position;
        }

        // and Actiavate video window
        videoWindow.SetActive(true);
    }

    // Detect when Cursor leaves the camera object
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        // Unactivate video window
        videoWindow.SetActive(false);
    }

    private void SetVideoClip()
    {
        VideoClip videoClip;
        videoClip = Resources.Load<VideoClip>("Video/Office Background 2") as VideoClip;
    }
}
