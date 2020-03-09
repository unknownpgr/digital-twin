﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Reflection;
using System.Linq;

/*
Node :

1. Node는 게임 상에서 설치 가능하고, 속성을 가지며, 이동 가능한 모든 객체를 말한다.
예를 들어 센서, 탈출구, 대피자 구역 등이 모두 Node에 포함된다.
각 Node는 NodeManager를 상속한 script로부터 구현된다.

2. Node는 Script-base로 관리된다. 즉, Node에 관한 모든 정보는 반드시 NodeManager를 상속한 script를 통해서 이루어져야하며,
gameobject등으로부터 직접적으로 수정되어서는 안 된다.
예컨대 Node의 위치를 바꾸려면 NodeManager에 그러한 함수를 넣어야 하며, gameobject.transform.Position등으로 접근하여 수정해서는 안 된다.

3. Node는 각 Node와 연결된 단 하나의 Gameobject를 가진다. 이 gameobject는 prefab등이어서는 안 되며,
반드시 initiated되어 실제로 game space상에 존재하는 object여야만 한다.
이 gameObject는 생성, 제거를 포함하여 NodeManager script에 의하여 자동으로 관리되며, 임의로 생성하거나 삭제하는 등의 조작을 가해서는 안 된다.

4. 각 Node와 연결된 gameObject는 prefab을 사용하는데, NodeManager script를 상속받은 script는 반드시 이 prefab을 코드상으로 지정해야만 한다.
이는 abstract class를 사용하여 반드시 구현하도록 강제될 것이다.
NodeManager은 prefab을 생성한 후 자기 자신을 prefab에 할당할 것이다. 따라서 prefab은 NodeManager script를 포함해서는 안 된다.

5. 노드는 단 하나의 유일한 Physical ID를 가진다. 이는 절대로 중복되지 않는다.
노드를 가져오기 위해서는 GetNode함수를 사용할 수 있다. 오직 physical ID를 사용해서만 node를 가져올 수 있다.

6. 노드를 생성하기 위해서는 오직 NodeManager.Instantiate 함수만을 사용할 수 있다.

7. Node의 Physical ID는 한 번 할당하면 절대로 변경할 수 없다.

8. 이 코드를 보면, 이 코드의 유일한 Dependancy는 MonoBehavior과 NodeManager 자기 자신이다. 그러므로 NodeManager 외부에서 어떤 변화가 있더라도 NodeManager는 Reusable하다.

Node를 캡슐화를 잘 하면서 Jsonfy-Inflate할 방법은 없는가?
Serialize를 하면 이 고생을 할 이유가 전연 없겠으나, 그러지 말라고 하니..
일단 노드에 NodeManager class단에서는 알 수는 없는, 특정 Property들이 저장되어야 한다는 것은 자명하다.
또한 그런 property들은 string, int enum vector3등 다양한 type임에 분명하다.
또 노드를 Instantiate할 때에는 이 값들을 어째야 하는가? 알 수 없다.

먼저 정리를 해 보자.

일단 노드를 생성하는 방법에는 두 가지가 있다.
첫 번째는 노드들 그냥 생성하는 방법이다. 노드가 가져야 할 property들은 기본값으로 초기화된다.
두 번째는 노드를 property와 함께 생성하는 방법이다.

우리는 두 번째 방법만을 먼저 고려할 것인데, 왜냐하면 첫 번째 방법은 두 번째 방법으로부터 파생될 수 있기 때문이다.
노드를 property와 함께 생성하는 것은 매우 어려운데, 왜냐 하면 그 property가 정확히 무엇인지 알 수 없기 때문이다.
한 가지 확실한 것은, 노드가 가진 모든 프로퍼티는 string으로 변환하능하며, string으로부터 복원가능해야만 한다.
또한 노드가 가진 프로퍼티가 전부 하나의 dictionary에 저장될 수 있다고 생각해서도 안 되는데, 왜냐하면 각 프로퍼티의 타입이 매우 크게 다를 수 있기 때문이다.
이 경우 직접 프로퍼티의 변환을 구현하는 것에 비하여 드는 노력이 크다.

저장 시:
abstract class에서는 class type, position, physical ID등 공통되는 속성에 대한 jsonfy를 진행하고, 
derived class에서는 센서마다 다른 property를 string-based dictionary에 담아서 반환한다.

로드 시:
abstract class에 jsonfy된 객체를 넘기면 abstract class가 이를 파싱하여 
해당 class의 객체를 생성하고, 객체에 상기한 공통되는 속성을 지정한다. 이후 class의 Init함수를 사용하여 
*/

abstract public class NodeManager
{
    //===[ Keys for stringfy ]==========================================================================
    private const string KEY_NODE_TYPE = "NodeType";
    private const string KEY_PHYSICAL_ID = "PhysicalID";
    private const string KEY_POSITION = "Position";
    private const string KEY_PROPERTY = "Property";

    //===[ Private-Static fields ]===========================================================================

    // Dictionary of every node
    private static Dictionary<string, NodeManager> nodes = new Dictionary<string, NodeManager>();
    // Dictionary of node types
    private static Dictionary<string, Type> nodeTypes = new Dictionary<string, Type>();

    //===[ Public properties of node ]===========================================================================

    // Physical ID of node.
    private string physicalID;
    public string PhysicalID { get => physicalID; }

    // Name of node type. default is class name.
    public virtual string NodeType { get => GetType().Name; }

