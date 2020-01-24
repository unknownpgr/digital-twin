using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using LitJson;

public class MQTTManager3: MonoBehaviour
{
    MqttClient client;
    MQTTClass MQTTConf;
    JsonParser jsonParser = new JsonParser();
    ScenarioManager3 scenarioManager;
    List<EvacuaterNodeJson> evacuaterNodeJsons;
    List<SensorNodeJson> sensorNodeJsons;
    List<ExitNodeJson> exitNodeJsons;
    List<AreaPositions> areaJsons;
    Dictionary<int, int> areaNums;
    GameObject MQTTWindow;
    Transform[] MQTTWindowTransforms;
    public InputField GUIText;
    public int msgID = 0;
    private string[] MessageDataElement = new string[] { "messageId", "nodeId", "timestamp", "sensorType", "value","value1","value2", "areaId" };
    public void Awake()
    {
        MQTTWindow = GameObject.Find("MQTTWindow");
        MQTTWindowTransforms = MQTTWindow.GetComponentsInChildren<Transform>();
        MQTTConf = jsonParser.Load<MQTTClass>("mqttconf");
    }
    public void Init()
    {
        
        if (MQTTConf.brokerAddr != null && MQTTConf.userName != null && MQTTConf.password != null)
        {
            MQTTConf.clientId = Guid.NewGuid().ToString();
            Debug.Log("Connecting ... " + MQTTConf.brokerAddr + ", " + MQTTConf.clientId);
            Connect();
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            client.ConnectionClosed += Client_ConnectionClosed;
            byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE };
            foreach(var t in MQTTConf.topic)
            {
                client.Subscribe(new string[] { t }, qosLevels);
            }
        }
    }

    private void Client_ConnectionClosed(object sender, EventArgs e)
    {
        client.Connect(MQTTConf.clientId);
        //Debug.Log("Connection closed...");
    }

    public void SetLists(ScenarioManager3 sm)
    {
        scenarioManager = sm;
    }
    public void SetLists(List<EvacuaterNodeJson> evacuaterNodeJsons, List<SensorNodeJson> sensorNodeJsons,
        List<ExitNodeJson> exitNodeJsons, List<AreaPositions> areaJsons, Dictionary<int, int> areaNums)
    {
        this.evacuaterNodeJsons = evacuaterNodeJsons;
        this.sensorNodeJsons = sensorNodeJsons;
        this.exitNodeJsons = exitNodeJsons;
        this.areaJsons = areaJsons;
        this.areaNums = areaNums;
    }

    private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string msg = System.Text.Encoding.UTF8.GetString(e.Message);
        Debug.Log("Received msg from " + e.Topic + " : " + msg);
        // Casing msg with topic!
        // for DEMO, sim start
        //scenarioManager.isSimulating = true;
        //scenarioManager.expName = Jsoning(msg).messageId;

        // 시뮬레이션 실험을 위한 메시지로, 대피자의 속도를 지정할 수 있음.
        //MessageData md = Jsoning(msg);
        //scenarioManager.areaNums[md.areaId] = (int)md.value;
        //scenarioManager.areaVelos[md.areaId] = (float)md.value2;
        //scenarioManager.isAreaChanged = true;

        // Test-Target
        //MessageData md = Jsoning(msg);
        //scenarioManager.SetTargetNodes(null, (int)md.value);


        if (e.Topic == MQTTConf.topic[0])
        {
            //DT/PeriodChange
            Debug.Log(msg);
        }
        else if (e.Topic == MQTTConf.topic[1])
        {
            //"mws/Notification/Periodic/SensingValueEvent", wan
            MessageData md = Jsoning(msg);
            //scenarioManager.sensorNodeJsons[md.nodeId].value1 = md.value;
            if (md.nodeId != null)
                if (scenarioManager.SensorDictionary.ContainsKey(md.nodeId))
                {
                    scenarioManager.SensorDictionary[md.nodeId].sensor_Attribute.one_sensor.value1 = md.value;
                    scenarioManager.SensorDictionary[md.nodeId].sensor_Attribute.one_sensor.disaster = false;

                    //scenarioManager.sensorNodeJsons[md.nodeId].nodeType = md.sensorType;
                    //scenarioManager.TestSetSensorValue(md.nodeId, md.value, md.sensorType);
                    scenarioManager.isSensorUpdated = true;
                }
        }
        else if (e.Topic == MQTTConf.topic[2])
        {
            //"mws/Notification/Periodic/EvacueeEvent", wan
            MessageData md = Jsoning(msg);
            if (scenarioManager.areaNums.ContainsKey(md.areaId))
                if (scenarioManager.areaNums[md.areaId] != (int)md.value)
                {
                    scenarioManager.areaNums[md.areaId] = (int)md.value;
                    scenarioManager.isAreaChanged = true;
                }
        }
        else if (e.Topic == MQTTConf.topic[3])//ScenarioManager3에서 말고 여기서 unity system에 방향지시등 모양 시각화 표시하도록
        {
            //"mws/Set/Direction": 대피명령
            /*
             {
             "messageID" : 42, 
             "nodeID" : 15, 
             "argument" : {
             "direction" : "left",
             "duration" : "10"
             }    
            */

            DirectionOperation d = JsonMapper.ToObject<DirectionOperation>(msg);
            Debug.Log(d);
            
            if (scenarioManager.SensorDictionary.ContainsKey(d.nodeId))
            {
                scenarioManager.SensorDictionary[d.nodeId].sensor_Attribute.one_sensor.value1 =
                    d.GetDirection();
            }
            scenarioManager.isSensorUpdated = true;
        }
        else if (e.Topic == MQTTConf.topic[4])
        {
            //"DT/Request/SensorData",  :: DEPRECATED

            
        }
        else if (e.Topic == MQTTConf.topic[5])
        {
            //"mws/Request/Area",

            
        }
        else if (e.Topic.Contains("mws/Response/Area/"))
        {
            // "mws/Response/Area/#"
            
            MessageData md = Jsoning(msg);
            if (scenarioManager.areaNums.ContainsKey(md.areaId))
                if (scenarioManager.areaNums[md.areaId] != (int)md.value)
                {
                    scenarioManager.areaNums[md.areaId] = (int)md.value;
                    scenarioManager.isAreaChanged = true;
                }

        }
        else if (e.Topic == MQTTConf.topic[7])
        {
            //"DT/UpdateSensorPosition", :: DEPRECATED

        }
        else if (e.Topic == MQTTConf.topic[8])
        {
            //mws/Notification/Periodic/DisasterEvent
            MessageData md = Jsoning(msg);
            //scenarioManager.sensorNodeJsons[md.nodeId].value1 = md.value;
            if (md.nodeId != null)
                if (scenarioManager.SensorDictionary.ContainsKey(md.nodeId))
                    {
                        scenarioManager.SensorDictionary[md.nodeId].sensor_Attribute.one_sensor.value1 = md.value;
                        scenarioManager.SensorDictionary[md.nodeId].sensor_Attribute.one_sensor.disaster = true;
                        scenarioManager.isSensorUpdated = true;
                    }
        }
        else if (e.Topic == "messageID")
        {
            MessageData md = Jsoning(msg);
            this.msgID = (int)(md.value);
        }
    }

    public void Publish(string _topic, string msg)
    {
        client.Publish(_topic, Encoding.UTF8.GetBytes(msg),
            MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
    }

    public void PubPeriod(int value)
    {
        string msg = "{";
        msg += "'timestamp':'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',";
        msg += "'value':'" + value.ToString() + "'}";
        Publish("DT/PeriodChange", msg);
    }
    public void PubDirectionOperation(string nodeId, string dir)//{"nodeId":"4","direction":"down"}MQTT신호 보내기
    {
        DirectionOperation d = new DirectionOperation();
        d.nodeId = nodeId;
        d.direction = dir;
        string jd = JsonMapper.ToJson(d);
        Publish(MQTTConf.topic[3], jd);
    }
    public void PubAreaUpdate(Text areaId)
    {
        if (scenarioManager != null)
        {
            string msg = "{'areaId':'" + areaId.text + "'}";
            Publish(MQTTConf.topic[5], msg);
        }
    }
    private void Connect()
    {
        client = new MqttClient(MQTTConf.brokerAddr);
        try
        {
            //client.Connect(MQTTConf.clientId);
            client.Connect(MQTTConf.clientId, MQTTConf.userName, MQTTConf.password, false, 0);
        }
        catch (Exception e)
        {
            Debug.LogError("Connnection error: " + e);
        }
        
    }

    MessageData Jsoning(string msg)
    {
        MessageData ret = new MessageData();
        msg = msg.Trim();
        msg = msg.Substring(1, msg.Length - 2);
        string[] msgs = msg.Split(',');
        for (int i = 0; i < msgs.Length; i++)
        {
            string[] tmp = msgs[i].Split(':');
            for (int j = 0; j < 2; j++)
            {
                tmp[j] = tmp[j].Replace("'", "");
                tmp[j] = tmp[j].Replace('"'.ToString(), "");
                tmp[j] = tmp[j].Trim();
            }
            ret.Set(tmp[0], tmp[1]);
        }
        return ret;
    }

    // GUI
    public void GUI()
    {
        if (GUIText != null) GUIText.text = MQTTConf.ToString();
        if (MQTTWindowTransforms[0].gameObject.activeSelf)
        {
            MQTTWindow.SetActive(false);
            for (int i = 0; i < MQTTWindowTransforms.Length; i++)
                MQTTWindowTransforms[i].gameObject.SetActive(false);
        }
        else
        {
            MQTTWindow.SetActive(true);
            for (int i = 0; i < MQTTWindowTransforms.Length; i++)
                MQTTWindowTransforms[i].gameObject.SetActive(true);
        }

    }

    public void SetGUI()
    {
        Debug.Log("test");
        MQTTConf.Set(GUIText.text);
        jsonParser.Save(MQTTConf, "mqttconf");
    }
    public void Close()
    {
        if (client.IsConnected)
            client.Disconnect();
    }
}

