using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropdownPrefaceController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == this.gameObject)
        {
            SetPrefaceText();
        }
    }

    public void SetPrefaceText()
    {
        /*
                GameObject.Find("Item 0: 건물 선택하기").transform      // Dropdown List
                .GetChild(1)        // Item Label
                .GetComponent<Text>().text = "--------------------------------------------------";

        */
    }


    public void OnPointerEnter(PointerEventData pointerEventData)
    {

    }

    // Detect when Cursor leaves the camera object
    public void OnPointerExit(PointerEventData pointerEventData)
    {

    }

}
