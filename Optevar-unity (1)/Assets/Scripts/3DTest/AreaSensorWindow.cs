using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AreaSensorWindow : MonoBehaviour
{
    public InputField nodeIdText;
    public InputField nodeTypeText;
    public Text nodeValueText;
    RectTransform RectTransform;
    Transform[] Children;
    areasensor_attribute pre_sensor;
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
        nodeTypeText.text = "0";
        nodeValueText.text = "0";
    }

    public void SetGUI(Vector3 position, areasensor_attribute sensor)
    {
        for (int i = 0; i < Children.Length; i++)
            Children[i].gameObject.SetActive(true);
        pre_sensor = sensor;
        nodeIdText.text = sensor.one_sensor.areaId.ToString();
    }


    public void SetId()
    {
        Debug.Log("Setting...");
        pre_sensor.one_sensor.areaId = (nodeIdText.text);
    }


    string SensorTypeToString(int type)
    {
        switch (type)
        {
            case 1:
                return "1(화재)";
            case 2:
                return "2(수재해)";
            case 3:
                return "3(지진)";
            case 4:
                return "4(방향지시등)";
            default:
                return "_" + type.ToString();
        }
    }
}
