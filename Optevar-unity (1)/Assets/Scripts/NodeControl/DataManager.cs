using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class DataManager : MonoBehaviour
{
    // Root path for json file
    private static string root;
    private static readonly List<FileInfo> jsonFileList = new List<FileInfo>();

    // UI related variables
    private static GameObject jsonButton;
    private GameObject sensorButton;
    private static List<GameObject> jsonButtons = new List<GameObject>();
    private Dictionary<string, GameObject> sensorButtons = new Dictionary<string, GameObject>();
    private InputField inputSaveFileName;

    // Current json file
    private static FileInfo currentJsonFile;

    // AutoSave file path
    private static string autoSaveFilePath;

    // DBManager
    private DBManager dBManager;

    // DataManager 초기화
    void Start()
    {
        // Set root path
        root = Application.dataPath + "/Resources/scenario_jsons/";
        // Set autosave file path
        autoSaveFilePath = root + Constants.GetAutoSaveFileName(FunctionManager.BuildingName);

        // Hide prefab
        jsonButton = GameObject.Find("button_json_file").gameObject;
        jsonButton.SetActive(false);
        sensorButton = GameObject.Find("button_sensor_ID").gameObject;
        sensorButton.SetActive(false);

        // Register input feild
        inputSaveFileName = GameObject.Find("input_save_file").GetComponent<InputField>();

        // Register callback
        NodeManager.OnNodeStateChanged += OnSensorStateUpdated;

        // Initialize dbmanager before use.
        dBManager = GameObject.Find("DBManager").GetComponent<DBManager>();
        dBManager.Init();

        currentJsonFile = null;
        try
        {
            // 먼저 JSON파일에서 정보 로드를 시도함.
            NodeManager.DestroyAll();
            NodeManager.InitiateFromFile(autoSaveFilePath);
            RenderNodeButtons();
            inputSaveFileName.text = Path.GetFileNameWithoutExtension(autoSaveFilePath);
        }
        catch
        {
            // 안되면 DB에서 로드
            LoadDataFromDB();
        }

        // 세이브파일(.json파일)리스트 업데이트. 주기적으로 호출해주면 좋음.
        UpdateSaveFileList();
    }

    // DB로부터 노드 데이터를 로드한다. 기존의 노드 버튼들 삭제, 버튼 렌더링 등 모든 과정들이 여기 다 포함된다.
    public bool LoadDataFromDB()
    {
        // Check if data loaded
        bool isDataLoaded = false;

        // Destroy existing nodes
        NodeManager.DestroyAll();

        // Load nodes from database
        foreach (string sd in dBManager.SensorLoad().Split('\n'))
        {
            string sensorData = sd.Trim();
            if (sensorData.Length < 3) continue;
            isDataLoaded = true;
            AddNodeFromString(sensorData);
        }

        // ToDo : 가능하면 AreaLoad함수도 합쳐버리자.
        foreach (string ar in dBManager.AreaLoad().Split('\n'))
        {
            string areaID = ar.Trim();
            if (areaID.Length < 2) continue;
            isDataLoaded = true;
            NodeManager.AddNode(areaID, typeof(NodeArea));
        }

        RenderNodeButtons();
        return isDataLoaded;
    }

    // 루트 디렉토리를 읽어서 json파일 리스트를 업데이트한다. 이 함수를 따로 만든 이유는, 자주 호출해야하는 반면 blocking되는 작업일 것 같아서.
    public static void UpdateSaveFileList()
    {
        new Thread(() =>
        {
            DirectoryInfo folder = new DirectoryInfo(root);
            foreach (var file in folder.GetFiles("*.json")) jsonFileList.Add(file);
        }).Start();
    }

    // 저장된 정보 파일 리스트 윈도우의 버튼들을 지우고 새로 만든다.
    public static void RenderSaveFileButtons()
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
            img.color = Color.white;
            newButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                currentJsonFile = new FileInfo(fi.FullName);
                foreach (GameObject button in jsonButtons)
                {
                    Image image = button.GetComponent<Image>();
                    image.color = Color.white;
                }
                img.color = new Color(0.9764706f, 0.5921569f, 0.454902f, 1f);   // #F99774
            });
            jsonButtons.Add(newButton);
        }
    }

    // 노드 설치 윈도우의 버튼들을 지우고 새로 만듬. 자주 호출하면 좋지 않음.
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

            // Siren은 만들지 않음.
            if (nm is NodeSound) continue;

            newButton = Instantiate(sensorButton);
            newButton.SetActive(true);

            Transform panel = sensorPanel; // Default is sensorPanel
            if (nm is NodeArea) panel = areaPanel;
            if (nm is NodeExit) panel = exitPanel;
            newButton.transform.SetParent(panel, false);

            newButton.transform.localPosition = Vector3.zero;
            newButton.transform.GetChild(0).GetComponent<Text>().text = nm.DisplayName;
            newButton.GetComponent<Button>().onClick.AddListener(() => OnSelectNode(physicalID));
            sensorButtons.Add(nm.PhysicalID, newButton);
        }

        UpdateSensorButtonColor();
    }

    // .autoSave.json파일에 정보를 백업함.
    private object mutex = new object();
    private void AutoSave()
    {
        Debug.Log("AutoSave");
        string data = NodeManager.Jsonfy();
        new Thread(() =>
        {
            lock (mutex)
            {
                File.WriteAllText(autoSaveFilePath, data);
            }
        }).Start();
    }

    // 센서 상태 변화에 따라, 센서 버튼 색깔을 업데이트
    private void UpdateSensorButtonColor()
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

    // String으로부터 새로운 노드를 추가
    private bool AddNodeFromString(string sensorString)
    {
        string[] parsed = sensorString.Split(';');
        if (parsed.Length < 2) return false;

        string id = parsed[0].Trim();
        int typeNumber = int.Parse(parsed[1].Trim());

        Type type = Constants.GetNodeTypeFromNumber(typeNumber);
        if (type == null) return false;
        if (NodeManager.AddNode(id, type)) return true;
        return false;
    }

    // 센서 상태가 업데이트되었을 때의 콜백.
    private void OnSensorStateUpdated()
    {
        UpdateSensorButtonColor();
        AutoSave();
    }

    // Called when load json button of menu bar clicked
    public void OnLoadJson()
    {
        WindowManager loadJsonWindow = WindowManager.GetWindow("window_load_json");
        RenderSaveFileButtons();
        loadJsonWindow.SetVisible(true);
    }

    // Called when user select json file and click load button
    public void OnLoadJsonFile()
    {
        if (currentJsonFile == null)
        {
            Popup.Show("JSON파일을 선택해주세요.");
            return;
        }
        NodeManager.DestroyAll();
        NodeManager.InitiateFromFile(currentJsonFile.FullName);
        RenderNodeButtons();
        inputSaveFileName.text = Path.GetFileNameWithoutExtension(currentJsonFile.FullName);

        Popup.Show("센서 설정을 로드하였습니다.");
        WindowManager.GetWindow("window_load_json").SetVisible(false);

        currentJsonFile = null;
    }

    // 센서 수동 생성 윈도우에서 생성할 때 발생 - ToDo : 나중에 이름 Node로 고칠 것.
    public void OnSensorManuallyCreate(InputField text)
    {
        string data = text.text;
        if (AddNodeFromString(data)) Popup.Show("노드를 새로 추가하였습니다.");
        else Popup.Show("문제가 발생하여 노드를 새로 추가하지 못했습니다.");
        RenderNodeButtons();
    }

    // 탈출구 생성 버튼을 눌렀을 때 발생
    public void OnExitCreate()
    {
        string newExitID = "EXIT " + NodeManager.GetNodeIDs().Length;
        if (NodeManager.AddNode(newExitID, typeof(NodeExit))) Popup.Show("탈출구를 새로 추가하였습니다.");
        else Popup.Show("문제가 발생하여 탈출구를 새로 추가하지 못했습니다.");
        RenderNodeButtons();
    }

    // 노드 설치 윈도우에서 노드 설치했을 경우 발생. 굳이 함수로 안 꺼내고, Lambda로 집어넣어버리는 것도 고려할 것.
    public void OnSelectNode(string nodeID)
    {
        NodeManager node = NodeManager.GetNodeByID(nodeID);
        MouseManager.ToNodePlaceMode(node);
    }

    // 상단 메뉴의 저장 버튼을 눌렀을 때 발생
    public void OnSaveJson()
    {
        WindowManager loadJsonWindow = WindowManager.GetWindow("window_save_json");
        loadJsonWindow.SetVisible(true);
    }

    // 저장 윈도우에서 저장 버튼을 눌렀을 때 발생
    public void OnSaveJsonFile()
    {
        // 파일이름 확인
        string fileName = inputSaveFileName.text;
        if (fileName == null || fileName.Length < 1)
        {
            Popup.Show("파일 이름을 입력하지 않았습니다.");
            return;
        }

        // 노드 정보 저장
        string path = root + fileName + ".json";
        string data = NodeManager.Jsonfy();
        new Thread(() => File.WriteAllText(path, data)).Start();

        // 팝업 띄우고
        Popup.Show("저장되었습니다.");
        WindowManager.GetWindow("window_save_json").SetVisible(false);

        // 세이브파일 리스트 업데이트.
        UpdateSaveFileList();
    }

    // (임시) 'DB Load' 버튼을 눌렀을 때 발생
    public void OnLoadFromDBClicked()
    {
        if (LoadDataFromDB()) Popup.Show("데이터가 성공적으로 DB에서 로드되었습니다.");
        else Popup.Show("DB에서 데이터를 로드하는 데 실패했습니다.");
    }
}

class Headcounter
{
    public class HeadCountInfo
    {
        public int column;
        public int number;
        public float time;
    }

    private List<HeadCountInfo> data = new List<HeadCountInfo>();
    private List<int> columns = new List<int>();

    public void Init()
    {
        data.Clear();
    }

    public void AddData(int column, int number, float time)
    {
        HeadCountInfo hci = new HeadCountInfo();
        hci.column = column;
        hci.number = number;
        hci.time = time;
        data.Add(hci);

        if (!columns.Contains(column)) columns.Add(column);
    }

    public int[] GetColumns()
    {
        return columns.ToArray();
    }

    public HeadCountInfo[] GetData(int column)
    {
        List<HeadCountInfo> o = new List<HeadCountInfo>();
        foreach (HeadCountInfo hci in data)
        {
            if (hci.column == column) o.Add(hci);
        }
        return o.ToArray();
    }
}