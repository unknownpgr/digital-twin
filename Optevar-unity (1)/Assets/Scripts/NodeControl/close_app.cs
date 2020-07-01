using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class close_app : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void close_app_button_clicked() {
        Application.Quit();
        //WindowManager closeWindow = WindowManager.GetWindow("window_close");
        //closeWindow.SetVisible(true);
    }

    public void CloseMainWindow()
    {
        //Application.Quit();
    }
}
