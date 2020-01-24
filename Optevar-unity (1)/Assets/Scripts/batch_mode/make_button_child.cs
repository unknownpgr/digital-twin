using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class make_button_child : MonoBehaviour
{
    person_inputfield mkbt;
    private void Awake()
    {
        
    }
    void Start()
    {
        mkbt = GameObject.Find("person_window").GetComponent<person_inputfield>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void on_clicked_female_child()
    {
        mkbt.final_age = 10;
        mkbt.final_speed = 20;
        mkbt.age.text = mkbt.final_age.ToString();
        mkbt.speed.text = mkbt.final_speed.ToString();
    }
    public void on_clicked_male_child()
    {
        mkbt.final_age = 10;//person에 붙어있는 속성 scrip에 값넣기 위한 값
        mkbt.final_speed = 20;
        mkbt.age.text = mkbt.final_age.ToString();//UI의 inputfield에 속성값 넣기
        mkbt.speed.text = mkbt.final_speed.ToString();
        
    }
    public void on_clicked_female_adult()
    {
        mkbt.final_age = 25;
        mkbt.final_speed = 50;
        mkbt.age.text = mkbt.final_age.ToString();
        mkbt.speed.text = mkbt.final_speed.ToString();
    }
    public void on_clicked_male_adult()
    {
        mkbt.final_age = 25;
        mkbt.final_speed = 60;
        mkbt.age.text = mkbt.final_age.ToString();
        
        mkbt.speed.text = mkbt.final_speed.ToString();
        
    }
    public void on_clicked_female_older()
    {
        mkbt.final_age = 10;
        mkbt.final_speed = 30;
        mkbt.age.text = mkbt.final_age.ToString();
        
        mkbt.speed.text = mkbt.final_speed.ToString();
        
    }
    public void on_clicked_male_older()
    {
        mkbt.final_age = 10;
        mkbt.final_speed = 40;
        mkbt.age.text = mkbt.final_age.ToString();
        
        mkbt.speed.text = mkbt.final_speed.ToString();
        
    }
}
