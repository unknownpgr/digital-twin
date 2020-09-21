using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;

/*
이 스크립트는 화재 센서에 해당하는 스크립트를 구현한 것이다.
*/

public class NodeFireSensor : NodeManager
{
    // 1. 먼저 이 센서와 연동된 Prefab을 알려줄 필요가 있다.
    // 아래와 같이 그냥 prefabName변수를 override하기만 하면, 나머지는 NodeManager parent class에서 알아서 구현한다.
    // 이 변수는 abstract로 선언되어있기 때문에, 만약 이 변수를 override하지 않으면 컴파일이 되지 않는다.
    protected override string prefabName { get => "Sensor"; }

    // 2. 다음으로는 이 Node가 버튼 등에 표시될 때 사용할 이름을 지정하는 것이다.
    // 이는 꼭 구현할 필요는 없다. 만약 이 부분을 구현하지 않으면, DisplayName은 클래스 이름이 된다.
    public override string DisplayName { get => "화재 센서:" + PhysicalID; }

    // 그 외의 변수는 필요에 따라 구현하면 되고, 자동으로 JSON에 반영된다.
    // public 변수는 기본적으로 반영이 된다. 반영이 안 되게 하고 싶으면 [JsonIgnore] 태그를 써 주면 된다.
    // private 변수는 [JsonProperty]라는 태그를 변수 위에 써 주면 반영이 된다.
    private UnityEngine.AI.NavMeshObstacle navObstacle;
    private Material material;
    private Material nodeColor;
    private bool isDisaster = false;
    [JsonIgnore]
    public bool IsDisaster
    {
        get => isDisaster;
        set
        {
            isDisaster = value;
            navObstacle.carving = value;
            material.color = new Color(1, 0, 0, value ? .3f : 0);
            FunctionManager.Find("window_video").GetChild(1).GetChild(2).GetComponent<Text>().text
                = FindCamera();
        }
    }

    // ToDo : Connect it with window
    public float ValueTemp, ValueFire, ValueSmoke;

    // 3. 다음으로 구현해야 하는 것은 Init 함수이다. 이 역시 필수적으로 구현해야만 한다.(abstract)
    // Init함수는 constructor와 같은 역할을 한다고 보면 된다.
    // NodeManager는 Factory design pattern을 사용하므로 new keyword로 생성할 수 없다.
    // 따라서 아래와 같은 초기화 함수가 필요하게 된다.
    // 만약 딱히 할 게 없다면, 빈 상태로 두면 된다.
    protected override void Init()
    {
        material = gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material;
        navObstacle = gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>();
        if (material == null) throw new System.Exception("Material is null");
        if (navObstacle == null) throw new System.Exception("NavObstacle is null");

        gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
    }

    private string FindCamera()
    {
        string cameraID = null;
        float min = 10000000f;
        float temp = 0f;

        List<NodeCCTV> nodeCCTVs = NodeManager.GetNodesByType<NodeCCTV>();

        foreach (NodeCCTV nodeCCTV in nodeCCTVs)
        {
            if (BuildingManager.GetFloor(nodeCCTV.Position) == BuildingManager.GetFloor(this.Position))
            {
                temp = Vector3.Distance(nodeCCTV.Position, this.Position);

                if (temp < min)
                {
                    min = temp;
                    cameraID = nodeCCTV.PhysicalID;
                }
            }
        }

        return cameraID;
    }

}

