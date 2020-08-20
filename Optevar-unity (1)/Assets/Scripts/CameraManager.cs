using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class CameraManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // (TO DO) Function Manager의 canvas가 초기화되기 전이라 Find 메서드로 video window의 Game Object를 가져올 수가 없음
    // inspector 창에서 drag & drop으로 연결해놓았지만
    // camera 객체가 prefab이므로 스크립트로 참조해야함.
    // window to be active at the mouse position
    public GameObject videoWindow;
    // Rect Transform of window
    private RectTransform videoWindowRect;
    // Rect Transform of parent of window
    private RectTransform parentRect;
    // UI Camera
    private Camera mainCamera;

    // Video Player of video window
    private VideoPlayer videoPlayer;

    private void Start()
    {
        // Get exsisting window video
        // videoWindow = FunctionManager.Find("window_video").gameObject;
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
            // Set local position of video window to position of cursor
            videoWindowRect.localPosition = localPos;
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
        // videoPlayer.clip = 
    }

}
