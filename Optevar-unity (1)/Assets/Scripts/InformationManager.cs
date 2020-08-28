using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InformationManager : MonoBehaviour
{
    // InputField about building
    public InputField inputBuilding;
    // Variable to save input building
    private string buildingInfo;

    // InputField about phone number
    public InputField inputPhoneNum;
    // Variable to save input phone number
    private string phoneNumber;

    // InputField about mqtt
    public InputField inputMqtt;
    // Variable to save input mqtt
    private string mqttInfo;

    // InputField about system
    public InputField inputSystem;
    // Variable to save input system
    private string systemInfo;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
 
    public void EditPhoneNumber()
    {
        // Set phone number to input value
        phoneNumber = inputPhoneNum.text;

        // If input value is invalid, pop up message
        if (phoneNumber == null || phoneNumber.Length < 1)
        {
            Popup.Show("유효한 전화번호를 입력하지 않았습니다.");
            return;
        }

        Popup.Show(phoneNumber +"로 전화번호가 저장되었습니다.");
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
        string inputValueName = parsed[1];

        switch (inputValueName)
        {
            case "phone":
                phoneNumber = inputPhoneNum.text;              
                break;
            case "building":
                buildingInfo = inputBuilding.text;
                if (buildingInfo == null || buildingInfo.Length < 1)
                {
                    Popup.Show("유효한 건물 정보를 입력하지 않았습니다.");
                }
                Popup.Show(buildingInfo + "로 건물 정보가 저장되었습니다.");
                break;

            case "mqtt":
                mqttInfo = inputMqtt.text;
                if (mqttInfo == null || mqttInfo.Length < 1)
                {
                    Popup.Show("유효한 mqtt 정보를 입력하지 않았습니다.");
                }
                Popup.Show(mqttInfo + "로 mqtt 정보가 저장되었습니다.");
                break;

            case "system":
                systemInfo = inputSystem.text;
                if (systemInfo == null || systemInfo.Length < 1)
                {
                    Popup.Show("유효한 시스템 정보를 입력하지 않았습니다.");
                }
                Popup.Show(systemInfo + "로 system 정보가 저장되었습니다.");
                break;
        }

        
    }
}
