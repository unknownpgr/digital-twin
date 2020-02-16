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
    public RectTransform scrollList;

    // Start is called before the first frame update
    void Start()
    {
        // Get building skp file location
        string root = Application.dataPath;
        string builings = root + "/Resources/Models";

        // Get building skp list and make buttons
        float term = 10;
        float left = term;
        float right = 0;
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

            // Place
            buildingView.transform.SetParent(scrollList.transform);
            RectTransform rt = buildingView.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(left, 0);
            rt.offsetMin = new Vector2(rt.offsetMin.x, 0);
            rt.offsetMax = new Vector2(rt.offsetMax.x, 0);
            left += rt.sizeDelta.x + term;
        }

        // Extend viewer
        scrollList.sizeDelta = new Vector2(left, scrollList.sizeDelta.y);
    }

    // Update is called once per frame
    void Update()
    {

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

        // Set text
        Text buildingName = prefab
                            .transform
                            .GetChild(1)
                            .GetChild(0)
                            .GetComponent<Text>();
        buildingName.text = name;
        // ToDo : Set image
        return prefab;
    }
}
