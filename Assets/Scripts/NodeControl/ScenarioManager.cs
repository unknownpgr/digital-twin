using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Linq;
using System;
using System.IO;
using CoolSms;
using System.Net.Http;

public class ScenarioManager : MonoBehaviour
{
    // Singleton obejct for static call
    public static ScenarioManager singleTon;

    MQTTManager mQTTManager;
    public Grid3 grid;

    // Current state.
    // true = at least one sensor is in disaster mode.
    // false = no sensor is in disaster mode.
    private bool currentDisasterState = false;

    // Check if disaster occurred at least once during simulation.
    private bool disasterOccurred = false;

    NavMeshPath navMeshPath;
    public SimulationManager3 simulationManager = null;
    int pathSize = 0;
    public bool isSimulating = false;

    Camera subCamera;

    GameObject videoWindow;
    // VideManager of video window
    private VideoManager videoManager = null;

    // Elements of path window UI
    GameObject defaultPathPanel;
    Transform pathWindowcontent;

    // Element of disaster warning UI
    private GameObject warningBox;
    private UnityEngine.UI.Image warningBoxBg;
    private UnityEngine.UI.Image warningIcon;
    private Text disasterName;
    private Text nearestCameraID;
    private List<ScreenshotAttr> pathImages = new List<ScreenshotAttr>();

    public void Start()
    {
        // Get video manger
        videoManager = GameObject.Find("Master").GetComponent<VideoManager>();

        singleTon = this;
        StartCoroutine(PeriodicCheck());
    }

    public void InitSimulation()
    {
        //Camera initiation
        if (subCamera == null) subCamera = GameObject.Find("SubCamera").GetComponent<Camera>();
        Vector3 buildingSize = BuildingManager.BuildingBound.size;
        float cameraViewSize = Mathf.Max(buildingSize.x, buildingSize.z) / 2;
        subCamera.orthographicSize = cameraViewSize;

        Vector3 cameraPosition = BuildingManager.BuildingBound.center;
        cameraPosition.y = 100;
        subCamera.transform.position = cameraPosition;

        // << BLOCKING TASK 1 >> Make new navgrid
        if (grid == null) grid = transform.GetComponent<Grid3>();
        NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
        grid.CreateGrid(tri.vertices, BuildingManager.FloorsCount);

        if (mQTTManager == null) mQTTManager = GetComponent<MQTTManager>();
        mQTTManager.Init();

        // Register node update callback listener
        MQTTManager.OnNodeUpdated = OnNodeUpdated;

        // Get exsisting path panel and transform of parent
        defaultPathPanel = GameObject.Find("panel_path");
        pathWindowcontent = GameObject.Find("window_path_content").transform;

        // Get text of disaster warning UI
        warningBox = FunctionManager.Find("warning_box").gameObject;
        warningBoxBg = warningBox.GetComponent<UnityEngine.UI.Image>();
        warningIcon = warningBox.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
        disasterName = warningBox.transform.GetChild(1).GetComponent<Text>();
        SetWarningBoxWithDisasterState(false);

        videoWindow = FunctionManager.Find("window_video").gameObject;
        nearestCameraID = videoWindow.transform.GetChild(1).GetChild(2).GetComponent<Text>();
        SetTextOfViedoWindowWithDisasterState(false);

        // << BLOCKING TASK 2 >> Add sgrid
        simulationManager = ScriptableObject.CreateInstance<SimulationManager3>();
        simulationManager.SetGrid(grid);

        // Initialize disaster state
        disasterOccurred = false;
    }

    public void EndSimulation()
    {
        // Reset path
        grid.ResetFinalPaths();
        grid.InitWeight();
        grid.ViewMinPath = false;
        grid.InitLiner();

        // Clear simulationManager
        simulationManager.EvacuatersList.Clear();

        // Reset area number
        foreach (NodeArea node in NodeManager.GetNodesByType<NodeArea>()) node.Num = 0;
        foreach (NodeFireSensor node in NodeManager.GetNodesByType<NodeFireSensor>()) node.IsDisasterFire = node.IsDisasterSmoke = node.IsDisasterTemp = false;
        foreach (NodeDirection node in NodeManager.GetNodesByType<NodeDirection>())
        {
            node.Direction = "off";
            mQTTManager.PubDirectionOperation(node.PhysicalID, "off");
        }
        // If a disaster has occurred at least once, reset the sensor. else, skip this process.
        if (disasterOccurred)
        {
            // Turn off all siren and all direction sensors
            SetSiren(false);
        }

        // Close mqttManager
        mQTTManager?.Close();
    }

