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
        float rightOffset = 0;
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
            /*
            buildingView.transform.SetParent(scrollList.transform);
            RectTransform rt = buildingView.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(leftOffset, 0);
            rt.offsetMin = new Vector2(rt.offsetMin.x, 0);
            rt.offsetMax = new Vector2(rt.offsetMax.x, 0);
            leftOffset += rt.sizeDelta.x + space;
            */
        }

        // Extend viewer
        // scrollList.sizeDelta = new Vector2(leftOffset, scrollList.sizeDelta.y);
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
}
