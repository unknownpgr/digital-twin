using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class mouse_button : MonoBehaviour
{
    private bool isClicked = false;
    private bool qPressed = false;

    private Transform mouseButton;
    // Start is called before the first frame update
    private Image image;

    void Start()
    {
        mouseButton = this.gameObject.transform;
        image = gameObject.GetComponent<Image>();
    }

    // Update is called once per frame
    bool befState = false;
    void Update()
    {
        qPressed = Input.GetKey(KeyCode.Q);

        if (befState != IsClicked())
        {
            if (IsClicked())
            {
                image.color = new Color(239f / 255f, 112f / 255f, 106f / 255f, 255f / 255f);
            }
            else
            {
                image.color = new Color(38f / 255f, 43f / 255f, 53f / 255f, 255f / 255f);
            }
        }

        befState = IsClicked();
    }

    public bool IsClicked()
    {
        return isClicked | qPressed;
    }

    public void on_mouse_button_down()
    {
        isClicked = !isClicked;
    }
}
