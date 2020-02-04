using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

abstract public class NodeManager : MonoBehaviour
{
    public enum NodeType
    {
        SENSOR_FIRE = 0,
        SENSOR_EARTHQUAKE = 1,
        SENSOR_FLOOD = 2,
        SIGN_DIRECTION = 3,
        SIGN_EXIT = 4,
        SIGN_AREA_NUM = 5
    }

    // The number of created node. used as unique key.
    public static int nodeNumber = 0;

    public static Dictionary<string, NodeManager> nodes = new Dictionary<string, NodeManager>();

    // Encapsulated node attribute
    private NodeType nodeType;
    public NodeType Type { get => nodeType; }

    private string nodeID;
    public string NodeID { get => nodeID; }

    private Dictionary<string, int> attribute = new Dictionary<string, int>();


    public new Transform transform;

    public static NodeManager GetNode(NodeType nodeType)
    {
        Debug.Log("GetNode : " + nodeType);

        // Load proper prefab
        string prefabName;
        switch (nodeType)
        {
            // Sensors
            case NodeType.SENSOR_FIRE:
                prefabName = "Sensor";
                break;
            case NodeType.SENSOR_EARTHQUAKE:
                prefabName = "Sensor";
                break;
            case NodeType.SENSOR_FLOOD:
                prefabName = "Sensor";
                break;

            // Signes
            case NodeType.SIGN_DIRECTION:
                prefabName = "DirectionSensor";
                break;
            case NodeType.SIGN_EXIT:
                prefabName = "ExitSign";
                break;

            // Area number
            case NodeType.SIGN_AREA_NUM:
                prefabName = "AreaNumber";
                break;

            default:
                prefabName = "";
                Debug.Log("Unknown node tpye");
                break;
        }
        GameObject prefab = (GameObject)Resources.Load("Prefabs/" + prefabName);
        if (prefab == null) return null;
        GameObject newNode = (GameObject)Instantiate(prefab);
        NodeManager newNodeManager = newNode.GetComponent<NodeManager>();

        // Initialize node attribute
        newNodeManager.nodeType = nodeType;
        newNodeManager.transform = newNode.GetComponent<Transform>();
        newNodeManager.nodeID = (int)(nodeType) + ":" + nodeNumber;
        nodeNumber++; // Node number must increase. wheather node is created or not.

        //do additional initialization steps here

        // Return nodeManager
        return newNodeManager;
    }

    public static NodeManager GetNode(Stream stream)
    {
        BinaryFormatter b = new BinaryFormatter();
        return (NodeManager)b.Deserialize(stream);
    }

    public void Serialize(Stream stream)
    {
        BinaryFormatter b = new BinaryFormatter();
        b.Serialize(stream, this);
    }

    private void Start()
    {
        // Do not initialize here.
        // Initialization should be conducted in GetNodeManager function.
    }

    void Update()
    {

    }
}