using UnityEngine;
using System.IO;
using System;

public class JsonParser
{
    string path = "/Resources/scenario_jsons/";
    public void Save(object _obj, string _name)
    {
        Debug.Log("Saving json...");
        string js = JsonUtility.ToJson(_obj);
        FolderCheck(Application.dataPath + path);

        File.WriteAllText(Application.dataPath
                + path + _name + ".json"
                , js);
    }

    public T Load<T>(string _name)
    {
        Debug.Log("Loading json...");
        string jsStr = File.ReadAllText(Application.dataPath
            + path + _name + ".json");
        T jsObj = JsonUtility.FromJson<T>(jsStr);
        return jsObj;
    }

    
    public void Save2(object _obj, string _path)
    {
        Debug.Log("Saving json...");
        string js = JsonUtility.ToJson(_obj);
        File.WriteAllText(_path, js);
    }

    //pc모든곳에 있는 파일 불러오기
    public T Load2<T>(string _path)
    {
        Debug.Log("Loading json...");
        string jsStr = File.ReadAllText(_path);
        T jsObj = JsonUtility.FromJson<T>(jsStr);
        return jsObj;
    }
    public T Load3<T>(string _path)//return = T : Scenario
    {
        Debug.Log("Loading json...");
        //Debug.Log(_path);
        string jsStr = File.ReadAllText(_path);
        Wrapper<T> jsObj = JsonUtility.FromJson<Wrapper<T>>(jsStr);
        return jsObj.objects;
    }

    public void Save3<T>(T _obj, string _name)//배열 input값 받기위해 wrap함
    {
        Debug.Log("Saving json...");
        Wrapper<T> wrapper = new Wrapper<T>();

        wrapper.objects = _obj;// 4개 종류 배열이 들어있는 하나의 변수

        string js = JsonUtility.ToJson(wrapper);
        //File.WriteAllText(_path, js);
        Debug.Log(path);
        FolderCheck(Application.dataPath + path);
        File.WriteAllText(Application.dataPath
                + path + _name + ".json"
                , js);
        Debug.Log("파일생성됨");
    }
    void FolderCheck(string _path)
    {
        DirectoryInfo di = new DirectoryInfo(_path);
        if (di.Exists == false)
        {
            di.Create();
        }
    }
    [Serializable]
    public class Wrapper<T>//T:Scenario
    {
        public T objects;
    }
}
