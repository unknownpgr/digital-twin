using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sensor_attribute : MonoBehaviour
{
    public SensorNodeJson one_sensor = new SensorNodeJson();
    //public int nodeId;
    // Start is called before the first frame update
    private void Awake()
    {
        one_sensor.nodeId = "-1";
        one_sensor.nodeType = -1;
        one_sensor.positions = new Vector3(0, 0, 0);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //nodeId = one_sensor.nodeId;
    }
}
