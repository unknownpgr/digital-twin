using CoolSms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ScenarioManager : MonoBehaviour
{
    // Singleton obejct for static call
    public static ScenarioManager singleton;

    MQTTManager mQTTManager;

    // Current state.
    // true = at least one sensor is in disaster mode.
    // false = no sensor is in disaster mode.
    private bool currentDisasterState = false;

    // Check if disaster occurred at least once during simulation.
    private bool disasterOccurred = false;

    NavMeshPath navMeshPath;

    Camera subCamera;

    GameObject videoWindow;
    // VideManager of video window
    private VideoManager videoManager = null;

    // Elements of path window UI
    GameObject defaultPathPanel;
    Transform pathWindowcontent;

    // Element of disaster warning UI
    private GameObject warningBox;
    private Image warningBoxBg;
    private Image warningIcon;
    private Text disasterName;
    private Text nearestCameraID;

    public void Start()
    {
        // Get video manger
        videoManager = GameObject.Find("Master").GetComponent<VideoManager>();

        singleton = this;
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
        NavMeshTriangulation tri = NavMesh.CalculateTriangulation();

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

        // Initialize disaster state
        disasterOccurred = false;
    }

    public void EndSimulation()
    {
        NodeArea.ClearHeadCount();
        RouteRenderer.InitRenderer();

        // Reset area number
        foreach (var node in NodeManager.GetNodesByType<NodeArea>()) node.Num = 0;

        // Reset disaster sensors
        foreach (var node in NodeManager.GetNodesByType<NodeFireSensor>()) node.IsDisasterFire = node.IsDisasterSmoke = node.IsDisasterTemp = false;
        foreach (var node in NodeManager.GetNodesByType<NodeEarthquakeSensor>()) node.IsDisaster = false;
        foreach (var node in NodeManager.GetNodesByType<NodeFloodSensor>()) node.IsDisaster = false;

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

        currentDisasterState = false;
        disasterOccurred = false;
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

                List<NodeCCTV> nodeCCTVs = NodeManager.GetNodesByType<NodeCCTV>();
                if (nodeCCTVs != null)
                {
                    foreach (NodeCCTV nodeCCTV in nodeCCTVs)
                    {
                        if (nodeCCTV.Hide == false)
                        {
                            videoWindow.SetActive(true);
                            // Set text of video window to ID of nearest camera from disaster
                            SetTextOfViedoWindowWithDisasterState(true);
                            // Show video window
                            WindowManager.GetWindow("window_video").SetVisible(true);
                            // Set video clip
                            videoManager.SetVideoClipWithDisasterState(true);
                            // Play video
                            videoManager.PlayVideoClip();
                            break;
                        }
                    }
                }

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

        EvacuateRouter router = new EvacuateRouter();

        // Calculate path from every area to every exit
        foreach (NodeArea area in NodeManager.GetNodesByType<NodeArea>())
        {
            // Skip empty area
            if (area.Num <= 0)
            {
                Debug.Log("Skip empty area " + area.PhysicalID);
                continue;
            }

            // for targets
            foreach (NodeExit exit in NodeManager.GetNodesByType<NodeExit>())
            {
                // Skip inactivated node
                if (exit.Hide) continue;

                // Calculate shortest path
                navMeshPath = new NavMeshPath();
                NavMesh.CalculatePath(area.Position, exit.Position, -1, navMeshPath);
                Vector3[] path = navMeshPath.corners;

                // Add calculated route to router
                router.AddRoute(area, exit, path);
            }
        }

        // Calculate all senarios
        EvacuateRouter.EvacuateScenario[] allScenarios = router.CalculateScenarios();

        // Set direction sensors
        SetDirectionSensor(allScenarios[0]);

        // Get top 10 senarios
        EvacuateRouter.EvacuateScenario[] bestSenarios = new EvacuateRouter.EvacuateScenario[allScenarios.Length > 10 ? 10 : allScenarios.Length];
        Array.Copy(allScenarios, 0, bestSenarios, 0, bestSenarios.Length);

        RouteRenderer.Render(bestSenarios, () =>
        {
            // This callback function is called after all rendering finished.

            // Remove existing panels
            foreach (Transform child in pathWindowcontent) Destroy(child.gameObject);

            for (int i = 0; i < bestSenarios.Length; i++)
            {
                float time = bestSenarios[i].RequiredTime;
                Texture2D texture = bestSenarios[i].Screenshot;

                // Create new image
                GameObject newPathPanel = Instantiate(defaultPathPanel);
                Transform newPanelTransform = newPathPanel.transform;
                newPanelTransform.SetParent(pathWindowcontent, false);
                newPanelTransform.localPosition = Vector3.zero;

                // Set image
                Image evacpathImage = newPanelTransform.GetChild(0).GetComponent<Image>();
                evacpathImage.sprite = Sprite.Create(texture, new Rect(0, 0, Screen.width - 1, Screen.height - 1), new Vector2(0.5f, 0.5f));

                // Set rank text
                Text evacRankText = newPanelTransform.GetChild(1).GetComponentInChildren<Text>();
                evacRankText.text = (i + 1).ToString();

                // Set time
                Text evacTimeText = newPanelTransform.GetChild(2).GetComponentInChildren<Text>();
                evacTimeText.text = "예상 시간 : " + string.Format("{0:F2}", time) + "(초)";

                bestSenarios[i].Log();
            }

            List<string> phones = InformationManager.GetSavedPhoneNumbers();
            phones = null; // Comment here to send message;
            UploadImage(phones, bestSenarios[0].Screenshot);
        });
    }

    // Upload optimal path image on server and send sms
    private async void UploadImage(List<String> phones, Texture2D image)
    {
        string imageServer = Constants.IMAGE_SERVER;

        // To upload the image, construct the form object
        MultipartFormDataContent form = new MultipartFormDataContent();
        byte[] buf = image.EncodeToPNG();
        form.Add(new ByteArrayContent(buf), Constants.IMAGE_KEY, Constants.IMAGE_KEY);

        // Add disaster message data to form
        int floor = BuildingManager.GetFloor(GetNodesInDisaster()[0].Position) + 1;
        string message = "<strong>" + floor + "층</strong>에서 " + disasterName.text;
        form.Add(new StringContent(message, System.Text.Encoding.UTF8), "message");

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
        string url = imageServer + responseString;
        Debug.Log("Image viewer URL: " + url);

        if (phones == null)
        {
            Debug.Log("SMS sending canceled because the phone number is not provided.");
            return;
        }

        foreach (var phone in phones)
        {
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
    }

    //최적경로에 따른 대피유도신호로 바꾸기
    private void SetDirectionSensor(EvacuateRouter.EvacuateScenario optimalScenario)
    {
        foreach (NodeDirection node in NodeManager.GetNodesByType<NodeDirection>())
        {
            // target = nearest path position
            Vector3 target = GetNearestPathPoint(node.Position, optimalScenario);

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
    private Vector3 GetNearestPathPoint(Vector3 origin, EvacuateRouter.EvacuateScenario optimalScenario)
    {
        float minDistance = float.MaxValue;
        Vector3 target = Vector3.zero;
        foreach (Vector3[] route in optimalScenario.Routes)
        {
            float distance = Vector3.Distance(origin, route[0]);
            if (distance < minDistance)
            {
                minDistance = distance;
                target = route[route.Length - 1];
            }
        }
        return target;
    }

    // return = up(z), right(x), down(-z), left(-x)
    private string VectorToDirection(Vector3 dir)
    {
        string ret;
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

