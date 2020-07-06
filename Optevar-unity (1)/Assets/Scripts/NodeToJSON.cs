using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
class NodePositions
{
    public Vector3[] positions;
}
[Serializable]
public class AreaPositions
{
    public string areaId = "";
    public Vector3 position;
}
[Serializable]
public class EvacuaterNodeJson
{
    //public GameObject gameobject;// clear하기 위한 object받아오기
    public int nodeId;
    public int nodeAge; //나이
    public int nodeSpeed;//속도
    //public int areaId;
    public Vector3 positions;
    public int areaId;
    //public float value1; //탈출무리 인원
    //public float previousUpdate;
    public EvacuaterNodeJson()
    {
    }
    public EvacuaterNodeJson(int _nodeId, int _areaId)
    {
        this.nodeId = _nodeId;
        this.areaId = _areaId;
    }
}

[Serializable]
public class SensorNodeJson
{
    public string nodeId;
    public int nodeType; // 화재 감지, mW, ToF ...
    //public int areaId;
    public Vector3 positions;
    public bool disaster;

    public float value1;
    //public float value2;
    //public float previousUpdate;
}

[Serializable]
public class ExitNodeJson
{
    public int nodeId;
    public int nodeType;
    public int areaId;
    public Vector3 positions;

    public float value1;
    public float previousUpdate;
}

[Serializable]
public class DisasterNodeJson 
    // 재난 발생 위치.
{
    public int nodeId;
    public int areaId;
    public int sensoId;
    public Vector3 positions;
    public float value1;
    public float previousUpdate;
}
/*
[Serializable]
public class AreaJson
{
    public int areaId;
    public Vector3[] points;
}*/

[Serializable]
public class Scenario
{
    //public int ScenarioId;
    public EvacuaterNodeJson[] evacuaterNodeJsons;//make_objects.person_list <--toArray로 바꾼다음 저장
    public SensorNodeJson[] sensorNodeJsons;
    public ExitNodeJson[] exitNodeJsons;
    //public AreaJson[] areaJsons;
    public DisasterNodeJson[] disasterNodeJsons;
    public AreaPositions[] areaPositionJsons;
}/*
[Serializable]
public class Scenario
{
    //public int ScenarioId;
    public List <EvacuaterNodeJson> evacuaterNodeJsons;
    public List <SensorNodeJson> sensorNodeJsons;
    public List <ExitNodeJson> exitNodeJsons;
    public List <DisasterNodeJson> disasterNodeJsons;
}*/

[Serializable]
public class ScreenshotAttr
{
    public Texture2D scrshot;
    public float time;
}/*
public interface IComparer {
    int Compare(float x, float y);
}

public class time_compare : IComparer {

    public float Compare(float x, float y) {
        return x.CompareTo(y);
    }

    int IComparer.Compare(float x, float y)
    {
        throw new NotImplementedException();
    }
}*/
