using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    // video window index
    // Because monobehavior works in single thread, do not care about race condition.
    private static int videoWindowIndex = 1;

    // (TODO) 재난 발생 시 윈도우가 자동으로 뜰 때 영상은 가장 최근에 눌린 카메라나
    // 화재 발생 가까운 곳에 있는 카메라의 영상을 비춰줘야 함.
    private Text cameraID;
    private Text sensorID;

    // video manager
    private VideoManager videoManager = null;

    private void Start()
    {
        // Get exsisting video window
        // videoWindow = GameObject.Find("window_video").gameObject;
        videoWindow = GameObject.Find("window_video");
        // videoWindow.name = "window_video_" + videoWindowIndex++;
        // Get Rect Transform of video window
        videoWindowRect = videoWindow.GetComponent<RectTransform>();
        // Get Rect Transform of 'body'
        parentRect = videoWindow.transform.parent.parent.GetComponent<RectTransform>();
        // Get exsiting main camera
        mainCamera = Camera.main;

        // Get exsiting video manager
        videoManager = GameObject.Find("Master").GetComponent<VideoManager>();

        // Get texts of video window object
        cameraID = videoWindow.transform.GetChild(1).GetChild(1).GetComponent<Text>();
        sensorID = videoWindow.transform.GetChild(1).GetChild(2).GetComponent<Text>();

        // update texts with saved data
        UpdateTextOfWindow();
    }

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

        SaveRecentlyClickedCameraID(this.gameObject.name);
        UpdateTextOfWindow();

        // and Actiavate video window
        videoWindow.SetActive(true);

        // Play video
        videoManager.videoPlayer.Play();
    }

    // Detect when Cursor leaves the camera object
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        // Unactivate video window
        videoWindow.SetActive(false);

        // Stop playing video
        videoManager.videoPlayer.Stop();
    }

    public void UpdateTextOfWindow()
    {
        cameraID.text = GetSavedCameraID();
    }

    // Save recently clicked ID of camera object
    // and return if this work is successful
    private bool SaveRecentlyClickedCameraID(string cameraID)
    {
        bool isSaved;

        if(PlayerPrefs.HasKey("cameraID") == true)
        {
            RemoveSavedCameraID();
        }

        PlayerPrefs.SetString("cameraID", cameraID);
        PlayerPrefs.Save();

        if(PlayerPrefs.HasKey("cameraID") == true &&
            PlayerPrefs.GetString("cameraID") == cameraID)
        {
            isSaved = true;
        }
        else
        {
            isSaved = false;
        }

        return isSaved;
    }

    // Remove saved ID of camera object
    // and return true if this work is successful
    private bool RemoveSavedCameraID()
    {
        bool isRemoved;

        PlayerPrefs.DeleteKey("cameraID");

        isRemoved = !PlayerPrefs.HasKey("cameraID");

        return isRemoved;
    }

    // Get saved camera ID value
    // and Return it
    private string GetSavedCameraID()
    {
        string cameraID = "camera ID";

        if(PlayerPrefs.HasKey("cameraID") == true)
        {
            cameraID = PlayerPrefs.GetString("cameraID");
        }

        return cameraID;
    }
}
