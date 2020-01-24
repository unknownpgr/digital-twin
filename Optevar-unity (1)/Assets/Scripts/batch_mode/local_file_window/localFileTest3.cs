using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Windows;
public class localFileTest3 : MonoBehaviour
{
    public string ParentFolderName;
    public string TargetFolderName;
    public List<string> FilePathList;
    public List<string> CoverPathList;
    public List<Texture2D> CoverSpriteList;
    string filepath;
    void Awake() {
        GetPathList();
        Debug.Log("done");
    }

    void GetPathList()
    {
        string _path = "";

        //타켓 폴더 패스 설정
        if (Application.platform == RuntimePlatform.Android)
        {
            //android일 경우 //
            _path = AndroidRootPath() + "Download/FH/" + ParentFolderName + "/" + TargetFolderName;
        }
        else
        {
            //unity일 경우 //


            _path = "C://Users/Engineer/Optevar-unity/Assets/Resources/jsons";//resources/jsons/";//System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/Desktop/FH/" + ParentFolderName + "/" + TargetFolderName;
        }

      DirectoryInfo Folder = new DirectoryInfo(_path);

        //각 파일 패스(URL) 리스트 만들기
        foreach (var file in Folder.GetFiles())
        {
            filepath = _path + "/" + file.Name;
            Debug.Log(filepath);
            if (file.Extension != ".meta" && file.Extension != ".json")
            { //비디오 이외의 확장자를 가진 파일 제외시키기
                filepath = _path + "/" + file.Name;
                Debug.Log("*");
                Debug.Log(filepath);
                if (!filepath.Contains("._"))
                { //파일명 에러 수정해주기
                    // filepath = filepath.Replace ("._", "");
                    if (filepath.Contains(".mp4")) //비디오 파일 add 리스트
                        FilePathList.Add(filepath);
                    else if (filepath.Contains(".jpg"))
                    { //커버이미지 파일 add 리스트
                        CoverPathList.Add(filepath);
                        Texture2D tex = null;
                        byte[] filedata;
                        if (File.Exists(filepath))
                        {
                            filedata = File.ReadAllBytes(filepath);
                            tex = new Texture2D(2, 2);
                            tex.LoadImage(filedata);
                            // Sprite sp = SpriteFromTexture2D (tex);
                            CoverSpriteList.Add(tex);
                        }
                    }
                }
            }
        }
        //Debug.Log(ParentFolderName + "/" + TargetFolderName + ", FileCount : " + FilePathList.Count + ",, SpriteCount : " + CoverSpriteList.Count);
    }

    string AndroidRootPath()
    {
        string[] temp = (Application.persistentDataPath.Replace("Android", "")).Split(new string[] { "//" }, System.StringSplitOptions.None);
        return (temp[0] + "/");
    }

    Sprite SpriteFromTexture2D(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }
}
