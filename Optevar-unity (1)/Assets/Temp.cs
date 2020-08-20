using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Used to select building at buildingSelect scene.
// Attatched to camera.

public class Temp : MonoBehaviour
{

    // This is Dropdown component of Dropdown for selecting building
    public Dropdown dropdown;

    // This is Button component of Start Button that start with selected building
    public Button startButton;

    // This is game object of caution text
    public GameObject cautionText;

    // a List of Dropdown options (building name)
    private List<string> buildingNames = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        SetDropdownOptions();
    }

    // Get building skp files and building names
    // and Set Dropdown options to building names
    private void SetDropdownOptions()
    {
        // Clear the old options of the Dropdown menu
        dropdown.ClearOptions();

        // Add preface text for dropdown 
        buildingNames.Add("건물 선택하기");

        // Get building skp file location
        string root = Application.dataPath;
        string buildings = root + "/Resources/Models";

        // Get building skp list
        foreach (string filePath in Directory.GetFiles(buildings))
        {
            // Check if given file is skp file
            if (!filePath.Contains(".skp")) continue;
            if (filePath.Contains(".meta")) continue;

            // Get name of building
            string buildingName = GetBuildingName(filePath);
            // Add name of building to list
            buildingNames.Add(buildingName);
        }

        // Add list of building name to dropdown options
        dropdown.AddOptions(buildingNames);
    }

    public void SetStartButton()
    {
        // Get building skp file location
        string root = Application.dataPath;
        string buildings = root + "/Resources/Models";

        // This is the index number of the current selection in the Dropdown
        int index = dropdown.value;

        // the value at index zero is not building name, so
        // If index isn't zero then 
        // Set start button with valid building file 
        if (index != 0)
        {
            // Find building in list using index
            string selectedBuildingName = buildingNames[index];
            Debug.Log("index number: " + index + " "+ "selected building name: " + selectedBuildingName);

            // If caution text is active
            // then Unactivate it
            if (cautionText.activeSelf == true)
            {
                cautionText.SetActive(false);
            }

            // Remove all listeners
            // and Add listener that start system with selected building
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => OnBuildingSelected(buildings, selectedBuildingName));
        }
        else if (index == 0)
        {
            // Remove all listeners
            // and Add listener that activate caution text
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() =>
            {
                // If caution text is unactive
                // then Activate it
                if (cautionText.activeSelf == false)
                {
                    cautionText.SetActive(true);
                }
            });
        }
    }

    // If user doesn't select building
    // then Show catuion text
    public void ShowCautionText()
    {
        if (cautionText.activeSelf == false)
        {
            cautionText.SetActive(true);
        }
    }

    private string GetBuildingName(string path)
    {
        // path = ~~~\~~~/name.skp
        char[] splitter = { '/', '\\', '.' };
        string[] parsed = path.Split(splitter);
        return parsed[parsed.Length - 2];
    }

    private void OnBuildingSelected(string path, string name)
    {
        FunctionManager.BuildingPath = path;
        FunctionManager.BuildingName = name;

        // ToDo : Check DB
        bool dataExists = false;

        if (dataExists) SceneManager.LoadScene("MonitoringMode");
        else SceneManager.LoadScene("PlacingMode");
    }
}
