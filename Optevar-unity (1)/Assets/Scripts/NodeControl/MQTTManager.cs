﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;
using System.Threading;
using System.IO;

public class MQTTManager : MonoBehaviour
{
    public class MQTTMsgData
    {
        // Visible variable
        [JsonIgnore]
        public Type NodeType { get => Const.GetNodeTypeFromNumber(sensorType); }
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
        public int sensorType = 0;
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
                data.sensorType = 53; // NodeArea
            }
            else data.PhysicalID = data.nodeId;
            if (data.PhysicalID == null || data.PhysicalID == "") throw new Exception("MQTTMsgData without physical ID.");
            return data;
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

    private MqttClient client;
    private MQTTClass mqttConf;
    private JsonParser jsonParser = new JsonParser();

    public delegate void Del(MQTTMsgData data);
    public static Del OnNodeUpdated;

    // Path of mqttconf. cannot call Application.dataPath in thread.
    private string mqttConfPath;

    public void Init(string configuration = "mqttconf")
    {
        // Initialzie dispatcher. Must be called in main therad because it uses Unity Engine.
        // Dispatcher.Init();

        // Do blocking tasks in thread.
        mqttConfPath = Application.dataPath + "/Resources/scenario_jsons/mqttconf.json";
        Thread thread = new Thread(InitTask);
        thread.Start();
    }

    private void InitTask()
    {
        // Load configuration
        mqttConf = JsonConvert.DeserializeObject<MQTTClass>(File.ReadAllText(mqttConfPath));

        // Connect
        mqttConf.clientId = Guid.NewGuid().ToString();//random string생성해 clientId입력
        Debug.Log("Connecting ... " + mqttConf.brokerAddr + ", " + mqttConf.clientId);
        Connect();
    }

    public void Publish(string topic, string msg)
    {
        Debug.Log("Topic : "+topic+"\nMessage : "+msg);
        new Thread(() => client.Publish(topic, Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false)).Start();
    }

    private void Connect()
    {
        new Thread(() =>
        {
            client = new MqttClient(mqttConf.brokerAddr);
            try
            {
                client.Connect(mqttConf.clientId, mqttConf.userName, mqttConf.password, false, 0);

                // Register event listener
                client.MqttMsgPublishReceived += OnMQTTMsgReceived;
                client.ConnectionClosed += OnConnectionClosed;

                // Setting - I don't know what exactly it does.
                byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE };
                foreach (var t in mqttConf.topic)
                {
                    client.Subscribe(new string[] { t }, qosLevels);
                }

                Dispatcher.Invoke(() => { Popup.Show("Connected to MQTT server."); });
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(() => { Debug.Log("Error occurred : " + e); });
            }
        }).Start();
    }

    private void OnConnectionClosed(object sender, EventArgs e)
    {
        client.Connect(mqttConf.clientId);
    }

    private void OnMQTTMsgReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string msgStr = System.Text.Encoding.UTF8.GetString(e.Message);
        string topic = e.Topic;

        if (topic == mqttConf.topic[0] ||   //mws/Notification/Periodic/SensingValueEvent
            topic == mqttConf.topic[1] ||   //mws/Notification/Periodic/DisasterEvent
            topic == mqttConf.topic[2] ||   //mws/Notification/Periodic/EvacueeEvent
            topic == mqttConf.topic[3])     //mws/Set/Direction
        {
            MQTTMsgData data = MQTTMsgData.GetMQTTMsgData(msgStr, topic == mqttConf.topic[1]);
            Dispatcher.Invoke(() => OnNodeUpdated?.Invoke(data));
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

    // ===[ Specialized publish method ]===========================

    public void PubDirectionOperation(string physicalID, string dir)
    {
        // e.g. {"nodeId":"4", "direction":"down"}
        string json = "{\"nodeId\":\"" + physicalID + "\",\"direction\":\"" + dir + "\"}";
        Publish(mqttConf.topic[3], json);
    }

    public void PubAreaUpdate(string id)
    {
        string msg = "{'areaId':'" + id + "'}";
        Publish(mqttConf.topic[5], msg);
    }

    public void PubSiren(bool siren)
    {
        //ToDo : NodeID for siren
        foreach (string sirenID in DataManager.GetSirenIDs())
        {
            string json = "{\"nodeId\":\"" + sirenID + "\",\"sound\":\"" + (siren ? "on" : "off") + "\"}";
            Publish("mws/Set/Sound", json);
        }
    }
}