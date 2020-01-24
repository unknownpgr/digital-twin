using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class question_button : MonoBehaviour
{
    //public GameObject question_btn;
    public GameObject question_img;
    
    // Start is called before the first frame update
    void Start()
    {
        GameObject question_img_copy = question_img;
        question_img_copy.gameObject.SetActive(false); 
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    public void on_pointer_enter()
    {
        GameObject question_img_copy = question_img;
        question_img_copy.gameObject.SetActive(true);

    }
    public void on_pointer_exit()
    {
        GameObject question_img_copy = question_img;
        question_img_copy.gameObject.SetActive(false);

    }

}