[SerializeField]
class MQTTClass
{
    public string clientId = "testClient";
    public string brokerAddr = "13.124.77.110";
    public int brokerPort = 8883;
    public string userName = "testId";
    public string password = "test";
    public string[] topic;

    override public string ToString()
    {
        string ret = "";
        ret += "Client ID: " + clientId + "\n";
        ret += "broker Addr: " + brokerAddr + "\n";
        ret += "broker Port: " + brokerPort + "\n";
        ret += "userName: " + userName + "\n";
        ret += "password: " + password + "\n";
        ret += "topics: \n";
        for (int i = 0; i < topic.Length; i++)
            ret += "  " + topic[i] + "\n";
        return ret;
    }
    public void Set(string _str)
    {
        string[] str = _str.Split('\n');
        Debug.Log(str[0]);
        this.clientId = str[0].Split(':')[1].Trim();
        this.brokerAddr = str[1].Split(':')[1].Trim();
        this.brokerPort = int.Parse(str[2].Split(':')[1].Trim());
        this.userName = str[3].Split(':')[1].Trim();
        this.password = str[4].Split(':')[1].Trim();
        List<string> tmp = new List<string>();
        for (int i = 6; i < str.Length; i++)
            tmp.Add(str[i].Trim());
        this.topic = tmp.ToArray();
        Debug.Log(this.ToString());
    }
}

