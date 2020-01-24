using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouse_button : MonoBehaviour
{
    public int is_clicked = 0;
    public Transform mouse_ob;
    // Start is called before the first frame update
    void Start()
    {
        mouse_ob = this.gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void on_mouse_button_down()
    {
        if (is_clicked == 0)
        {
            is_clicked = 1;
        }
        else if (is_clicked == 1)
        {
            is_clicked = 0;
        }
    }
}
