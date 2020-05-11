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
    private static List<string> sirenIDList = new List<string>();


    // UI related variables
    private static List<GameObject> jsonButtons = new List<GameObject>();
    private Dictionary<string, GameObject> sensorButtons = new Dictionary<string, GameObject>();
    private static GameObject jsonButton;
    private GameObject sensorButton;
    private InputField saveFileName;
    public void LoadDataFromDB()
    {
        // Destroy existing nodes
        NodeManager.DestroyAll();

        // Load nodes from database
        DBManager dBManager = GameObject.Find("DBManager").GetComponent<DBManager>();
        dBManager.Init();

        foreach (string sd in dBManager.SensorLoad().Split('\n'))
        {
            string sensorData = sd.Trim();
            if (sensorData.Length < 3) continue;
            AddNodeFromString(sensorData);
        }

        // ToDo : 가능하면 AreaLoad함수도 합쳐버리자.
        foreach (string ar in dBManager.AreaLoad().Split('\n'))
        {
            string areaID = ar.Trim();
            if (areaID.Length < 2) continue;
            NodeManager.AddNode(areaID, typeof(NodeArea));
        }

        RenderNodeButtons();
    }

    public static void UpdateList()
    {
        string root = Application.dataPath + "/Resources/scenario_jsons";
        new Thread(() =>
        {
            DirectoryInfo folder = new DirectoryInfo(root);
            foreach (var file in folder.GetFiles("*.json")) jsonFileList.Add(file);
        }).Start();
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

    public static string[] GetSirenIDs()
    {
        return sirenIDList.ToArray();
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
            Transform panel = sensorPanel; // Default is sensorPanel
            if (nm is NodeArea) panel = areaPanel;
            if (nm is NodeExit) panel = exitPanel;
            newButton.transform.SetParent(panel, false);

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
        new Thread(() => File.WriteAllText(path, data)).Start();
        Popup.Show("저장되었습니다.");
        WindowManager.GetWindow("window_save_json").SetVisible(false);
        UpdateList();
    }

    private void OnSensorStateUpdated()
    {
        foreach (string physicalID in sensorButtons.Keys)
        {
            NodeManager nm = NodeManager.GetNodeByID(physicalID);
            if (nm == null) continue;
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
    }

    public void OnSensorManuallyCreate(InputField text)
    {
        string data = text.text;
        if (AddNodeFromString(data)) Popup.Show("노드를 새로 추가하였습니다.");
        else Popup.Show("문제가 발생하여 노드를 새로 추가하지 못했습니다.");
        RenderNodeButtons();
    }

    private bool AddNodeFromString(string sensorString)
    {
        string[] parsed = sensorString.Split(';');
        if (parsed.Length < 2) return false;

        string id = parsed[0].Trim();
        int typeNumber = int.Parse(parsed[1].Trim());

        Type type = null;
        switch (typeNumber)
        {
            case 21:
            case 22:
            case 23:
                type = typeof(NodeFireSensor);
                break;

            case 26:
                sirenIDList.Add(id);
                break;

            case 27:
                type = typeof(NodeDirection);
                break;

            case 50:
                type = typeof(NodeEarthquakeSensor);
                break;

            case 51:
                type = typeof(NodeFloodSensor);
                break;

            case 52:
                type = typeof(NodeExit);
                break;

            case 53:
                type = typeof(NodeArea);
                break;

            default:
                return false;
        }
        if (type != null && NodeManager.AddNode(id, type)) return true;
        return false;
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
