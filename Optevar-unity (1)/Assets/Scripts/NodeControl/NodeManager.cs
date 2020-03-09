using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization.Json;

/*

아래 1~8번까지는 개발을 하면서 NodeManager가 가져야 하는 속성들을 생각나는 대로 나열한 것으로, 순서에 의미는 없다.

Node :

1. Node는 게임 상에서 설치 가능하고, 속성을 가지며, 이동 가능한 모든 객체를 말한다.
예를 들어 센서, 탈출구, 대피자 구역 등이 모두 Node에 포함된다.
각 Node는 NodeManager를 상속한 script로부터 구현된다.

2. Node는 Script-base로 관리된다. 즉, Node에 관한 모든 정보는 반드시 NodeManager를 상속한 script를 통해서 이루어져야하며,
gameobject등으로부터 직접적으로 수정되어서는 안 된다.
예컨대 Node의 위치를 바꾸려면 NodeManager에 그러한 함수를 넣어야 하며, gameobject.transform.Position등으로 접근하여 수정해서는 안 된다.

3. Node는 각 Node와 연결된 단 하나의 Gameobject를 가진다. 이 gameobject는 resource에서 로드되어 아직 Initiated된 prefab등이어서는 안 되며,
반드시 initiated되어 실제로 game space상에 존재하는 object여야만 한다.
이 gameObject는 생성, 제거를 포함하여 NodeManager script에 의하여 자동으로 관리되며, 임의로 생성하거나 삭제하는 등의 조작을 가해서는 안 된다.

4. 각 Node와 연결된 gameObject는 prefab을 사용하는데, NodeManager script를 상속받은 script는 반드시 이 prefab을 코드상으로 지정해야만 한다.
이는 abstract class를 사용하여 반드시 구현하도록 강제될 것이다.
NodeManager은 prefab을 생성한 후 자기 자신을 prefab에 할당할 것이다. 따라서 prefab은 NodeManager script를 포함해서는 안 된다.

5. Node는 단 하나의 유일한 Physical ID를 가진다. 이는 절대로 중복되지 않는다.
Node를 가져오기 위해서는 GetNode함수를 사용할 수 있다. 오직 physical ID를 사용해서만 node를 가져올 수 있다.

6. Node를 생성하기 위해서는 오직 NodeManager.Instantiate 함수만을 사용할 수 있다.

7. Node의 Physical ID는 한 번 할당하면 절대로 변경할 수 없다.

8. 이 코드를 보면, 이 코드의 유일한 Dependancy는 MonoBehavior과 NodeManager 자기 자신이다. 그러므로 NodeManager 외부에서 어떤 변화가 있더라도 NodeManager는 Reusable하다.

< 노드의 Save / Load 에 관하여 >
Node를 생성하는 방법에는 두 가지가 있다.
첫 번째는 Node들 그냥 생성하는 방법이다. Node가 가져야 할 property들은 기본값으로 초기화된다.
두 번째는 Node를 property와 함께 생성하는 방법이다.

우리는 두 번째 방법만을 먼저 고려할 것인데, 왜냐하면 첫 번째 방법은 두 번째 방법으로부터 파생될 수 있기 때문이다.
또한 Node를 property와 함께 생성하는 것은 매우 어려운데, 왜냐하면 그 property가 정확히 무엇인지 알 수 없기 때문이다.
그러나 한 가지 확실한 것은, Node가 가진 모든 Property는 string으로 변환하능하며, string으로부터 복원가능해야만 한다는 것이다.
다만 Node가 가진 Property가 전부 하나의 dictionary에 저장될 수 있다고 생각해서도 안 되는데, 왜냐하면 각 Property의 타입이 크게 다를 수 있기 때문이다.
이 경우 하나의 Dictionary에 모든 변수를 담으려 하면, 직접 Property->Dictionary 변환을 구현하는 것에 비하여 드는 노력이 크다.

따라서 다음과 같은 방법을 사용할 수 있다.

저장 시:
abstract class에서는 class type, position, physical ID등 공통되는 속성에 대한 jsonfy를 진행하고, 
derived class에서는 센서마다 다른 property를 string-based dictionary에 담아서 반환한다.
그러면 abstract class에서는 공통 속성과 derived class-dependent한 속성을 전부 하나의 json에 담아 반환한다.

로드 시:
abstract class에 jsonfy된 객체를 넘기면 abstract class가 이를 파싱하여 
해당 class의 객체를 생성하고, 객체에 상기한 공통되는 속성을 지정한다. 이후 derived class에서 다시 dependent한 속성을 받아 복원한다.


* 이때 객체를 바로 Serialize할 수 있으나, 그렇게 하지 않고 Dictionary를 거쳐 Serialize하는 것은 추후 옵션이 변경될 경우를 고려한 것이다.
어떠한 저장 매체를 사용하던지, class를 바로 Serialize하지 않는 경우에는 반드시 string형식으로의 변환이 필요하다.
이때 dictionary를 사용한다면 DB든 CSV이든 다른 매체로의 변경이 편리하다.
따라서 어차피 Dictionary<string,string>으로 일시적으로 변환하는 것이 어렵지 않은 바, 추후 변경할 것을 고려하여 그렇게 구현하였다.

-----

또한 NodeManager는 abstract class로서, 반드시 child class를 생성해야만 하나, 여러 기법들을 사용하여
어떤 child class를 생성하든 NodeManager에 dependency가 없어 NodeManager를 수정할 필요가 없는 점에 유의하라.
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

    // Name of node type. default is class name. It is just for display.
    // Do not use DisplayName as more than a string, such as dictionary key.
    // As a dictionary key, use the PhysicalID instead.
    public virtual string DisplayName { get => GetType().Name; }

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

        // Create NodeManager instance
        initiatable = true;
        NodeManager nodeManager = (NodeManager)Activator.CreateInstance(nodeType);
        initiatable = false;

        // Load prefab
        GameObject prefab = (GameObject)Resources.Load("Prefabs/" + nodeManager.prefabName);
        if (prefab == null) return null;

        // Attach gameObject to nodeManager
        nodeManager.gameObject = (GameObject)GameObject.Instantiate(prefab);
        nodeManager.gameObject.transform.position = position;

        // Set other properties
        nodeManager.physicalID = physicalID;
        nodeManager.Position = position;
        nodeManager.DictToProperty(properties);
        nodes.Add(physicalID, nodeManager);
        nodeManager.Init();

        string nodeInfo =
        "\n===[ Node Information ]===============" +
        "\n    Physical ID = " + physicalID +
        "\n    Type = " + nodeType +
        "\n    Position = " + position +
        "\n======================================\n";
        Debug.Log(nodeInfo);

        return nodeManager;
    }

    // Get node by it's name
    public static NodeManager GetNodeByName(string nodeName)
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

    //===[ Function for test ]===========================================================================

    public static string[] __TEST__GetTestNodes(int n)
    {
        string[] r = new string[n];
        string[] types = nodeTypes.Keys.ToArray();
        for (int i = 0; i < n; i++)
        {
            Dictionary<string, string> t = new Dictionary<string, string>();
            t[KEY_NODE_TYPE] = types[i % nodeTypes.Count] + ""; ;
            t[KEY_PHYSICAL_ID] = "ID_TEST_" + i;
            t[KEY_POSITION] = new Vector3(i, i, i) + "";
            t[KEY_PROPERTY] = new Dictionary<string, string>().Stringfy(); ;
            r[i] = t.Stringfy();
            Debug.Log(r[i]);
        }
        return r;
    }
}


// Extension for jsonfy
static class JSONExtension
{
    public static string Stringfy(this Dictionary<string, string> dictionary)
    {
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
        using (MemoryStream ms = new MemoryStream())
        {
            serializer.WriteObject(ms, dictionary);
            return System.Text.Encoding.Default.GetString(ms.ToArray());
        }
    }

    public static Dictionary<string, string> Jsonfy(this string json)
    {
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
        using (MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(json)))
        {
            Dictionary<string, string> dict = (Dictionary<string, string>)serializer.ReadObject(ms);
            return dict;
        }
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