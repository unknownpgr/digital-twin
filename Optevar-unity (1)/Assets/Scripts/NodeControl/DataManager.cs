using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class DataManager : MonoBehaviour
{
    private static List<FileInfo> jsonFileList = new List<FileInfo>();
    private static List<GameObject> jsonButtons = new List<GameObject>();
    private Dictionary<string, GameObject> sensorButtons = new Dictionary<string, GameObject>();

    private static GameObject jsonButton;
    private GameObject sensorButton;


    private InputField saveFileName;
    public static void UpdateList()
    {
        string root = Application.dataPath + "/Resources/scenario_jsons";
        Thread t = new Thread(() =>
        {
            DirectoryInfo folder = new DirectoryInfo(root);
            foreach (var file in folder.GetFiles("*.json")) jsonFileList.Add(file);
        });
        t.Start();
    }
    private static FileInfo selectedJsonFile;

    public static void SetJosnFileList()
    {
        // Delete existing buttons
        foreach (GameObject g in jsonButtons) GameObject.Destroy(g);
        jsonButtons.Clear();

        Transform panelJson = GameObject.Find("panel_json_file").transform;

        // Create floor buttons
        GameObject newButton;
        foreach (FileInfo fi in jsonFileList)
        {
            newButton = Instantiate(jsonButton);
            newButton.SetActive(true);
            newButton.transform.SetParent(panelJson, false);
            newButton.transform.localPosition = Vector3.zero;
            newButton.transform.GetChild(0).GetComponent<Text>().text = fi.Name;

            Image img = newButton.GetComponent<Image>();
            img.color = Color.grey;
            newButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                selectedJsonFile = new FileInfo(fi.FullName);
                foreach (GameObject button in jsonButtons)
                {
                    Image image = button.GetComponent<Image>();
                    image.color = Color.grey;
                }
                img.color = Color.white;
            });
            jsonButtons.Add(newButton);
        }
    }

    // Called when load json button of menu bar clicked
    public void OnLoadJson()
    {
        WindowManager loadJsonWindow = WindowManager.GetWindow("window_load_json");
        SetJosnFileList();
        loadJsonWindow.SetVisible(true);
    }

    // Called when user select json file and click load button
    public void OnLoadJsonFile()
    {
        if (selectedJsonFile == null)
        {
            Popup.Show("JSON파일을 선택해주세요.");
            return;
        }
        NodeManager.DestroyAll();
        NodeManager.InitiateFromFile(selectedJsonFile.FullName);
        RenderNodeButtons();
        saveFileName.text = Path.GetFileNameWithoutExtension(selectedJsonFile.FullName);

        Popup.Show("센서 설정을 로드하였습니다.");
        WindowManager.GetWindow("window_load_json").SetVisible(false);

        selectedJsonFile = null;
    }

    public void LoadDataFromDB()
    {
        // Destroy existing nodes
        NodeManager.DestroyAll();

        // Load nodes from database
        DBManager dBManager = GameObject.Find("DBManager").GetComponent<DBManager>();
        foreach (string sd in dBManager.SensorLoad(true).Split('\n'))
        {
            string sensorData = sd.Trim();
            if (sensorData.Length < 3) continue;
            string[] parts = sensorData.Split(';');
            if (parts.Length < 2)
            {
                Debug.Log("Wrong sensor data string : " + sensorData);
                continue;
            }
            string id = parts[0];
            // 지금은 화재센서밖에 없으니까 그냥 하드코딩 해 놨는데, 일단은 typeNumber를 받아 둔다.
            int typeNumber = int.Parse(parts[1]);
            Type type = typeof(NodeFireSensor);

            NodeManager.AddNode(id, type);
        }

        foreach (string ar in dBManager.AreaLoad(true).Split('\n'))
        {
            string areaID = ar.Trim();
            if (areaID.Length < 2) continue;
            NodeManager.AddNode(areaID, typeof(NodeArea));
        }
        RenderNodeButtons();
    }

    private void RenderNodeButtons()
    {
        // Remove existing buttons
        foreach (GameObject button in sensorButtons.Values) GameObject.Destroy(button);
        sensorButtons.Clear();

        // 노드 로드하여 윈도우에 추가하는 부분
        // Initialize sensor buttons and create existing node

        Transform sensorPanel = GameObject.Find("panel_sensor").transform;
        Transform areaPanel = GameObject.Find("panel_area").transform;
        Transform exitPanel = GameObject.Find("panel_exit").transform;

        GameObject newButton;
        foreach (string physicalID in NodeManager.GetNodeIDs())
        {
            NodeManager nm = NodeManager.GetNodeByID(physicalID);
            newButton = Instantiate(sensorButton);
            newButton.SetActive(true);

            if (nm is NodeFireSensor) newButton.transform.SetParent(sensorPanel, false);
            if (nm is NodeDirection) newButton.transform.SetParent(sensorPanel, false);
            if (nm is NodeArea) newButton.transform.SetParent(areaPanel, false);
            if (nm is NodeExit) newButton.transform.SetParent(exitPanel, false);

            newButton.transform.localPosition = Vector3.zero;
            newButton.transform.GetChild(0).GetComponent<Text>().text = nm.DisplayName;
            newButton.GetComponent<Button>().onClick.AddListener(() => OnSelectSensor(physicalID));
            sensorButtons.Add(nm.PhysicalID, newButton);
        }

        OnSensorStateUpdated();
    }

    public void OnSelectSensor(string nodeID)
    {
        NodeManager node = NodeManager.GetNodeByID(nodeID);
        MouseManager.ToNodePlaceMode(node);
    }

    public void OnSaveJson()
    {
        WindowManager loadJsonWindow = WindowManager.GetWindow("window_save_json");
        loadJsonWindow.SetVisible(true);
    }

    public void OnSaveJsonFile()
    {
        string fileName = saveFileName.text;
        if (fileName == null || fileName.Length < 1)
        {
            Popup.Show("파일 이름을 입력하지 않았습니다.");
            return;
        }
        string path = Application.dataPath + "/Resources/scenario_jsons/" + fileName + ".json";
        string data = NodeManager.Jsonfy();
        Thread t = new Thread(() => File.WriteAllText(path, data));
        t.Start();
        Popup.Show("저장되었습니다.");
        WindowManager.GetWindow("window_save_json").SetVisible(false);
        UpdateList();
    }

    private void OnSensorStateUpdated()
    {
        foreach (string physicalID in sensorButtons.Keys)
        {
            Debug.Log(physicalID);
            NodeManager nm = NodeManager.GetNodeByID(physicalID);
            if(nm==null)continue;
            Color color;
            switch (nm.State)
            {
                case NodeManager.NodeState.STATE_INITIALIZED:
                    color = new Color(.7f, .7f, 1);
                    break;
                case NodeManager.NodeState.STATE_PLACING:
                    color = new Color(.7f, .7f, .7f);
                    break;
                default: // NodeManager.NodeState.STATE_UNINITIALIZED
                    color = new Color(1, .7f, .7f);
                    break;
            }
            sensorButtons[physicalID].GetComponent<Image>().color = color;
        }

        // Update json file
        // string path = Application.dataPath + "/Resources/scenario_jsons/NEW.json";
        // File.WriteAllText(path, NodeManager.Jsonfy());
    }

    public void OnSensorManuallyCreate(InputField text)
    {
        string data = text.text;
        string[] parsed = data.Split(';');
        if (parsed.Length < 2) return;
        string id = parsed[0].Trim();
        int typeNumber = int.Parse(parsed[1].Trim());
        Type type;
        switch (typeNumber)
        {
            case 21:
            case 22:
            case 23:
                type = typeof(NodeFireSensor);
                break;
            default:
                type = typeof(NodeArea);
                break;
        }
        if (NodeManager.AddNode(id, type))
        {
            Popup.Show("노드를 새로 추가하였습니다.");
        }
        else
        {
            Popup.Show("문제가 발생하여 노드를 새로 추가하지 못했습니다.");
        }
        RenderNodeButtons();
    }

    void Start()
    {
        // Hide prefab
        jsonButton = GameObject.Find("button_json_file").gameObject;
        jsonButton.SetActive(false);

        sensorButton = GameObject.Find("button_sensor_ID").gameObject;
        sensorButton.SetActive(false);

        // Register input feild
        saveFileName = GameObject.Find("input_save_file").GetComponent<InputField>();
        NodeManager.OnNodeStateChanged += OnSensorStateUpdated;

        LoadDataFromDB();

        UpdateList();
    }
}
