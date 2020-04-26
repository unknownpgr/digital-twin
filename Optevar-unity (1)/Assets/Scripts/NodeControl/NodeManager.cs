using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization;

/*

    <<  Node / NodeManager에 대한 설명  >>

Node : Node는 게임 상에서 설치 가능하고, 속성을 가지며, 이동 가능한 모든 객체를 말한다.
    예를 들어 센서, 탈출구, 대피자 구역 등이 모두 Node에 포함된다.
    각 Node는 NodeManager를 상속한 script를 사용하여 구현된다. 다음과 같은 속성을 가진다.
    - Gameobject    : 게임 화면 상에 표시되는 실제 오브젝트
    - Physical ID   : 노드를 Unique하게 식별할 수 있는 키
    - NodeState     : 현재 노드의 상태. 설치되지 않았거나, 설치 중이거나, 설치 완료의 세 가지 상태 중 하나이다.

NodeManager : Node에 필요한 각종 작업들을 포함한 추상 클래스이다.
     이런 클래스가 필요한 이유는, 다른 오브젝트와 다르게, 노드는 게임화면 상에 있을 수도 있고 없을 수도 있기 때문이다.
    예컨대 DB상에서 어떤 노드의 정보가 존재하지만, 아직 설치되지 않았기 때문에 게임 화면상에서는 드러나지 않을 수도 있다.
    따라서 Node에 직접 Script를 붙여서는 아직 활성화되지 않은 노드들을 다룰 수 없다.
    뿐만 아니라 상태 변경, 재난 상황 추적, 초기화, 저장/로드 등 수많은 상황에서 노드들을 일괄적으로 다룰 필요가 있는데,
    기존의 노드 하나하나마다 서로 다른 Script가 할당된 경우 이러한 작업이 불가능하다.
    따라서 GameObject와는 무관한 NodeManager를 만들고, 이를 상속하는 Script들을 사용함으로써 이런 문제를 해결한다.
    즉, NodeManager는 Unity Engine과는 반대로 동작한다. Unity Engine에서는 GameObject가 있고, 스크립트가 그 속성으로 부여된다.
    그러나 NodeManager는 NodeManager가 있고, 그 속성으로 GameObject를 갖는다. 이로부터 Gameobject의 유무와 관계없이
    여러 작업들을 할 수 있다.

     NodeManager는 또한 Node의 Serialize/Deserialzie를 담당한다.
    JSON.net 라이브러리를 바탕으로 구현되며, 일괄적으로 모든 노드들의 정보를 하나의 JSON으로 담을 수 있고,
    JSON string으로부터 노드들을 다시 복원할 수 있다. 이는 NodeManager를 상속할 경우 자동으로 구현되는 것이기 때문에
    각 노드마다 Serialize/Deserialzie를 구현할 필요가 없다.
    NodeManager를 상속한 클래스의 프로퍼티들은 JSON으로 변환될 때 포함될 수도 있고 그렇지 않을 수도 있다.
    예컨대 위치는 포함되어야겠으나, 방향 센서의 방향이나 화재 센서의 온도는 포함될 필요가 없다.
    이를 결정하는 규칙은 아래와 같다.
    - 변수에 [JsonProperty] Attribute가 있다면 반드시 포함된다.
    - 변수에 [JsonIgnore] Attribute가 있다면 반드시 제외된다.
    - 만약 아무런 Attribute도 없을 경우, public 변수는 포함되고 private변수는 제외된다.
    - GameObject나 Delegate등 Serialize할 수 없는 변수는 포함되지 않는다.

    그 외에도 NodeManager는 abstract method를 사용하여 각 Node가 필수적으로 구현해야 할 속성들을 강제하는 역할도 한다.
*/

abstract public class NodeManager
{
    //===[ Private-Static fields ]===========================================================================

    // Dictionary of every node
    private static Dictionary<string, NodeManager> nodes = new Dictionary<string, NodeManager>();
    // Dictionary of node types
    private static Dictionary<string, Type> nodeTypes = new Dictionary<string, Type>();

    //===[ Public properties of node ]===========================================================================

    // Physical ID of node.
    [JsonIgnore]
    private string _physicalID;
    [JsonProperty(Order = -1)]
    public string PhysicalID
    {
        get => _physicalID;
        set
        {
            if (nodes.ContainsKey(value)) throw new Exception("Node " + value + " already exists");
            else nodes.Add(value, this);
            _physicalID = value;
        }
    }

    // Name of node type. default is class name. It is just for display.
    // Do not use DisplayName as more than a string, such as dictionary key.
    // As a dictionary key, use the PhysicalID instead.
    [JsonIgnore]
    public virtual string DisplayName { get => GetType().Name; }

    // Position of node.
    // This can be serialized via 'position' wrapper variable.
    [JsonIgnore]
    public Vector3 Position
    {
        get { return gameObject.transform.position; }
        set { gameObject.transform.position = value; }
    }

    // Wrapper variable for serialize Position(Vector3)
    [JsonProperty]
    private float[] position
    {
        get
        {
            return new float[] { Position.x, Position.y, Position.z };
        }
        set
        {
            Vector3 position = Position;
            position.x = value[0];
            position.y = value[1];
            position.z = value[2];
            Position = position;
        }
    }

