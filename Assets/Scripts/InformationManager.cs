using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InformationManager : MonoBehaviour
{

    // InputField about phone number
    public InputField inputPhoneNum;

    /*
    // InputField about building
    public InputField inputBuilding;



    // InputField about mqtt
    public InputField inputMqtt;

    // InputField about system
    public InputField inputSystem;
    */


    // Transform of information window
    public Transform informationWindow;
    // Transform of parent of information panels
    private Transform infoWindowPanelParent;

    // Start is called before the first frame update
    void Start()
    {
        // Get existing transform of parent of panels
        infoWindowPanelParent = informationWindow.GetChild(3);

        // Set texts of information window to saved values
        UpdateTextsOfInformationWindow();
    }

    // Update is called once per frame
    void Update()
    {
   
    }

    // (TO DO) 임시로 만들어놓은 함수. 나중에 input 값이 정해지면 각자에 맞는 메서드를 만들 것.
    public void EditInformationToInput()
    {
        // Current selected button
        GameObject currentButton = EventSystem.current.currentSelectedGameObject;
        /*
         * name of button : button_edit_****
         * name of parent(panel): panel_****
         */
        string[] parsed = currentButton.transform.parent.gameObject.name.Split('_');
        string menuName = parsed[1];

        string inputValue;

        switch (menuName)
        {
            case "phone":
                inputValue = inputPhoneNum.text;
                break;
            /*
            case "building":
                inputValue = inputBuilding.text;
                break;

            case "mqtt":
                inputValue = inputMqtt.text;
                break;

            case "system":
                inputValue = inputSystem.text;
                break;
            */
            default:
                inputValue = null;
                break;
        }

        if (inputValue == null || inputValue.Length < 1)
        {
            Popup.Show("유효한 값을 입력하지 않았습니다.");
        }
        else
        {
            SaveInformationToInput(menuName, inputValue);
            Popup.Show(inputValue + "로 정보가 저장되었습니다.");
            UpdateTextsOfInformationWindow();
        }
    }

    private void SaveInformationToInput(string menuName, string inputValue)
    {
        if (PlayerPrefs.HasKey(menuName) == true)
        {
            RemoveInformation(menuName);
        }

        PlayerPrefs.SetString(menuName, inputValue);
        PlayerPrefs.Save();
    }

    private string GetInformationValue(string menuName)
    {
        string inputValue;

        if (PlayerPrefs.HasKey(menuName) == true)
        {
            inputValue = PlayerPrefs.GetString(menuName);
        }
        else
        {
            switch (menuName)
            {
                case "phone":
                    inputValue = "전화번호를 입력하세요.";
                    break;

                case "building":
                    inputValue = "건물 정보를 입력하세요.";
                    break;

                case "mqtt":
                    inputValue = "MQTT 정보를 입력하세요.";
                    break;

                case "system":
                    inputValue = "시스템 정보를 입력하세요.";
                    break;
                default:
                    inputValue = null;
                    break;
            }
        }

        return inputValue;
    }

    // Remove information data with given value
    private void RemoveInformation(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    // Remove all information data
    private void RemoveAllInformation()
    {
        PlayerPrefs.DeleteAll();
    }

    // Update texts of information window to saved values
    // 다음의 경우에서 실행된다.
    // (1) Start system
    // (2) Click close button of information window
    // (3) Click edit button
    public void UpdateTextsOfInformationWindow()
    {
        inputPhoneNum.text = GetInformationValue("phone");
        /*
        inputBuilding.text = GetInformationValue("building");
        inputMqtt.text = GetInformationValue("mqtt");
        inputSystem.text = GetInformationValue("system");
        */

    }

    public void OnClickInformationWindowMenu()
    {
        // Transform of panel about clicked button
        Transform selectedPanelTransform;
        // Name of clicked button
        string buttonName = EventSystem.current.currentSelectedGameObject.name;
        /*
         * button name : button_****
         * panel name : panel_****
         */
        string[] parsed = buttonName.Split('_');
        string panelName = "panel";

        // Make panel name with parsed button name
        for (int i = 1; i < parsed.Length; i++)
        {
            string temp = "_" + parsed[i];
            panelName += temp;
        }

        // Find panel with panel name using parent transform
        selectedPanelTransform = infoWindowPanelParent.Find(panelName);
        // Set panel transform to the end of the transform list
        selectedPanelTransform.SetAsLastSibling();

        UpdateTextsOfInformationWindow();
    }
}
