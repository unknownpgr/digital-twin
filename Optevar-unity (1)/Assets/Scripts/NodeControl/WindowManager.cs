using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WindowManager : MonoBehaviour
{
    // Dictinoary of all window. Tuple of <window name, windowManager>
    private static Dictionary<string, WindowManager> windows = new Dictionary<string, WindowManager>();

    public string WindowName = "Test_Window";
    public Vector3 WINODW_VISIBLE_POSITION = new Vector2(0, -100);

    // Private fields
    public Dictionary<string, Transform> Elements = new Dictionary<string, Transform>();
    private GameObject head;

    private Vector3 WINDOW_HIDE_POSITION;
    private bool visibility = false;
    private float movingTime = 0;   // Used to window moving.(hide/show)

    private RectTransform parantTransform;  // Rect transform of parent
    private RectTransform rectTransform;    // Rect transform of this window

    // Used to window drag
    private Vector2 mousePosition;

    // Start is called before the first frame update
    void Start()
    {
        FunctionManager.RecursiveRegisterChild(transform, Elements);
        Debug.Log("Window assigned : " + WindowName);
        Elements["text_title"].GetComponent<Text>().text = WindowName;

        if (!Application.IsPlaying(gameObject)) return;


        // Load transforms
        rectTransform = gameObject.GetComponent<RectTransform>();
        parantTransform = transform.parent.GetComponent<RectTransform>();

        WINDOW_HIDE_POSITION = new Vector2(0, rectTransform.sizeDelta.y + 100);

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
        });

        // Add drag event listener for window moving
        EventTrigger.Entry entryDragging = new EventTrigger.Entry();
        entryDragging.eventID = EventTriggerType.Drag;
        entryDragging.callback.AddListener((eventData) =>
        {
            rectTransform.anchoredPosition = mousePosition + delta;
        });

        // Add drag end event listener of window positioning
        EventTrigger.Entry entryDrop = new EventTrigger.Entry();
        entryDrop.eventID = EventTriggerType.EndDrag;
        entryDrop.callback.AddListener((evetData) =>
        {
            WINODW_VISIBLE_POSITION = rectTransform.anchoredPosition;
            WINDOW_HIDE_POSITION.x = rectTransform.anchoredPosition.x;
        });

        // Add event listener
        EventTrigger trigger = Elements["header"].GetComponent<EventTrigger>();
        trigger.triggers.Add(entryDragging);
        trigger.triggers.Add(entryBeginDrag);
        trigger.triggers.Add(entryDrop);

        // Add close event listener
        Elements["button_close"].GetComponent<Button>().onClick.AddListener(() => { SetVisible(false); });
    }

    // Half size of screen. used for mouse position convertion. I assumed that the screen size would not change.
    Vector2 align = new Vector2(Screen.width / 2, -Screen.height / 2);
    void Update()
    {
        if (visibility)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parantTransform, Input.mousePosition, GetComponentInParent<Canvas>().worldCamera, out mousePosition);
            mousePosition += align;
        }

        if (movingTime > 0)
        {
            float t;
            if (visibility) t = movingTime;
            else t = 1 - movingTime;
            rectTransform.anchoredPosition = Vector2.Lerp(WINODW_VISIBLE_POSITION, WINDOW_HIDE_POSITION, SmoothMove(t));
            movingTime -= Time.deltaTime;
        }

        // Exponential approach. Easy to calculate but foo fast when hide.
        // rectTransform.anchoredPosition += (targetPosition - rectTransform.anchoredPosition) * Time.deltaTime * 4.0f;
    }

    public static WindowManager GetWindow(string windowName)
    {
        if (windows.ContainsKey(windowName)) return windows[windowName];
        else return null;
    }

    public static void CloseAll()
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

    /*
    Below is a function that:
    1) f(0)=0, f(1)=1
    2) f'(0)=f'(1)=f''(0)=f''(1)=0
    Therefore smoothly move and accelerates.
    */
    public static float SmoothMove(float t)
    {
        return t * t * t * (3 * t * (2 * t - 5) + 10);
    }
}