using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    // Popup associated values
    private static Vector3 POPUP_SHOW = new Vector2(0, -100);
    private static Vector3 POPUP_HIDE = new Vector2(0, 200);
    private RectTransform popupTransform;       // Transform
    private static Text popupText;              // Text
    private static float popupLifetime = 0;     // Popup lifetime. hide if 0
    public static float POPUP_DURATION = 5;     // Popup default lifetime.

    // Start is called before the first frame update
    void Start()
    {
        // Initialize popup
        popupTransform = gameObject.GetComponent<RectTransform>();
        popupText = popupTransform.GetChild(0).GetComponent<Text>();
        popupTransform.anchoredPosition = POPUP_HIDE;
    }

    // Update is called once per frame
    float t;
    void Update()
    {
        if (popupLifetime > 0)
        {
            // Showing
            if (popupLifetime > 1.0f)
            {
                t = POPUP_DURATION - popupLifetime;
                if (t > 1) t = 1;
            }
            // Hiding
            else
            {
                t = popupLifetime;
            }
            popupTransform.anchoredPosition = Vector2.Lerp(POPUP_HIDE, POPUP_SHOW, WindowManager.SmoothMove(t));
            popupLifetime -= Time.deltaTime;
        }
    }

    // Show popup message
    public static void Show(string text)
    {
        if (popupLifetime > 0) popupLifetime = POPUP_DURATION - 1.0f;
        else popupLifetime = POPUP_DURATION;        // Set lifetime to 5 sec
        popupText.text = text;                      // Set text fo popup message
    }
}
