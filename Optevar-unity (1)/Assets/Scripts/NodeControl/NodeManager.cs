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

-----

Node의 Jsonfy에 대하여.
Node를 Jsonfy하려는데, 여러 문제가 발생하는 듯하다.

- 내가 개발한 방법을 사용
>> 속성들을 전부 Dict로 저장해야 하므로 코딩하기가 매우매우 귀찮다.
>> 생성된 JSON이 가독성이 개판이다.
>> 기존 JSON들과 호환되지 않는다.
>> 한 번에 여러 노드들을 Jsonfy할 수 없다.
>> Hierarchy구조로 만들기가 어렵다

- 새로 개발한 방법을 사용
>> 노드를 New 키워드를 사용하여 Initialize해야한다.
>> 반드시 Initialize를 하고 나서 JSON을 로드하게 된다.

그렇다면 먼저 기존의 Jsonfy-Inflate과정을 살펴보자.
1. 타입을 읽는다.
2. 타입에 맞는 NodeManager를 new 키워드로 생성한다.
3. prefab종류를 읽는다.
4. prefab을 Inflate한다.
5. 나머지 속성을 읽는다.
6. 나머지 속성을 적용한다.

그런데 생각해본 바, JSON.net라이브러리에서 형식을 유지하는 기능이 있었다!
따라서 다음과 같이 할 수 있다.

1. 형식을 유지하면서 new Keyword를 사용하여 객체를 Inflate한다.
2. prefabName은 컴파일 시에 지정되기 때문에, constructor에서 prefab을 Inflate한 후 attach.
3. 나머지 변수를 자동적으로 복구

이럴 경우 Type등을 굳이 변수로 지정할 필요가 없다는 장점도 있다.
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
        get => gameObject.transform.position;
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
        STATE_INITIALIZED       // Node is placed.
    }
    private NodeState state = NodeState.STATE_UNINITIALIZED;
    public NodeState State
    {
        get => state;
        set
        {
            state = value;
            if (state == NodeState.STATE_INITIALIZED) gameObject.SetActive(true);
            else gameObject.SetActive(false);
            if (OnNodeStateChanged != null) OnNodeStateChanged();
        }
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
        List<NodeType> nodes = new List<NodeType>();
        Type nodeType = typeof(NodeType);
        foreach (NodeManager node in nodes)
        {
            if (node.GetType() == nodeType) nodes.Add((NodeType)node);
        }
        return nodes;
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