    private void OnNodeUpdated(MQTTManager.MQTTMsgData data)
    {
        // Ignore wrong MQTT data
        if (NodeManager.GetNodeByID(data.PhysicalID) == null)
        {
            Debug.Log("Unknown node : " + data.PhysicalID);
            return;
        }

        // false = every node is not in disaster mode
        // true = at least one node is in disaster mode
        NodeManager node = NodeManager.GetNodeByID(data.PhysicalID);

        // Apply changes on node and check current disaster state.
        // in case of fire sensor
        if (node is NodeFireSensor nodeFire)
        {
            switch (data.sensorType)
            {
                case Constants.NODE_SENSOR_TEMP:
                    nodeFire.IsDisasterTemp = data.IsDisaster;
                    nodeFire.ValueTemp = data.Value;
                    break;

                case Constants.NODE_SENSOR_FIRE:
                    nodeFire.IsDisasterFire = data.IsDisaster;
                    nodeFire.ValueFire = data.Value;
                    break;

                case Constants.NODE_SENSOR_SMOKE:
                    nodeFire.IsDisasterSmoke = data.IsDisaster;
                    nodeFire.ValueSmoke = data.Value;
                    break;
            }

            Debug.Log(nodeFire.ValueTemp);

            if (nodeFire.IsDisaster) disasterName.text = "화재 발생";
        }

        // in case of earthquake sensor
        else if (node is NodeEarthquakeSensor nodeEarthquake)
        {
            nodeEarthquake.IsDisaster = data.IsDisaster;
            nodeEarthquake.ValueEarthquake = data.Value;

            //Debug.Log("지진 센서: " + nodeEarthquake.ValueEarthquake);

            if (nodeEarthquake.IsDisaster) disasterName.text = "지진 발생";
        }

        // in case of flood sensor
        else if (node is NodeFloodSensor nodeFlood)
        {
            nodeFlood.IsDisaster = data.IsDisaster;
            nodeFlood.ValueFlood = data.Value;

            //Debug.Log("풍수해 센서: " + nodeFlood.ValueFlood);

            if (nodeFlood.IsDisaster) disasterName.text = "풍수해 발생";
        }

        // Update node
        else if (node is NodeArea nodeArea)
        {
            if (nodeArea.Num != data.Value)
            {
                nodeArea.Num = (int)data.Value;
                // Check if number of people in area changed.
                // isAreaChanged는 인원수가 0인지 아닌지 변했을 때에만 trigger된다.
            }
        }

        // Set direction sensor
        else if (node is NodeDirection nodeDirection)
        {
            nodeDirection.Direction = data.Direction;
        }
    }