    // Position of node.
    public Vector3 Position
    {
        get => gameObject.transform.position;
        set { gameObject.transform.position = value; }
    }

    // ===[ Protected = child-only properties of node ]==========================================================================

    // Prefab name of node
    protected abstract string prefabName { get; }

    // Related gameobject
    protected GameObject gameObject;

    //===[ Constructors ]===========================================================================

    // Automatically register existing NodeType to nodeTypes
    static NodeManager()
    {
        foreach (Type t in Assembly
        .GetAssembly(typeof(NodeManager))
        .GetTypes()
        .Where(t => t.IsSubclassOf(typeof(NodeManager))))
        {
            nodeTypes.Add(t.Name, t);
            Debug.Log("Node type : " + t.Name);
        }
    }

    public NodeManager()
    {
        // New keyword exception
        if (!initiatable) throw new Exception("You cannot create NodeManager via new keyword.");
    }

    //===[ Public static methods ]===========================================================================

    // You only can make any instance of NodeManager's subclass by using this method.
    private static bool initiatable = false; // It is used to prevent nodemanager is created via new keyword.
    public static NodeManager Instantiate(string json)
    {
        // Use jsonfy extension
        Dictionary<string, string> jsonDict = json.Jsonfy();

        // Create NodeManager.
        // Because NodeManager is not a derivated type of MonoBehavior,
        // It can be initiated with new keyword.
        string physicalID = jsonDict[KEY_PHYSICAL_ID];

        // Check if given key exists
        if (nodes.ContainsKey(physicalID)) return null;

        // Recover common properties
        Type nodeType = nodeTypes[jsonDict[KEY_NODE_TYPE]];
        Vector3 position = jsonDict[KEY_POSITION].ToVector3();
        Dictionary<string, string> properties = jsonDict[KEY_PROPERTY].Jsonfy();

        // Create NodeManager instance of given type and set common property
        initiatable = true;
        NodeManager nodeManager = (NodeManager)Activator.CreateInstance(nodeType);
        initiatable = false;
        nodeManager.physicalID = physicalID;
        nodeManager.Position = position;
        nodeManager.DictToProperty(properties);

        // Load prefab
        GameObject prefab = (GameObject)Resources.Load("Prefabs/" + nodeManager.prefabName);
        if (prefab == null) return null;

        // Attach gameObject to nodeManager
        nodeManager.gameObject = (GameObject)GameObject.Instantiate(prefab);
        nodeManager.gameObject.transform.position = position;
        nodeManager.Init();

        return nodeManager;
    }

    // Get node by it's name
    public static NodeManager GetNode(string nodeName)
    {
        if (!nodes.ContainsKey(nodeName)) return null;
        return nodes[nodeName];
    }

    // Return the list of name(=physical id) of every node.
    public static string[] GetNodeNames()
    {
        return nodes.Keys.ToArray();
    }

    // Return the list of all nodes
    public static List<NodeManager> GetAll()
    {
        List<NodeManager> list = new List<NodeManager>();
        foreach (string key in nodes.Keys) list.Add(nodes[key]);
        return list;
    }

    // Destroy every node and its properties.
    public static void DestroyAll()
    {
        string[] keys = new string[nodes.Count];
        nodes.Keys.CopyTo(keys, 0);
        foreach (string key in keys) nodes[key].Destroy();
    }

    //===[ Protected abstract methods ]===========================================================================

    // You must initialize node here.
    protected abstract void Init();
    protected abstract void DictToProperty(Dictionary<string, string> properties);
    protected abstract Dictionary<string, string> PropertyToDict();

    //===[ Public methods ]===========================================================================

    public void SetActive(bool activation)
    {
        gameObject.SetActive(activation);
    }

    public void Destroy()
    {
        // Remove gameObject first.
        GameObject.Destroy(gameObject);
        // Then remove this object from dictionary.
        nodes.Remove(physicalID);
        // To prevent further access, remove some properties.
        physicalID = null;
        gameObject = null;
    }

    // Convert current node to json.
    public string Stringfy()
    {
        Dictionary<string, string> property = PropertyToDict();
        string propertyString = property.Stringfy();

        Dictionary<string, string> json = new Dictionary<string, string>();
        json.Add(KEY_PHYSICAL_ID, physicalID);
        json.Add(KEY_NODE_TYPE, GetType().Name);
        json.Add(KEY_POSITION, Position.ToString());
        json.Add(KEY_PROPERTY, propertyString);

        return json.Stringfy();
    }

    //===[ Default method override ]===========================================================================
    public override sealed string ToString()
    {
        return Stringfy();
    }
}

// Extension for jsonfy
static class JSONExtension
{
    public static string Stringfy(this Dictionary<string, string> dictionary)
    {
        var kvs = dictionary.Select(kvp => string.Format("\"{0}\":\"{1}\"", kvp.Key, string.Concat(",", kvp.Value)));
        return string.Concat("{", string.Join(",", kvs), "}");
    }

    public static Dictionary<string, string> Jsonfy(this string json)
    {
        string[] keyValueArray = json.Replace("{", string.Empty).Replace("}", string.Empty).Replace("\"", string.Empty).Split(',');
        return keyValueArray.ToDictionary(item => item.Split(':')[0], item => item.Split(':')[1]);
    }

    public static Vector3 ToVector3(this string sVector)
    {
        // Remove the parentheses
        sVector = sVector.Replace("(", "").Replace(")", "");

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        return new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));
    }
}