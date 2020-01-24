using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Windows;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class localFileTest_4 : MonoBehaviour
{
    public List<string> file_path_list;
    public List<string> file_list;
    string file_path;
    public Button ori_file_name_button;
    public GameObject file_panel;
    public string clicked_button_text;
    public List<Transform> file_list_button;


    void Start()
    {
        //GetPathList();
        //Debug.Log("done");
    }
    public void GetPathList(List<string> lists)
    {
        ori_file_name_button.onClick.AddListener(file_name_clicked);
        DrawLists(lists);
    }
    public void GetPathList()
    {
        ori_file_name_button.onClick.AddListener(file_name_clicked);

        string path = Application.dataPath + "/Resources/scenario_jsons";
        
        DirectoryInfo folder = new DirectoryInfo(path);
        file_list.Clear();
        file_path_list.Clear();
        if (file_list_button.Count>0) {
            for (int y = 0; y < file_list_button.Count; y++)
            {
                Destroy(file_list_button[y].gameObject);

            }
        }

        //파일 리스트
        foreach (var file in folder.GetFiles("*.json"))
        {
            file_path = path + "/" + file.Name;
            
            file_path_list.Add(file_path);
            file_list.Add(file.Name);
            
        }
        
        DrawLists(this.file_list);
    }

    void DrawLists(List<string> _list)
    {
        for (int r = 0; r < _list.Count; r++)//file_list.Count; r++)
        {
            Button new_button;
            if (r == 0)
            {
                ori_file_name_button.GetComponentInChildren<Text>().text = _list[r];
            }
            else
            {
                new_button = Instantiate(ori_file_name_button, ori_file_name_button.transform.position, Quaternion.identity);
                new_button.transform.SetParent(file_panel.transform);
                new_button.GetComponentInChildren<Text>().text = _list[r];
                new_button.transform.localScale = ori_file_name_button.transform.localScale;
                new_button.transform.rotation = ori_file_name_button.transform.rotation;
                new_button.onClick.AddListener(file_name_clicked);
                file_list_button.Add(new_button.transform);
            }

        }
    }
    public void file_name_clicked() {//파일이름이 클릭될 때 button에 달아놓은 listener실행
        clicked_button_text = Application.dataPath + "/Resources/scenario_jsons/" + EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text;//이게 클릭된 버튼에 제대로 접근하는지 확인

    }
    public string get_clicked_file_name() {
        return clicked_button_text;
    }
    public void close_window() {

    }

    

}
