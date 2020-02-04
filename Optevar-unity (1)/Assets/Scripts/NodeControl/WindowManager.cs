using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WindowManager : MonoBehaviour
{
    private Vector3 WINDOW_HIDE_POSITION;
    private Vector3 WINODW_VISIBLE_POSITION = new Vector2(0, -100);
    private bool visibility = false;
    private float movingTime = 0;

    private RectTransform parantTransform;
    private RectTransform rectTransform;
    private static Dictionary<string, WindowManager> windows = new Dictionary<string, WindowManager>();

    Vector2 mousePosition;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Window assigned : " + gameObject.name);

        // Load transforms
        Transform transform = gameObject.GetComponent<Transform>();
        rectTransform = gameObject.GetComponent<RectTransform>();
        parantTransform = transform.parent.GetComponent<RectTransform>();

        WINDOW_HIDE_POSITION = new Vector2(0,rectTransform.sizeDelta.y+100);

        // Move current window to given position
        rectTransform.anchoredPosition = WINDOW_HIDE_POSITION;

        // Add window to dictionary.
        windows.Add(gameObject.name, this);

        // Window moving part
        Vector2 delta = new Vector2(0, 0);

        // Add drag start event listener
        EventTrigger.Entry entryBeginDrag = new EventTrigger.Entry();
        entryBeginDrag.eventID = EventTriggerType.BeginDrag;
        entryBeginDrag.callback.AddListener((eventData) =>
        {
            delta = rectTransform.anchoredPosition - mousePosition;
            Debug.Log(delta);
        });

        // Add drag event listener for window moving
        EventTrigger.Entry entryDragging = new EventTrigger.Entry();
        entryDragging.eventID = EventTriggerType.Drag;
        entryDragging.callback.AddListener((eventData) =>
        {
            rectTransform.anchoredPosition = mousePosition + delta;
        });

        // Add event listener
        EventTrigger trigger = transform.GetChild(0).Find("title").GetComponent<EventTrigger>();
        trigger.triggers.Add(entryDragging);
        trigger.triggers.Add(entryBeginDrag);

        // Add close event listener
        transform.GetChild(0).Find("close_button").GetComponent<Button>().onClick.AddListener(() =>
             {
                 SetVisible(false);
             });
    }

    // Half size of screen. used for mouse position convertign. I supoosed that screen size won't be changed.
    Vector2 align = new Vector2(Screen.width / 2, -Screen.height / 2);
    void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parantTransform, Input.mousePosition, GetComponentInParent<Canvas>().worldCamera, out mousePosition);
        mousePosition += align;

        if (movingTime > 0)
        {
            if (visibility) rectTransform.anchoredPosition = Vector2.Lerp(WINODW_VISIBLE_POSITION, WINDOW_HIDE_POSITION, movingTime);
            else rectTransform.anchoredPosition = Vector2.Lerp(WINODW_VISIBLE_POSITION, WINDOW_HIDE_POSITION, 1 - movingTime);
            movingTime -= Time.deltaTime;
        }

        // rectTransform.anchoredPosition += (targetPosition - rectTransform.anchoredPosition) * Time.deltaTime * 4.0f;
    }

    public static WindowManager GetWindow(string windowName)
    {
        if (windows.ContainsKey(windowName)) return windows[windowName];
        else return null;
    }

    public static void CloaseAll()
    {
        foreach (string key in windows.Keys)
        {
            windows[key].SetVisible(false);
        }
    }

    public void SetPosition(int x, int y)
    {
        rectTransform.position = new Vector3(x, y, 0);
    }

    public void SetVisible(bool visibility)
    {
        if (this.visibility != visibility)
        {
            movingTime = 1.0f;
            this.visibility = visibility;
        }

        if (visibility)
        {
            // Do some initializations here.
        }
    }

    public static float SmoothMove(float t)
    {
        return t * t * t * (3 * t * (2 * t - 5) + 10);
    }
}