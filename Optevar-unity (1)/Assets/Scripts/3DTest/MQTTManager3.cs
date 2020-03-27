using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using LitJson;

public class MQTTManager3 : MonoBehaviour
{
    [SerializeField]
    public class MQTTMsgData
    {
        public string messageId;
        public string nodeId;
        public string timestamp;
        public int sensorType;
        public float value;
        public bool isDisaster;

        public MQTTMsgData(string msgstr, bool isDisaster)
        {
            msgstr = msgstr.Trim();
            msgstr = msgstr.Substring(1, msgstr.Length - 2);
            string[] msgs = msgstr.Split(',');
            foreach (string msg in msgs)
            {
                string[] tmp = msg.Split(':');
                for (int j = 0; j < 2; j++)
                {
                    tmp[j] = tmp[j]
                        .Replace("'", "")
                        .Replace("\"", "")
                        .Trim();
                }
                this.SetProperty(tmp[0], tmp[1]);
            }
            this.isDisaster = isDisaster;
        }

        private void SetProperty(string key, string value)
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
                default:
                    Debug.Log("Unknown key :" + key);
                    break;
            }
        }
    }

    private MqttClient client;
    private MQTTConf MQTTConf;
    private JsonParser jsonParser = new JsonParser();

    public delegate void Del(MQTTMsgData data);
    public Del OnSensorUpdated;
    public Del OnDirectionUpdated;

    public void Init(string configuration = "mqttconf")
    {
        // Load configuration
        MQTTConf = jsonParser.Load<MQTTConf>("mqttconf");

        // Connect
        MQTTConf.clientId = Guid.NewGuid().ToString();//random string생성해 clientId입력
        Debug.Log("Connecting ... " + MQTTConf.brokerAddr + ", " + MQTTConf.clientId);
        Connect();

        // Register event listener
        client.MqttMsgPublishReceived += OnMQTTMsgReceived;
        client.ConnectionClosed += OnConnectionClosed;

        // Setting - I don't know what exactly it does.
        byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE };
        foreach (var t in MQTTConf.topic)
        {
            client.Subscribe(new string[] { t }, qosLevels);
        }
    }

    private void OnConnectionClosed(object sender, EventArgs e)
    {
        client.Connect(MQTTConf.clientId);
        //Debug.Log("Connection closed...");
    }

    private void OnMQTTMsgReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string msgStr = System.Text.Encoding.UTF8.GetString(e.Message);
        string topic = e.Topic;
        Debug.Log("Received msg from " + topic + " : " + msgStr);

        if (topic == MQTTConf.topic[0] ||//mws/Notification/Periodic/SensingValueEvent
            topic == MQTTConf.topic[1] ||//mws/Notification/Periodic/DisasterEvent
            topic == MQTTConf.topic[3])//mws/Set/Direction
        {
            MQTTMsgData data = new MQTTMsgData(msgStr, topic == MQTTConf.topic[1]);
            OnSensorUpdated?.Invoke(data);
        }
        else
        {
            Debug.Log("Unregistered MQTT topic : " + e.Topic);
            Debug.Log("Data : " + msgStr);
        }

        return;

        if (e.Topic == MQTTConf.topic[0])
        {
            /*
                {
                "nodeId":"00010000",
                "timestamp":"2020-02-21T02:23:03.321Z",
                "sensorType":33,
                "value":-10
                }
             */
            //"mws/Notification/Periodic/SensingValueEvent", wan

        }
        else if (e.Topic == MQTTConf.topic[1])
        {
            // print(MQTTConf.topic[1]);
            /*
             {
             "nodeId":"00010000",
             "timestamp":"1970-01-17T12:02:24Z",
             "sensorType":33,
             "value":-10
             }
             */
            // MQTTMsgData md = Parse(msgStr);
        }
        else if (e.Topic == MQTTConf.topic[2])//mws/Notification/Periodic/EvacueeEvent
        {

            /*
            room1 ( 00001000, 000020000)
            room2 ( 00001000, 000030000)
            일때, 
            00001000의 센싱이 발생할 때, 아래와 같이 두번의 mqtt event가 발생한다.
            {
            "areaId":"room2",
            "timestamp":"2020-02-21T02:56:17.355Z",
            "value":1
            }
            { "areaId":"room1","timestamp":"2020-02-21T02:56:17.353Z","value":2}
            // */
            // MessageData md = Jsoning(msg);
            // if (scenarioManager.areaNums.ContainsKey(md.areaId.Replace("room", "")))//(md.areaId))
            //     if (scenarioManager.areaNums[md.areaId.Replace("room", "")] != (int)md.value)
            //     {
            //         scenarioManager.areaNums[md.areaId.Replace("room", "")] = (int)md.value;
            //         scenarioManager.isAreaChanged = true;
            //     }
        }
        else if (e.Topic == MQTTConf.topic[3])
        //ScenarioManager3에서 말고 여기서 unity system에 방향지시등 모양 시각화 표시하도록
        {
            /*
             {
             "nodeId": "00001000",
             "direction": "up"
             }
            */
        }
        else if (e.Topic == MQTTConf.topic[4])//**아직 없음 "mws/Set/Sound"
        {
            /*
              {
              "nodeId": "00001000",
              "sound": "on"
              }   
            */

        }
        else if (e.Topic == MQTTConf.topic[5])//**mws/Request/Area    from DT to mws
        {//"미들웨어에게 특정 지역의 계산된 인원수 값을 요청한다.특정 지역의 계산된 인원수 값을 요청한다."
            /*
             { 
             "areaId": "room1" 
             }    
             */

        }
        else if (e.Topic.Contains("mws/Response/Area/"))//mws/Response/Area/room1   from mws to DT
        {//토픽의 wild card로 특정 지역 area ID를 지정한다. 특정 지역의 계산된 인원수 값을 응답한다.
         //****추가수정필요
            /*
             {
             "peopleCount":2
             }
             */
            // if (scenarioManager.areaNums.ContainsKey(md.areaId))
            //     if (scenarioManager.areaNums[md.areaId] != (int)md.value)
            //     {
            //         scenarioManager.areaNums[md.areaId] = (int)md.value;
            //         scenarioManager.isAreaChanged = true;
            //     }

        }
        else if (e.Topic == "messageID")//?
        {

        }
    }

    public void Publish(string topic, string msg)
    {
        client.Publish(topic, Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
    }

    public void PubDirectionOperation(string nodeId, string dir)//{"nodeId":"4","direction":"down"}
    {
        DirectionOperation d = new DirectionOperation();
        d.nodeId = nodeId;
        d.direction = dir;
        string jd = JsonMapper.ToJson(d);
        Publish(MQTTConf.topic[3], jd);
    }

    public void PubAreaUpdate(string id)
    {
        string msg = "{'areaId':'" + id + "'}";
        Publish(MQTTConf.topic[5], msg);
    }

    private void Connect()
    {
        client = new MqttClient(MQTTConf.brokerAddr);
        try
        {
            client.Connect(MQTTConf.clientId, MQTTConf.userName, MQTTConf.password, false, 0);
        }
        catch (Exception e)
        {
            Debug.LogError("Connnection error: " + e);
        }

    }

    public void Close()
    {
        if (client.IsConnected)
            client.Disconnect();
    }
}

[SerializeField]
class MQTTConf
{
    public string clientId = "testClient";
    public string brokerAddr = "13.124.77.110";
    public int brokerPort = 8883;
    public string userName = "testId";
    public string password = "test";
    public string[] topic;
}

class DirectionOperation//
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