[SerializeField]
class MessageData
{
    public string messageId;
    public string nodeId;
    public string timestamp;
    public int sensorType;
    public float value;
    public float value1;
    public float value2;
    public string areaId;
    public void Set(string key, string value)
    {
        switch (key)
        {
            case "messageId":
                messageId = value;
                break;
            case "nodeId":
                nodeId = (value);
                break;
            case "timestamp":
                timestamp = value;
                break;
            case "sensorType":
                sensorType = int.Parse(value);
                break;
            case "value":
                this.value = float.Parse(value);
                break;
            case "value1":
                this.value1 = float.Parse(value);
                break;
            case "value2":
                this.value2 = float.Parse(value);
                break;
            case "areaId":
                areaId = (value);
                break;
        }
    }
}

class DirectionOperation
{
    /*
             {
             "nodeID" : 15, 
             "direction" : "left"
               }
            */
    //public int messageID { get; set; }
    public string nodeId { get; set; }
    public string direction { get; set; }


    public int GetDirection()
    {
        int dir = 0;
        switch (direction)
        {
            case "up":
                dir = 0;
                break;
            case "down":
                dir = 1;
                break;
            case "left":
                dir = 2;
                break;
            case "right":
                dir = 3;
                break;
            case "upi":
                dir = 4;
                break;
            case "downi":
                dir = 5;
                break;
            case "lefti":
                dir = 6;
                break;
            case "righti":
                dir = 7;
                break;
            default:
                dir = -1;
                break;
        }
        return dir;
    }
}
