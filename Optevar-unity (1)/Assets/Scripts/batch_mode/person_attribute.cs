using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//person의 속성을 담는 script
public class person_attribute : MonoBehaviour
{
    
    public EvacuaterNodeJson one_person = new EvacuaterNodeJson();
    // Start is called before the first frame update
    private void Awake()
    {
        //person node의 default값 설정
        one_person.nodeId = -1;
        one_person.nodeAge = 20;
        one_person.nodeSpeed = 50;
        one_person.positions = new Vector3(0, 0, 0);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //nodeAge = one_person.nodeAge;
    }
}
