using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class win_close : MonoBehaviour
{
    public GameObject person_window;
    private Transform[] persons_children;
    int window_chilren_size;
    private void Awake()
    {
        persons_children = person_window.gameObject.GetComponentsInChildren<Transform>();
        window_chilren_size = persons_children.Length;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void on_close_button_click()
    {
        //Debug.Log("close 버튼눌림");
        Transform[] persons_children_copy = person_window.gameObject.GetComponentsInChildren<Transform>();
        for (int y = 0; y < window_chilren_size; y++)
        {
            persons_children_copy[y].gameObject.SetActive(false);
        }

    }
}
