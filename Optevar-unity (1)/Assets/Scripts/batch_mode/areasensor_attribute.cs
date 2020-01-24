using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class areasensor_attribute : MonoBehaviour
{
    public AreaPositions one_sensor = new AreaPositions();
    //public int nodeId;
    // Start is called before the first frame update
    private void Awake()
    {
        one_sensor.areaId = "-1";
        one_sensor.position = new Vector3(0, 0, 0);
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
