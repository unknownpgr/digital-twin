using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;

public class MQTTManager : MonoBehaviour
{
    public class MQTTMsgData
    {
        // Visible variable
        [JsonIgnore]
        public Type NodeType { get => GetNodeType(sensorType); }
        [JsonIgnore]
        public string PhysicalID;
        [JsonIgnore]
        public float Value { get => value; }
        [JsonIgnore]
        public bool IsDisaster { get => isDisaster; }
        [JsonIgnore]
        public string Direction { get => direction; }

        // Json property variable
        [JsonProperty]
        private string areaId = null;
        [JsonProperty]
        private string nodeId = null;
        [JsonProperty]
        private float value = 0;
        [JsonProperty]
        private int sensorType = 0;
        [JsonProperty]
        private bool isDisaster = false;
        [JsonProperty]
        private string direction = "";
        [JsonProperty]
        public string messageId = "";
        [JsonProperty]
        public string timestamp = "";

        private MQTTMsgData()
        {

        }

        public static MQTTMsgData GetMQTTMsgData(string jsonStr, bool isDisaster)
        {
            MQTTMsgData data = JsonConvert.DeserializeObject<MQTTMsgData>(jsonStr);
            data.isDisaster = isDisaster;
            if (data.nodeId == null || data.nodeId == "")
            {
                data.PhysicalID = data.areaId;
                data.sensorType = 100; // NodeArea
            }
            else data.PhysicalID = data.nodeId;
            if (data.PhysicalID == null || data.PhysicalID == "") throw new Exception("MQTTMsgData without physical ID.");
            return data;
        }

        private static Type GetNodeType(int nodeType)
        {
            // ToDo: Implement some other sensors including area.
            switch (nodeType)
            {
                case 33:    // Fire, 16진수 21
                    return typeof(NodeFireSensor);
                case 2:     // Water
                    return typeof(NodeFireSensor);
                case 3:     // Eearthquake
                    return typeof(NodeFireSensor);
                case 39:    // Direction
                    return typeof(NodeFireSensor);
                case 100:   //Area
                    return typeof(NodeArea);
                default:
                    return null;
            }
        }

        public override string ToString()
        {
            Dictionary<string, string> r = new Dictionary<string, string>()
            {
                ["PhysicalID"] = PhysicalID,
                ["NodeType"] = NodeType + "",
                ["Value"] = Value + "",
                ["IsDisaster"] = IsDisaster + ""
            };
            return JsonConvert.SerializeObject(r, Formatting.Indented);
        }
    }

    private class Dispatcher : MonoBehaviour
    {
        /*
        Dispatcher class for multithreading environment.
        Most UnityEngine object does not works on non-main thread.
        Methods can be invoked in main thread via this class.
        This class is especially designed to deal with MQTT receive event.
        */
        private static Dispatcher singleTon;
        private static List<Del> tasks = new List<Del>();
        private static List<MQTTMsgData> parameters = new List<MQTTMsgData>();
        private static GameObject dispatcherObject;

        public static void Init()
        {
            if ((singleTon == null) != (dispatcherObject == null)) throw new Exception("The nullity of the singleton and the gameobject are different.");
            if (dispatcherObject == null)
            {
                dispatcherObject = new GameObject();
                singleTon = dispatcherObject.AddComponent<Dispatcher>();
            }
        }

        public static void Invoke(Del action, MQTTMsgData param)
        {
            if (action == null) return;
            lock (tasks)
            {
                tasks.Add(action);
                parameters.Add(param);
            }
        }

        private void Update()
        {
            lock (tasks)
            {
                if (tasks.Count != parameters.Count) throw new Exception("The number of the tasks and the parameters unmatches.");
                for (int i = 0; i < tasks.Count; i++)
                {
                    try { tasks[i](parameters[i]); }
                    catch (Exception e) { Debug.Log(e); }
                }
                tasks.Clear();
                parameters.Clear();
            }
        }
    }

    private MqttClient client;
    private MQTTClass MQTTConf;
    private JsonParser jsonParser = new JsonParser();

    public delegate void Del(MQTTMsgData data);
    public Del OnNodeUpdated;

    public void Init(string configuration = "mqttconf")
    {
        // Initialzie dispatcher
        Dispatcher.Init();

        // Load configuration
        MQTTConf = jsonParser.Load<MQTTClass>("mqttconf");

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

    public void Publish(string topic, string msg)
    {
        client.Publish(topic, Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
    }

    public void PubDirectionOperation(string nodeId, string dir)//{"nodeId":"4","direction":"down"}
    {
        string json = string.Format("{\"nodeId\":\"{0}\",\"direction\":\"{1}\"}", nodeId, dir);
        // DirectionOperation d = new DirectionOperation();
        // d.nodeId = nodeId;
        // d.direction = dir;
        // string jd = JsonMapper.ToJson(d);
        // Publish(MQTTConf.topic[3], jd);
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
    private void OnConnectionClosed(object sender, EventArgs e)
    {
        client.Connect(MQTTConf.clientId);
        Debug.Log("Connection closed...");
    }

    private void OnMQTTMsgReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string msgStr = System.Text.Encoding.UTF8.GetString(e.Message);
        string topic = e.Topic;
        Debug.Log("Received msg from " + topic + " : " + msgStr);

        if (topic == MQTTConf.topic[0] ||   //mws/Notification/Periodic/SensingValueEvent
            topic == MQTTConf.topic[1] ||   //mws/Notification/Periodic/DisasterEvent
            topic == MQTTConf.topic[2] ||   //mws/Notification/Periodic/EvacueeEvent
            topic == MQTTConf.topic[3])     //mws/Set/Direction
        {
            MQTTMsgData data = MQTTMsgData.GetMQTTMsgData(msgStr, topic == MQTTConf.topic[1]);
            Dispatcher.Invoke(OnNodeUpdated, data);
        }
        else
        {
            Debug.Log("Unregistered MQTT topic : " + e.Topic);
            Debug.Log("Data : " + msgStr);
        }
    }

    public void Close()
    {
        if (client.IsConnected)
            client.Disconnect();
    }
}