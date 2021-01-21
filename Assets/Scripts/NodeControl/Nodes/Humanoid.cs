using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Humanoid : MonoBehaviour
{
    void Start()
    {
        if (FunctionManager.BuildingName == "노유자시설")
        {
            this.gameObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        }
    }

    // Update is called once per frame
    void Update()
    {
            // transform.LookAt(Camera.main.transform.position, Vector3.up);
    }
}
