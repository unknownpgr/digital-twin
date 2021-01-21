using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
관리자 전화번호를 가져오려면
GetSavedPhoneNumbers() 를 사용하기
*/

public class InformationManager : MonoBehaviour
{
    // Transform of panel of phone numbers
    private Transform phoneNumberPanel;

    // GameObject of panel of phone number input field
    private GameObject inputPhoneNumberPanel;

    // List of input fields about phone numbers
    private static List<InputField> inputFields = new List<InputField>();
    // List of input panels about phone numbers
    private static List<GameObject> inputPanels = new List<GameObject>();

    // Transform of information window
    private Transform informationWindow;
    // Transform of parent of information panels
    private Transform infoWindowPanelParent;

    // Start is called before the first frame update
    void Start()
    {
        // Get existing transform of panel of phone numbers
        phoneNumberPanel = GameObject.Find("panel_phone_number").transform;
        // Get existing game object of input panel of phone number
        inputPhoneNumberPanel = phoneNumberPanel.GetChild(1).gameObject;
        // Unactivate original input panel
        inputPhoneNumberPanel.SetActive(false);

        // Get existing transform of information window
        informationWindow = GameObject.Find("window_information").transform;
        // Get existing transform of parent of information panels
        infoWindowPanelParent = informationWindow.GetChild(3);

        // Update texts of input fields with saved data
        UpdatePhoneNumberInputFields();
    }

    // Update input fields with saved data
    // 다음의 경우에서 실행된다.
    // (1) Start system
    // (2) Click close button of information window
    // (3) Click save button
    public void UpdatePhoneNumberInputFields()
    {
        // Get saved data
        List<string> phoneNumbers = GetSavedPhoneNumbers();
        // Set texts of input fields with saved data
        CreateInputFieldsWithSavedData(phoneNumbers);
    }

    // Get list of saved phone numbers
    // and Return it
    public static List<string> GetSavedPhoneNumbers()
    {
        // Create new list of string
        List<string> savedPhoneNumbers = new List<string>();

        if (PlayerPrefs.HasKey("phone") == true)
        {
            string savedValue = PlayerPrefs.GetString("phone");

            if (savedValue.Length > 0)
            {
                /*
                saved value: 010-xxxx-xxxx/010-abcd-efgh
                */
                string[] parsed = savedValue.Split('/');

                foreach (var phoneNumber in parsed)
                {
                    if (phoneNumber != "")
                    {
                        // Add phone number to list
                        savedPhoneNumbers.Add(phoneNumber);
                    }
                }
            }
        }

        // Return list of phone numbers
        return savedPhoneNumbers;
    }

    // Create Input fields with parameter(saved data)
    private void CreateInputFieldsWithSavedData(List<string> phoneNumbers)
    {
        if (phoneNumbers.Count > 0)
        {
            // Destroy all existing input field
            foreach (var inputField in inputFields)
            {
                Destroy(inputField);
            }
            inputFields.Clear();

            // Destroy all existing game object of input panel
            foreach (var inputPanel in inputPanels)
            {
                Destroy(inputPanel);
            }
            inputPanels.Clear();

            // Create new input fields with parameter
            foreach (var phoneNumber in phoneNumbers)
            {
                AddInputField(phoneNumber);
            }
        }
    }

    // Add input field in panel
    // and Set text of input field to parameter
    public void AddInputField(string phoneNumber)
    {
        // Instantiate original input panel
        GameObject newInputPanel = Instantiate(inputPhoneNumberPanel);
        // Get input field and button in object
        InputField inputField = newInputPanel.transform.GetChild(0).GetComponent<InputField>();
        Button deleteButton = newInputPanel.transform.GetChild(1).GetComponent<Button>();

        // If there is non-null parameter, 
        // Set text of input field to it
        if (phoneNumber != null) { inputField.text = phoneNumber; }

        // Add input field to list
        inputFields.Add(inputField);
        // Add game object of input panel to list
        inputPanels.Add(newInputPanel);

        // Add (on click) method to delete button
        deleteButton.onClick.AddListener(() => DeleteInputField(newInputPanel));

        // Set transform of new input panel
        newInputPanel.transform.SetParent(phoneNumberPanel, false);
        newInputPanel.transform.localPosition = Vector3.zero;

        // Actiavate new input panel
        newInputPanel.SetActive(true);
    }

    //=============<call-back method>============================

    // Save data with texts of input fields
    public void SavePhoneNumberDataWithInputs()
    {
        if (PlayerPrefs.HasKey("phone"))
        {
            PlayerPrefs.DeleteKey("phone");
        }

        string inputs = "";

        if (inputFields.Count > 0)
        {
            foreach (var inputField in inputFields)
            {
                if (inputField.text.Length > 0)
                {
                    inputs += inputField.text + "/";
                }
            }
        }

        // If input fields are not empty
        // Save data with them
        if (inputs != "")
        {
            PlayerPrefs.SetString("phone", inputs);
            PlayerPrefs.Save();

            // Log
            Debug.Log("관리자 전화번호 " + inputFields.Count + "개:\n"
            + PlayerPrefs.GetString("phone").Replace('/', '\n'));
            Popup.Show("관리자 전화번호 " + inputFields.Count + "개가 저장되었습니다.");
        }
    }

    // Destroy input panel
    private void DeleteInputField(GameObject inputPanel)
    {
        // Get existing input field of input panel
        InputField inputField = inputPanel.transform.GetChild(0).GetComponent<InputField>();

        // Remove input field from list
        if (inputFields.Contains(inputField) == true) { inputFields.Remove(inputField); }
        // Remove input panel from list
        if (inputPanels.Contains(inputPanel) == true) { inputPanels.Remove(inputPanel); }

        // Update data with texts of input fields currently contained in list
        SavePhoneNumberDataWithInputs();

        // Destroy input field
        Destroy(inputField);
        // Destroy input panel
        Destroy(inputPanel);
    }

    // 선택된 메뉴에 해당하는 패널을 가장 마지막 자식으로 설정하여
    // 화면상 가장 앞으로 보이도록 함.
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

        UpdatePhoneNumberInputFields();
    }

    //===============<call-back method>============================
}