    // Whether the node has been initialized
    // You cannot de-initialize node.
    public enum NodeState
    {
        STATE_UNINITIALIZED,    // Node has not been initialized
        STATE_PLACING,          // Node is beeing placed.
        STATE_INITIALIZED,      // Node is placed.
    }

    private bool hide = false;
    [JsonIgnore]
    public bool Hide
    {
        get => hide;
        set { if (hide == value) return; hide = value; onNodeStateChanged(); }
    }

    private NodeState state = NodeState.STATE_UNINITIALIZED;
    public NodeState State
    {
        get => state;
        set { if (state == value) return; state = value; onNodeStateChanged(); }
    }

    // ===[ Protected = child-only properties of node ]==========================================================================

    // Prefab name of node
    protected abstract string prefabName { get; }

    // Related gameobject
    [JsonIgnore]
    protected GameObject gameObject;

    //===[ Callbacks ]===========================================================================

    public delegate void Del();
    public static Del OnNodeStateChanged;

    private void onNodeStateChanged()
    {
        // Do not modify 'Hide' or 'State' in here. It would occur recursive function call stack overflow.
        hide = state != NodeState.STATE_INITIALIZED;    // Initialize 'hide' when node is not initialized.
        gameObject.SetActive(!hide);                    // If node is initialized and not hidden, show it.
        OnNodeStateChanged?.Invoke();                   // Invoke callback
    }

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
        }
    }

    public NodeManager()
    {
        // Check if initializable.
        if (!initiatable) throw new Exception("Cannot initialize the NodeManager(" + GetType() + ") with the new keyword.");

        // Load prefab from prefabName
        GameObject prefab = (GameObject)Resources.Load("Prefabs/" + prefabName);
        if (prefab == null) throw new Exception("No such prefab(" + prefabName + ") exists.");

        // Attach gameObject to nodeManager
        if (gameObject != null) GameObject.Destroy(gameObject);
        gameObject = (GameObject)GameObject.Instantiate(prefab);

        // Set visibility
        hide = state != NodeState.STATE_INITIALIZED;
        gameObject.SetActive(!hide);
    }

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        Init();
    }

    //===[ Public static methods ]===========================================================================

    // You only can make any instance of NodeManager's subclass by using this method.
    private static bool initiatable = false; // It is used to prevent nodemanager is created via new keyword.

    public static void Instantiate(string json)
    {
        initiatable = true;
        JsonConvert.DeserializeObject(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        initiatable = false;
    }

    public static void InitiateFromFile(string path)
    {
        string nodeString = File.ReadAllText(path);
        NodeManager.DestroyAll();
        NodeManager.Instantiate(nodeString);
    }

    public static string Jsonfy()
    {
        return JsonConvert.SerializeObject(GetAll(), Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
    }

    // Get node by it's phyiscal ID
    public static NodeManager GetNodeByID(string nodeID)
    {
        if (!nodes.ContainsKey(nodeID)) return null;
        return nodes[nodeID];
    }

    public static List<NodeType> GetNodesByType<NodeType>() where NodeType : NodeManager
    {
        List<NodeType> result = new List<NodeType>();
        Type nodeType = typeof(NodeType);
        foreach (NodeManager node in GetAll())
        {
            if (node.GetType() == nodeType) result.Add((NodeType)node);
        }
        return result;
    }

    public static List<NodeManager> GetNodesByType(Type nodeType)
    {
        List<NodeManager> nodes = new List<NodeManager>();
        foreach (NodeManager node in nodes)
        {
            if (node.GetType() == nodeType) nodes.Add(node);
        }
        return nodes;
    }

    // Return the list of physical id of every node.
    public static string[] GetNodeIDs()
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
    public static void ResetAll()
    {
        // 
        string[] keys = new string[nodes.Count];
        nodes.Keys.CopyTo(keys, 0);
        foreach (string key in keys) nodes[key].Reset();
    }

    public static void DestroyAll()
    {
        string[] keys = nodes.Keys.ToArray();
        foreach (string key in keys)
        {
            nodes[key].Destroy();
        }
        if (nodes.Count > 0) throw new Exception("Undestroied node remains");
    }

    //===[ Protected abstract methods ]===========================================================================

    // You must initialize node here.
    protected abstract void Init();

    //===[ Public methods ]===========================================================================

    public void Reset()
    {
        State = NodeState.STATE_UNINITIALIZED;
    }

    public void Destroy()
    {
        nodes.Remove(PhysicalID);
        OnNodeStateChanged = null;
        GameObject.Destroy(gameObject);
    }

    //===[ Function for test ]===========================================================================
    public static string __TEST__GetTestNodes(int n)
    {
        initiatable = true;

        List<NodeManager> nodeManagers = new List<NodeManager>();
        string[] typeNames = nodeTypes.Keys.ToArray();
        for (int i = 0; i < n; i++)
        {
            Type nodeType = nodeTypes[typeNames[i % nodeTypes.Count]];
            NodeManager newNode = (NodeManager)Activator.CreateInstance(nodeType);
            newNode.PhysicalID = "ID_TEST_" + i;
            newNode.Position = new Vector3(i, i, i);
            newNode.State = (i % 2 == 0) ? NodeState.STATE_INITIALIZED : NodeState.STATE_UNINITIALIZED;
            nodeManagers.Add(newNode);
        }

        string jsonfied = JsonConvert.SerializeObject(nodeManagers, Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

        DestroyAll();

        initiatable = false;

        return jsonfied;
    }
}