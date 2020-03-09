using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeFireSensor : NodeManager
{
    protected override string prefabName { get => "Sensor"; }
    public override string DisplayName { get => "화재 센서(Fire Sensor)"; }

    private const string KEY_SENSOR_SIZE = "SensorSize";
    private float sensorSize;

    protected override void Init()
    {
        gameObject.transform.localScale = new Vector3(sensorSize, sensorSize, sensorSize);
    }

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
}
