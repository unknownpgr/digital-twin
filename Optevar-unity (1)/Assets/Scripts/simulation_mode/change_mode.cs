using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class change_mode : MonoBehaviour
{
    public GameObject batch_button;
    public GameObject simulate_button;
    Scene batch_scene;
    Scene main_scene;
    Scene simulate_scene;
    
    // Start is called before the first frame update

    /*
     batch_scene index : 1
     main_scene index : 2
     simulate_scene index : 3
        */
    void Awake() {
        //batch_scene = SceneManager.GetSceneByBuildIndex(1);
        //main_scene = SceneManager.GetSceneByBuildIndex(2);
        //simulate_scene = SceneManager.GetSceneByBuildIndex(3);
        batch_scene = SceneManager.GetSceneByName("batch_scene");
        main_scene = SceneManager.GetSceneByName("main_scene");
        simulate_scene = SceneManager.GetSceneByName("simulate_scene");
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //일단 카메라 하나로 합쳐야함 && UI위치 제대로 고정하기
    //UI가 어긋나있어서 안보인다
    public void active_batch_scene()
    {
        Debug.Log("batch눌림");
        SceneManager.LoadScene("batch_scene", LoadSceneMode.Additive);
        //Debug.Log(active_scenes.name);

        //SceneManager.MoveGameObjectToScene(batch_button, 1);
        //SceneManager.MoveGameObjectToScene(simulate_button, 1);
        if (batch_scene.IsValid())
        {
            SceneManager.MoveGameObjectToScene(batch_button, batch_scene);
            SceneManager.MoveGameObjectToScene(simulate_button, batch_scene);
        }
        else {
            Debug.Log("batch_scene이 load되지 않았습니다.");
        }
        if (simulate_scene.IsValid())
        {
            SceneManager.UnloadSceneAsync(3);
        }
        else {
            Debug.Log("simulate_scene unload하지 않음");
        }


        ///////////////////************그냥 batch_scene에도 batch, simulate 버튼 만들면 된다
        ///*/////////////////////////
        //tive_scenes = SceneManager.GetActiveScene(); 이거 안된다


    }
    public void active_simulate_scene()
    {
        Debug.Log("simulation눌림");
        SceneManager.LoadScene("simulate_scene", LoadSceneMode.Additive);
        
        if (simulate_scene.IsValid())
        {

        }
        else
        {
            Debug.Log("simulate_scene이 load되지 않았습니다.");
        }
        if (batch_scene.IsValid())
        {
            SceneManager.UnloadSceneAsync(1);
        }
        else
        {
            Debug.Log("batch_scene unload하지 않음");
        }

    }
    //https://debuglog.tistory.com/31 UI조절하는거
}