    private IEnumerator PeriodicCheck()
    {
        // Infinite loop with counter
        for (int i = 0; ; i++)
        {
            // Get new disaster state. Default is false
            bool newDisasterState = false;

            // If any fire sensor is active, it is disaster.
            foreach (NodeFireSensor nodeFire in NodeManager.GetNodesByType<NodeFireSensor>())
            {
                newDisasterState |= nodeFire.IsDisaster;
            }

            // in case of earthquake sensor
            foreach (NodeEarthquakeSensor nodeEarthquake in NodeManager.GetNodesByType<NodeEarthquakeSensor>())
            {
                newDisasterState |= nodeEarthquake.IsDisaster;
            }

            // in case of floode sensor
            foreach (NodeFloodSensor nodeFlood in NodeManager.GetNodesByType<NodeFloodSensor>())
            {
                newDisasterState |= nodeFlood.IsDisaster;
            }

            // But if there are nobody in the building, it is not disaster.
            int headTotalCount = 0;
            foreach (NodeArea node in NodeManager.GetNodesByType<NodeArea>())
            {
                headTotalCount += node.Num;
            }
            if (headTotalCount == 0) newDisasterState = false;

            // Disaster started(false->true) or during disaster
            if (newDisasterState & (!currentDisasterState || (i % Constants.PERIODIC_CHECK_TIME == 0)))
            {
                // Start siren, show path window, activate warning box.
                SetSiren(true);
                WindowManager.GetWindow("window_path").SetVisible(true);
                SetWarningBoxWithDisasterState(true);
                warningBox.SetActive(true);
                videoWindow.SetActive(true);

                // Set text of video window to ID of nearest camera from disaster
                SetTextOfViedoWindowWithDisasterState(true);
                // Show video window
                WindowManager.GetWindow("window_video").SetVisible(true);
                // Set video clip
                videoManager.SetVideoClipWithDisasterState(true);
                // Play video
                videoManager.PlayVideoClip();

                // Set all floor visible and start simulation.
                FunctionManager.SetFloorVisibility(int.MaxValue);
                StartCoroutine(StartSimulation());
                StartCoroutine(SetWarningTextOpacity());

                // Set disaster occurred flag
                disasterOccurred = true;
            }

            // Disaster is finished(true->false)
            if (currentDisasterState & !newDisasterState)
            {
                // Turn off siren
                SetSiren(false);

                // Init grid(should make this process simpler)
                grid.InitWeight();
                grid.ViewMinPath = false;
                grid.InitLiner();

                // Set warning box
                SetWarningBoxWithDisasterState(false);

                // Set text of video window
                SetTextOfViedoWindowWithDisasterState(false);

                // Set video clip
                videoManager.SetVideoClipWithDisasterState(false);
                // Play video
                videoManager.PlayVideoClip();
                // Hide video window
                WindowManager.GetWindow("window_video").SetVisible(false);
            }

            currentDisasterState = newDisasterState;

            yield return new WaitForSeconds(1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isSimulating)
        {
            if (simulationManager.EvacuatersList.Count > 0)
            {
                Debug.Log("SIM...");
                if (simulationManager.Progress())
                {
                    // 모든 경로에 대해 시뮬레이션이 완료됨.
                    grid.InitWeight();
                    grid.ViewMinPath = true;
                    grid.Liner();
                    isSimulating = false;
                    simulationManager.PrintOut("");
                    SetDirectionSensor();
                }
                StartCoroutine(ScreenShot(simulationManager.delayList[simulationManager.delayList.Count - 1]));
            }
        }
    }

    private AudioSource sirenPlayer;
    public void SetSiren(bool play)
    {
        // If there are no player, make one.
        if (sirenPlayer == null) sirenPlayer = gameObject.GetComponent<AudioSource>();

        // Publish siren signal only when state changes.
        if ((!sirenPlayer.isPlaying) && play)
        {
            mQTTManager.PubSiren(true);
            sirenPlayer.Play();
        }
        else if (sirenPlayer.isPlaying && (!play))
        {
            mQTTManager.PubSiren(false);
            sirenPlayer.Stop();
        }
    }

    private IEnumerator StartSimulation()
    {
        List<NodeExit> exits = new List<NodeExit>();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Init simulation manager
        simulationManager = ScriptableObject.CreateInstance<SimulationManager3>();
        simulationManager.SetGrid(grid);
        simulationManager.EvacuatersList.Clear();
        grid.ResetFinalPaths();
        grid.ViewMinPath = false;
        exits.Clear();

        float height = -1;
        foreach (NodeExit exit in NodeManager.GetNodesByType<NodeExit>())
        {
            //아래로
            if (disasterName.text == "화재 발생" || disasterName.text == "지진 발생")
            {
                if (height == -1)
                {
                    height = exit.Position.y;
                    exits.Add(exit);
                }
                if (height > exit.Position.y + 1.0)
                {
                    foreach (NodeExit temp in exits) temp.Hide = true;
                    height = exit.Position.y;
                    exits.Clear();
                    exits.Add(exit);
                }
                else if (height < exit.Position.y + 1.0 && height > exit.Position.y - 1.0) exits.Add(exit);
                else if (height < exit.Position.y - 1.0)
                {
                    exit.Hide = true;
                }
            }
            //위로
            else if (disasterName.text == "풍수해 발생")
            {
                if (height == -1)
                {
                    height = exit.Position.y;
                    exits.Add(exit);
                }
                if (height < exit.Position.y - 1.0)
                {
                    foreach (NodeExit temp in exits) temp.Hide = true;
                    height = exit.Position.y;
                    exits.Clear();
                    exits.Add(exit);
                }
                else if (height < exit.Position.y + 1.0 && height > exit.Position.y - 1.0) exits.Add(exit);
                else if (height > exit.Position.y + 1.0)
                {
                    exit.Hide = true;
                }
            }
        }

        // Store paths
        foreach (NodeArea area in NodeManager.GetNodesByType<NodeArea>())
        {
            // Pass empty area
            if (area.Num <= 0) continue;

            //simulationManager.SetEvacuaters(this.areaJsons, this.areaNums);
            List<Node3[]> paths = new List<Node3[]>();

            // for targets
            foreach (NodeExit exit in NodeManager.GetNodesByType<NodeExit>())
            {
                // Except inactivated node
                if (exit.Hide) continue;

                // Calculate path
                List<Node3> path = new List<Node3>();
                navMeshPath = new NavMeshPath();

                // Calculate path from every area to every exit
                NavMesh.CalculatePath(area.Position, exit.Position, -1, navMeshPath);
                for (int o = 0; o < navMeshPath.corners.Length - 1; o++)
                {
                    path.AddRange(grid.GetNodesFromLine(navMeshPath.corners[o], navMeshPath.corners[o + 1]));
                }

                // Remove duplicated location
                for (int o = 0; o < path.Count - 1; o++)
                {
                    if (path[o] == path[o + 1])
                    {
                        while (path[o] == path[o + 1])
                            path.Remove(path[o + 1]);
                    }
                }

                // Add to paths
                if (path.Count > 0) paths.Add(path.ToArray());
            }

            pathSize += paths.Count;
            simulationManager.AddEvacuater(area.Position, area.Num, paths, area.Velocity);
        }

        // Now, path calculating finished.
        simulationManager.InitSimParam(pathSize);
        pathImages.Clear();

        // Start simulating
        isSimulating = true;
    }

    private IEnumerator ScreenShot(float time)
    {
        // Remove existing panels
        foreach (Transform child in pathWindowcontent.transform)
        {
            DestroyImmediate(child.gameObject);
        }
        ScreenshotAttr screenShotData = new ScreenshotAttr
        {
            time = time
        };

        // Take picture
        subCamera.enabled = true;
        yield return new WaitForEndOfFrame();
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        subCamera.targetTexture = rt;
        subCamera.Render();
        subCamera.enabled = false;
        RenderTexture.active = rt;

        Texture2D screenShotTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
        screenShotTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        screenShotTexture.Apply();

        byte[] bytes = screenShotTexture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/Resources/최적경로.png", bytes);

        GameObject phoneNumber = GameObject.Find("inputField_user_phone_number");

        screenShotData.scrshot = screenShotTexture;
        pathImages.Add(screenShotData);

        // If image is full
        if (pathSize == pathImages.Count)
        {
            pathImages.Sort(delegate (ScreenshotAttr x, ScreenshotAttr y)
            {
                if (x.time > y.time) return 1;
                else if (x.time < y.time) return -1;
                return 0;
            });

            /*
			// Remove existing panels
			foreach (Transform child in pathWindowcontent.transform)
			{
                DestroyImmediate(child.gameObject);
			}*/

            // Show top-10 fastest pathes on panel
            int listCount = Math.Min(pathImages.Count, 10);
            for (int r = 0; r < listCount; r++)
            {
                // Create new image
                GameObject newPathPanel = Instantiate(defaultPathPanel);
                Transform newPanelTransform = newPathPanel.transform;
                newPanelTransform.SetParent(pathWindowcontent, false);
                newPanelTransform.localPosition = Vector3.zero;

                // Set image
                UnityEngine.UI.Image evacpathImage = newPanelTransform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
                evacpathImage.sprite = Sprite.Create(pathImages[r].scrshot, new Rect(0, 0, Screen.width - 1, Screen.height - 1), new Vector2(0.5f, 0.5f));

                // Set rank text
                Text evacRankText = newPanelTransform.GetChild(1).GetComponentInChildren<Text>();
                evacRankText.text = (r + 1).ToString();

                // Set time
                Text evacTimeText = newPanelTransform.GetChild(2).GetComponentInChildren<Text>();
                evacTimeText.text = "예상 시간 : " + string.Format("{0:F2}", pathImages[r].time) + "(초)";
                // evacTimeText.text = "Time : " + pathImages[r].time.ToString() + "(초)";
            }

            foreach (string phone in InformationManager.GetSavedPhoneNumbers())
            {
                // ToDo : DO NOT CALL UploadImage MULTIPLE TIMES.
                // Move it out of loop. Instead of passing each phone number to method, pass whole list.
                string _phone = phone;
                _phone = null; // Comment here to send message
                UploadImage(_phone);
            }
        }
    }

    // Upload optimal path image on server and send sms
    private async void UploadImage(string phone)
    {
        string imageServer = Constants.IMAGE_SERVER;

        // To upload the image, construct the form object
        MultipartFormDataContent form = new MultipartFormDataContent();
        using (FileStream fs = new FileStream(Application.dataPath + "/Resources/최적경로.png", FileMode.Open))
        {
            int len = (int)fs.Length;
            byte[] buf = new byte[len];
            fs.Read(buf, 0, len);
            form.Add(new ByteArrayContent(buf), Constants.IMAGE_KEY, Constants.IMAGE_KEY);
        }

        // Upload the image and get the url of the uploaded image 
        HttpResponseMessage response = null;
        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                response = await httpClient.PostAsync(imageServer + "/upload", form);
                response.EnsureSuccessStatusCode();
            }
        }
        catch
        {
            Debug.LogError("Could not upload image on server.");
            return;
        }

