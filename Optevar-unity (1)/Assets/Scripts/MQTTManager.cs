using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;




public class MQTTManager: ScriptableObject
{
    MqttClient client;
    MQTTClass MQTTConf;
    JsonParser jsonParser = new JsonParser();
    ScenarioManager scenarioManager;
    List<EvacuaterNodeJson> evacuaterNodeJsons;
    List<SensorNodeJson> sensorNodeJsons;
    List<ExitNodeJson> exitNodeJsons;
    List<AreaPositions> areaJsons;
    Dictionary<int, int> areaNums;
    private string[] MessageDataElement = new string[] { "messageId", "nodeId", "timestamp", "sensorType", "value", "areaId" };
    public void Init()
    {
        MQTTConf = jsonParser.Load<MQTTClass>("mqttconf");
        if (MQTTConf.brokerAddr != null && MQTTConf.userName != null && MQTTConf.password != null)
        {
            Debug.Log("Connecting ... " + MQTTConf.brokerAddr);
            Connect();
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE };
            foreach(var t in MQTTConf.topic)
            {
                client.Subscribe(new string[] { t }, qosLevels);
            }
        }
    }
    public void SetLists(ScenarioManager sm)
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
        if (e.Topic == MQTTConf.topic[0])
        {
            //JsonUtility.FromJson
        }
        else if (e.Topic == MQTTConf.topic[1])
        {

        }
        else if (e.Topic == MQTTConf.topic[2])
        {
            //mwEvent
            //MessageData md = JsonUtility.FromJson<MessageData>(msg);
            MessageData md = Jsoning(msg);
            //scenarioManager.areaNums[md.areaId] = (int)md.value;
            scenarioManager.isAreaChanged = true;
        }
        else if (e.Topic == MQTTConf.topic[3])
        {
            // for DEMO, sim start
            scenarioManager.DEMOSTART = true;
        } else if (e.Topic == MQTTConf.topic[4])
        {
            // SensingValueEvent
            MessageData md = Jsoning(msg);
            //scenarioManager.sensorNodeJsons[md.nodeId].value1 = md.value;
        }
    }

    private void Publish(string _topic, string msg)
    {
        client.Publish(_topic, Encoding.UTF8.GetBytes(msg),
            MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
    }

    private void Connect()
    {
        client = new MqttClient(MQTTConf.brokerAddr);
        try
        {
            client.Connect(MQTTConf.clientId);
        }
        catch (Exception e)
        {
            Debug.LogError("Connnection error: " + e);
        }
    }

    MessageData Jsoning(string msg)
    {
        MessageData ret = new MessageData();
        msg = msg.Substring(1, msg.Length - 2);
        string[] msgs = msg.Split(',');
        for (int i = 0; i < msgs.Length; i++)
        {
            string[] tmp = msgs[i].Split(':');
            for (int j = 0; j < 2; j++)
            {
                tmp[j] = tmp[j].Replace("'", "");
                tmp[j] = tmp[j].Trim();
            }
            ret.Set(tmp[0], tmp[1]);
        }
        return ret;
    }
}
