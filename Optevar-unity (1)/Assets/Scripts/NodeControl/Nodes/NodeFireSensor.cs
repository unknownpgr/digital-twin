using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    // 이는 꼭 구현할 필요는 없다. 만약 이 부분을 구현하지 않으면 DisplayName을 참조하면 클래스 이름이 반환된다.
    public override string DisplayName { get => "화재 센서(Fire Sensor):" + PhysicalID; }

    // 이 변수는 아래에서 dictionary key로 사용된다.
    private const string KEY_SENSOR_SIZE = "SensorSize";

    // 별 의미는 없는, 사이즈를 결정하는 변수이다. 예시로 들고자 넣었다.
    private float sensorSize;

    // 3. 다음으로 구현해야 하는 것은 Init 함수이다. 이 역시 필수적으로 구현해야만 한다.(abstract)
    // Init함수는 constructor와 같은 역할을 한다고 보면 된다.
    // NodeManager는 Factory design pattern을 사용하여 new keyword로 생성할 수 없다.
    // 따라서 아래와 같은 초기화 함수가 필요한 것이다.
    // 만약 딱히 할 게 없다면, 빈 상태로 두면 된다.
    protected override void Init()
    {
        gameObject.transform.localScale = new Vector3(sensorSize, sensorSize, sensorSize);
    }

    // 4. 마지막으로 해야 하는 것은 Dict와 오브젝트의 속성을 변환하는 함수를 구현하는 것이다.
    // 아래 둘 역시 abstract로 선언되어 있어 구현하지 않으면 컴파일이 되지 않는다.
    // 아래 함수가 하는 일은, sensorSize와 같이 부모 클래스에 정의되어있지 않은 변수들을 json으로 변환할 수 있도록
    // Dict형식으로 바꾸고 이를 다시 원래 property로 변환하는 것이다.
    // 그냥 새로 정의한 모든 변수를 string으로 바꾸고, 다시 이를 변수에 할당하기만 하면 된다.
    // 만약 새로 정의한 변수들이 전부 string이라면, 그냥 변수들을 dict로 한번에 선언한 후 한꺼번에 넘겨도 된다.
    // 만약 새로 정의한 변수가 단 하나도 없다면, Init과 마찬가지로 비워두면 된다.
    // DictToProperty를 구현할 때에는, dict에 해당 key가 없는 경우를 고려하여 구현해야만 한다.
    // 만약 꼭 그 key가 필요한 경우, 그런 key가 없으면 Exception을 throw 하도록 구현하면 된다.
    protected override void DictToProperty(Dictionary<string, string> dict)
    {
        // Load value from dict with key check.
        sensorSize = dict.ContainsKey(KEY_SENSOR_SIZE) ? float.Parse(dict[KEY_SENSOR_SIZE]) : 1.0f;
    }

    protected override Dictionary<string, string> PropertyToDict()
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict.Add(KEY_SENSOR_SIZE, sensorSize + "");
        return dict;
    }

    // 위에서 설명한 세 개의 함수 중 Init, DictToProperty는 객체가 초기화될 때 호출된다.
    // 단 DictToProperty가 먼저 호출되어 property를 다 설정한 후 Init이 호출된다.
    // 따라서 만약 Init에서 어떤 변수를 초기화하고, DictToProperty에서 참조하려 한다면,
    // Init이 나중에 호출되므로 변수가 초기화되지 않아 Exception을 일으킬 것이다.
}
