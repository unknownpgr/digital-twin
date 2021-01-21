using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Used to select building at buildingSelect scene.
// Attatched to camera.

public class BuildingSelector : MonoBehaviour
{
    // This is Dropdown component of Dropdown for selecting building
    public Dropdown dropdown;

    // These are Button components of Start Button
    public GameObject startButtonObject;
    private Button startButton;
    private Image startButtonImage;

    // a List of Dropdown options (building name)
    private List<string> buildingNames = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        // Get components of start button
        startButton = startButtonObject.GetComponent<Button>();
        startButtonImage = startButtonObject.GetComponent<Image>();

        // Set Dropdown options with building names
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

    // This method is attached to dropdown
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
            Debug.Log("selected building name: " + selectedBuildingName);

            // If start button is not interactable
            // then Set it to be interactable
            if (startButton.interactable == false)
            {
                startButton.interactable = true;
            }
            // and Set button image color to #E0E1FF   
            startButtonImage.color = new Color(0.1882353f, 0.2117647f, 0.6666667f);

            // Remove all listeners
            // and Add listener that start system with selected building
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => OnBuildingSelected(buildings, selectedBuildingName));
        }
        else if (index == 0)
        {
            // If start button is interactable
            // then Set it to be not interactiable
            if (startButton.interactable == true)
            {
                startButton.interactable = false;
            }
            // and Set button image color to #3036AA
            startButtonImage.color = new Color(0.8784314f, 0.8823529f, 1.0f);

            // Remove all listeners 
            startButton.onClick.RemoveAllListeners();
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

    public void CloseApp()
    {
        Application.Quit();
    }

    /*
    // public RectTransform scrollList;
    public Transform content;

    // Start is called before the first frame update
    void Start()
    {
        // Get building skp file location
        string root = Application.dataPath;
        string builings = root + "/Resources/Models";

        // Get building skp list and make buttons
        float space = 10;
        float leftOffset = space;
        foreach (string filePath in Directory.GetFiles(builings))
        {
            // Check if given file is skp file
            if (!filePath.Contains(".skp")) continue;
            if (filePath.Contains(".meta")) continue;

            // Get name of building
            string buildingName = getBuildingName(filePath);

            // Get view
            GameObject buildingView = GetBuildingView(buildingName);

            // Add event listener
            EventTrigger trigger = buildingView.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) =>
            {
                OnBuildingSelected(filePath, buildingName);
            });
            trigger.triggers.Add(entry);
        }
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

    private string getBuildingName(string path)
    {
        // path = ~~~\~~~/name.skp
        char[] splitter = { '/', '\\', '.' };
        string[] parsed = path.Split(splitter);
        return parsed[parsed.Length - 2];
    }

    private GameObject GetBuildingView(string name)
    {
        // Get prefab and instantiate
        GameObject prefab = (GameObject)Resources.Load("Prefabs/BuildingView");
        prefab = (GameObject)Instantiate(prefab);

        prefab.transform.SetParent(content);
        prefab.transform.localPosition = Vector3.zero;
        prefab.transform.localScale = new Vector3(1.0f, 1.0f);

        // Set text
        Text buildingName = prefab
                            .transform
                            .GetChild(0)
                            .GetChild(0)
                            .GetComponent<Text>();
        buildingName.text = name;

        // Load image from file
        string filePath = Application.dataPath + "/textures/" + name + ".png";
        byte[] fileData;
        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);

            // Convert it to sprite
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            // Apply
            prefab.GetComponent<Image>().sprite = sprite;
        }
        else
        {
            Debug.Log("Thumbnail image " + filePath + " does not exist.");
        }

        return prefab;
    }
    */
}