        // Parse response and get url. Response is a json string like {"img":"IMAGE_KEY"}
        string responseString = response.Content.ReadAsStringAsync().Result;
        string url = imageServer + "/?img=" + responseString
            .Split(':')[1]
            .Replace("}", "")
            .Replace("\"", "");
        Debug.Log("Image viewer URL: " + url);

        if (phone == null)
        {
            Debug.Log("SMS sending canceled because the phone number is not provided.");
            return;
        }

        // Send message
        SmsApi api = new SmsApi(new SmsApiOptions
        {
            ApiKey = "NCSP8ABD0A2GUNI5", // 발급 받은 ApiKey
            ApiSecret = "FCWNJZVBLK5EFP7LFVRLHQISHOK6YJSD", // 발급받은 ApiSecret key
            DefaultSenderId = "01026206621" // 문자 보내는 사람 폰 번호
        });
        var request = new SendMessageRequest(phone, "화재 발생! - " + url); // 이미지 링크가 포함된 문자 메세지 전송
        var result = api.SendMessageAsync(request);

        // Log result
        Debug.Log("메시지 전송완료 : " + result);
    }

    //최적경로에 따른 대피유도신호로 바꾸기
    private void SetDirectionSensor()
    {
        foreach (NodeDirection node in NodeManager.GetNodesByType<NodeDirection>())
        {
            // target = nearest path position
            Vector3 target = GetNearestPathPoint(node.Position);

            // Calculate direction
            NavMeshPath p = new NavMeshPath();
            NavMesh.CalculatePath(node.Position, target, -1, p);
            if (p.status == NavMeshPathStatus.PathComplete)
            {
                mQTTManager.PubDirectionOperation(node.PhysicalID, VectorToDirection(p.corners[1] - p.corners[0]));
            }
        }
    }

    // 가장 가까운 경로 위치를 가져옴.
    private Vector3 GetNearestPathPoint(Vector3 origin)
    {
        float minDistance = float.MaxValue;
        Vector3 target = Vector3.zero;
        foreach (Node3[] node in grid.MinPaths)
        {
            float tmp = Vector3.Distance(origin, node[0].position);
            if (tmp < minDistance)
            {
                minDistance = tmp;
                target = node.Last().position;
            }
        }
        return target;
    }

    // return = up(z), right(x), down(-z), left(-x)
    private string VectorToDirection(Vector3 dir)
    {
        string ret = "up";
        Vector3 tmp = dir.normalized;
        if (tmp.z < tmp.x)
        {
            if (tmp.z > -tmp.x) ret = "right";
            else ret = "down";
        }
        else
        {
            if (tmp.z > -tmp.x) ret = "up";
            else ret = "left";
        }
        return ret;
    }

    private bool isRunning = false;
    private IEnumerator SetWarningTextOpacity()
    {
        if (!isRunning)
        {
            isRunning = true;
            warningIcon.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            disasterName.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            yield return new WaitForSeconds(0.8f);
            while (currentDisasterState) // Please avoid using indirect stat flag. (warningBox.activeSelf)
            {
                warningIcon.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                disasterName.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                yield return new WaitForSeconds(0.6f);

                warningIcon.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                disasterName.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                yield return new WaitForSeconds(0.6f);
            }
            isRunning = false;
        }
    }

    private void SetWarningBoxWithDisasterState(bool IsDisaster)
    {
        if (IsDisaster == true)
        {
            // Set background color to red
            warningBoxBg.color = new Color(1.0f, 0.2648465f, 0.1037736f, 1.0f);
        }
        else if (IsDisaster == false)
        {
            // Set background color to green
            warningBoxBg.color = new Color(0.2707055f, 0.8773585f, 0.240032f, 1.0f);
            disasterName.text = "상태 안전";
        }
    }

    private List<NodeManager> GetNodesInDisaster()
    {
        List<NodeManager> nodesInDisaster = new List<NodeManager>();

        foreach (NodeFireSensor nodeFireSensor in NodeManager.GetNodesByType<NodeFireSensor>())
        {
            if (nodeFireSensor.IsDisaster == true)
            {
                nodesInDisaster.Add(nodeFireSensor);
            }
        }

        foreach (NodeEarthquakeSensor nodeEarthquakeSensor in NodeManager.GetNodesByType<NodeEarthquakeSensor>())
        {
            if (nodeEarthquakeSensor.IsDisaster == true)
            {
                nodesInDisaster.Add(nodeEarthquakeSensor);
            }
        }

        foreach (NodeFloodSensor nodeFloodSensor in NodeManager.GetNodesByType<NodeFloodSensor>())
        {
            if (nodeFloodSensor.IsDisaster == true)
            {
                nodesInDisaster.Add(nodeFloodSensor);
            }
        }

        return nodesInDisaster;
    }

    private string GetNearestCameraID()
    {
        string cameraID = "재난 현장에 가까운 CCTV가 없습니다.";
        float minDist = float.MaxValue;

        foreach (NodeCCTV nodeCCTV in NodeManager.GetNodesByType<NodeCCTV>())
        {
            foreach (NodeManager nodeManager in GetNodesInDisaster())
            {
                if (BuildingManager.GetFloor(nodeCCTV.Position) == BuildingManager.GetFloor(nodeManager.Position))
                {
                    float cctvDist = Vector3.Distance(nodeCCTV.Position, nodeManager.Position);
                    if (cctvDist < minDist)
                    {
                        minDist = cctvDist;
                        cameraID = nodeCCTV.PhysicalID;
                    }
                }
            }
        }
        return cameraID;
    }

    private void SetTextOfViedoWindowWithDisasterState(bool isDisaster)
    {
        nearestCameraID.text = "재난 발생 현장에서 가장 가까운 CCTV ID" + "\n";

        if (isDisaster == true)
        {
            nearestCameraID.text += GetNearestCameraID();
        }
        else
        {
            nearestCameraID.text += "현재 발생한 재난이 없습니다.";
        }
    }
}

