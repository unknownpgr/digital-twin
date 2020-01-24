using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionSensorScript : MonoBehaviour
{
    List<GameObject> directions;
    // 0: North, 1: South, 2: East, 3: West
    // out: 0, 1, 2, 3; in: 4, 5, 6, 7
    // Start is called before the first frame update
    void Start()
    {
        directions = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
            directions.Add(transform.GetChild(i).gameObject);

        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init()
    {
        for (int i = 0; i < directions.Count; i++)
            directions[i].SetActive(false);
    }

    
    public void OnOff(int _direction, bool _isIn, bool _isOn)
    {
        if (_direction == -1)
        {
            Init();
            return;
        }
        int tmp = 0;
        if (_isIn) tmp += 4;

        tmp += _direction;
        directions[tmp].SetActive(_isOn);
    }
}
