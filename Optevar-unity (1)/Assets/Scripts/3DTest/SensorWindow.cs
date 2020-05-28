using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensorWindow : MonoBehaviour
{
    public InputField nodeIdText;
    public InputField nodeTypeText;
    public Text nodeValueText;
    RectTransform RectTransform;
    Transform[] Children;
    sensor_attribute pre_sensor;
    // Start is called before the first frame update
    void Start()
    {
        Children = this.GetComponentsInChildren<Transform>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Set(Vector3 position, int nodeType, float value = 0, int nodeId = -1)
    {
        for (int i = 0; i < Children.Length; i++)
            Children[i].gameObject.SetActive(true);

        nodeIdText.text = nodeId.ToString();
        nodeTypeText.text = SensorTypeToString(nodeType);
        nodeValueText.text = value.ToString();
    }

    public void SetGUI(Vector3 position, sensor_attribute sensor)
    {
        for (int i = 0; i < Children.Length; i++)
            Children[i].gameObject.SetActive(true);
        pre_sensor = sensor;
        nodeIdText.text = sensor.one_sensor.nodeId.ToString();
        nodeTypeText.text = SensorTypeToString(sensor.one_sensor.nodeType);
        nodeValueText.text = sensor.one_sensor.value1.ToString();
    }

    public void Set()
    {
        Debug.Log("Setting...");
        pre_sensor.one_sensor.nodeId = (nodeIdText.text);
        pre_sensor.one_sensor.nodeType = int.Parse(nodeTypeText.text);
    }
    public void SetId()
    {
        Debug.Log("Setting...");
        pre_sensor.one_sensor.nodeId = nodeIdText.text;
    }
    public void SetType()
    {
        pre_sensor.one_sensor.nodeType = int.Parse(nodeTypeText.text);
    }

    string SensorTypeToString(int type)
    {
        switch (type)
        {
            case 33:
                return "33(화재)";
            case 2:
                return "2(수재해)";
            case 3:
                return "3(지진)";
            case 39:
                return "39(방향지시등)";
            default:
                return "_" + type.ToString();
        }
    }
}
