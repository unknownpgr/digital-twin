using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // window to be active at the mouse position
    public GameObject videoWindow;
    // Rect Transform of window
    private RectTransform videoWindowRect;
    // Rect Transform of parent of window
    private RectTransform parentRect;
    // UI Camera
    private Camera mainCamera;

    private void Start()
    {
        // Init
        videoWindowRect = videoWindow.GetComponent<RectTransform>();
        parentRect = videoWindow.GetComponentInParent<RectTransform>();
        mainCamera = Camera.main;
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        Vector2 localPos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect
            , Input.mousePosition
            , mainCamera
            , out localPos))
        {
            videoWindowRect.localPosition = localPos;
        }
        videoWindow.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        videoWindow.SetActive(false);
    }
}
