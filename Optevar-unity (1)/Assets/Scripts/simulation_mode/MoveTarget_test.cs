using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveTarget_test : MonoBehaviour
{
    public LayerMask hitLayers;
    public Transform start;
    public Transform obstacle;
    public GameObject pathfinderObj;
    PathfinderController pathfinder;
    public float TmpVelo = 1.0f;
    public float timeAdj = 1f;
    public int EvacuaterNum = 10;
    float startTime = 0f;
    float minTime = 100000f;
    float tmpTime = 0f;
    float accum = 0;
    int preIdx = 0;
    List<Evacuaters> EvacuatersList;
    bool IsEvacs;
    int pathIdx;
    int evacsNum;
    int targetNum;
    List<Vector3> testP = new List<Vector3>();
    
    //float[] time_array;
    //List<float> time_list;
    
    //screenshot_info paths;
    float time;
    //int index;
    int index = 0;
    public Camera sub_camera;
    public Camera simulation_camera;
    private Texture2D scrshot_tecture;
    public GameObject image_panel;
    public GameObject contect;
    public Image image_ob1;
    Image new_image_ob;
    /*
    Rect rScrollRect;  // 화면상의 스크롤 뷰의 위치
    Rect rScrollArea; // 총 스크롤 되는 공간
    Vector2 vScrollPos; // 스크롤 바의 위치
    Rect new_space;
    Rect space;
    */
    void Start()
    {
        pathfinderObj = GameObject.Find("GridManager");
        pathfinder = pathfinderObj.GetComponent<PathfinderController>();
        EvacuatersList = new List<Evacuaters>();

        simulation_camera.enabled = true;
        sub_camera.enabled = false;
        image_panel.active = false;
    }

    // Update is called once per frame
    void Update()
    {
        // move
        // check time

        /*
        // check is on path?
        if (pathfinder.grid.FinalPath != null)
        {
            // calc velocity to nodes per sec.
            //accum += pathfinder.GetNodeVelocity() * Time.deltaTime;
            accum += Time.deltaTime / 2000;
            //move

            if (accum > preIdx)
            {
                if (Mathf.RoundToInt(accum) - preIdx >= pathfinder.grid.FinalPath.Length)
                {
                    pathfinder.grid.FinalPath = null;
                    return;
                }
                Node moved = pathfinder.grid.FinalPath[Mathf.RoundToInt(accum) - preIdx];
                start.position = moved.Position;
                pathfinder.Calc();
                preIdx = Mathf.RoundToInt(accum);
            }

        }
        */
        //Move(timeAdj);


        if (Input.GetMouseButtonDown(1))
        {//시뮬레이션 모든경로에 대해 한번에 돌리고 중간중간 log로 시간띄우기

            Vector3 mouse = Input.mousePosition;
            Ray castPnt = Camera.main.ScreenPointToRay(mouse);
            RaycastHit hit;
            if (Physics.Raycast(castPnt, out hit, Mathf.Infinity, (1 << LayerMask.NameToLayer("Obstacle"))))
            {
                Debug.Log("Obs");
            }

            if (Physics.Raycast(castPnt, out hit, Mathf.Infinity, hitLayers))
            {
                //start.position = hit.point;
                if (startTime == 0) startTime = Time.time;
                /*
                SetEvacuater(hit.point);
                IsEvacs = false;
                pathIdx = 0;
                evacsNum = EvacuatersList.Count;
                targetNum = pathfinder.grid.TargetNodes.Count;
                for (int o = 0; o < evacsNum; o++)
                {
                    EvacuatersList[o].SetPath(0);
                    pathfinder.grid.AddPath(EvacuatersList[o].GetPath(0));

                }

            */
                //testP.Add(hit.point);

            }

            /*JsonParser jp = new JsonParser();
            NodePositions np = jp.Load<NodePositions>("test");
            EvacuatersList = new List<Evacuaters>();
            minTime = 100000f;
            for (int i = 0; i < np.positions.Length; i++)
                SetEvacuater(np.positions[i]);
            */
            IsEvacs = false;
            pathfinder.grid.ResetPaths();
            //pathfinder.grid.InitWeight();

            pathIdx = 0;
            /*
            //time_list = new List<float>();//**********
            paths.times = new List<float>();
            paths.indexes = new List<int>();*/
            evacsNum = EvacuatersList.Count;
            //time_array = new float[evacsNum];//걸린시간 받아오는 array
            targetNum = pathfinder.grid.TargetNodes.Count;
            for (int o = 0; o < evacsNum; o++)
            {
                EvacuatersList[o].SetPath(0);
                pathfinder.grid.AddPath(EvacuatersList[o].GetPath(0));

            }
            Moves();
            while (!NextRoute()) ;
            pathfinder.grid.Liner();
            //image_ob1 = image_panel.AddComponent<Image>();
            StartCoroutine(screen_pixels());
            sub_camera.enabled = false;
            /***********************
            paths.times.Sort();//index받아야함
            //paths.times.
            Debug.Log("실험갯수 : "+paths.times.Count);
            ****************************/

        }
        if (Input.GetMouseButtonDown(0))//대피자 객체생성
        {
            //if (startTime == 0) startTime = Time.time;
            Vector3 mouse = Input.mousePosition;
            Ray castPnt = Camera.main.ScreenPointToRay(mouse);
            RaycastHit hit;
            //SetDanger(castPnt, hit);
            //SavePositionsToJSON(testP.ToArray());
            if (Physics.Raycast(castPnt, out hit, Mathf.Infinity, hitLayers))
            {
                SetEvacuater(hit.point);

            }
        }
        if (Input.GetMouseButtonDown(2))
        {

            Ray cast = Camera.main.ScreenPointToRay(Input.mousePosition);
            SetDanger(cast);
            minTime = 100000f;
            pathfinder.grid.InitWeight();
            EvacuatersList.Clear();
            pathfinder.grid.InitLiner();
            IsEvacs = false;
            pathfinder.grid.ResetPaths();
        }
    }/*
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(100, 100, 250, Screen.height)); // 화면상의 100, 100, 400, 400 의 위치에 스크롤 공간을 잡는다.
        rScrollArea = new Rect(0, 0, 500, 700);      // 100, 100 을 기준으로, 0, 0, 500, 700 만큼의 스크롤 되는 content의 공간을 잡는다.

        // vScrollPos 는 현재 스크롤 바의 위치를 return받는다.
        //vScrollPos = GUI.BeginScrollView(rScrollRect, vScrollPos, rScrollArea);
        vScrollPos = GUILayout.BeginScrollView(vScrollPos, GUILayout.Width(145));
        StartCoroutine(screen_pixels());
        // todo : 여기에 스크롤 되는 컨텐츠를 넣어 스크롤 뷰에 표시할 수 있다.
        GUI.EndScrollView();
        GUILayout.EndArea();

    }
    IEnumerator screen_pixels()
    {
        sub_camera.enabled = true;
        scrshot_tecture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
        yield return new WaitForEndOfFrame();


        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);//depth : 일단 24
        sub_camera.targetTexture = rt;
        //texture1
        sub_camera.Render();
        RenderTexture.active = rt;
        scrshot_tecture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        scrshot_tecture.Apply();
        //Image_object1.GetComponent<MeshRenderer>().material.SetTexture("ScreenShot_texture1", Texture1);
        //Image_object1.GetComponent<Image>().material.SetTexture("ScreenShot_texture1", Texture1);
        //image_ob1.transform.SetParent(image_panel.transform, false);
        for (int n = 1; n < 10; n++)
        {
            Debug.Log("n : " + n);
            Debug.Log(image_ob1.name);
            //new_image_ob = Instantiate(space., image_ob1.transform.position, Quaternion.identity);
            GUILayout.Box(scrshot_tecture, GUILayout.Width(140));

            //new_image_ob.rectTransform.transform.localScale.Equals();
            //new_image_ob.transform.localPosition = new Vector3( image_ob1.transform.localPosition.x, + image_ob1.transform.localPosition.y - image_ob1.rectTransform.rect.height, image_ob1.transform.localPosition.z);
        }
        image_ob1.GetComponent<Image>().sprite = Sprite.Create(scrshot_tecture, new Rect(0, 0, Screen.width, Screen.height), new Vector2(0.5f, 0.5f));
        //image_panel
        //Debug.Log(image_panel.name);
        Debug.Log("적용됨");


    }*/
    IEnumerator screen_pixels() {
        sub_camera.enabled = true;
        image_panel.active = true;
        scrshot_tecture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
        yield return new WaitForEndOfFrame();

        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);//depth : 일단 24
        sub_camera.targetTexture = rt;
        //texture1
        sub_camera.Render();
        RenderTexture.active = rt;
        scrshot_tecture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        scrshot_tecture.Apply();
        //Image_object1.GetComponent<MeshRenderer>().material.SetTexture("ScreenShot_texture1", Texture1);
        //Image_object1.GetComponent<Image>().material.SetTexture("ScreenShot_texture1", Texture1);
        //image_ob1.transform.SetParent(image_panel.transform, false);
        for (int n=1; n<index; n++)
        {
            //Debug.Log("n : " + n);
            //Debug.Log(image_ob1.name);
            new_image_ob = Instantiate(image_ob1, image_ob1.transform.position, Quaternion.identity);
            new_image_ob.transform.SetParent(contect.transform);
            new_image_ob.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, image_ob1.rectTransform.rect.width);
            new_image_ob.transform.localPosition = new Vector3(image_ob1.transform.localPosition.x, +image_ob1.transform.localPosition.y - (image_ob1.rectTransform.rect.height+10)*n, image_ob1.transform.localPosition.z);
            new_image_ob.transform.rotation = image_ob1.transform.rotation;
            new_image_ob.transform.localScale = image_ob1.transform.localScale;
            new_image_ob.GetComponent<Image>().sprite = Sprite.Create(scrshot_tecture, new Rect(0, 0, Screen.width, Screen.height), new Vector2(0.5f, 0.5f));
            //new_image_ob.rectTransform.transform.localScale.Equals();
            //new_image_ob.transform.localPosition = new Vector3( image_ob1.transform.localPosition.x, + image_ob1.transform.localPosition.y - image_ob1.rectTransform.rect.height, image_ob1.transform.localPosition.z);
        }
        image_ob1.GetComponent<Image>().sprite = Sprite.Create(scrshot_tecture, new Rect(0, 0, Screen.width, Screen.height), new Vector2(0.5f, 0.5f));
        //image_panel
        //Debug.Log(image_panel.name);
        Debug.Log("적용됨");


    }

    void Moves()
    {
        if (EvacuatersList.Count > 0)
        {
            float dt = 0.1f;
            float t = 0f;
            while (!IsEvacs)
            {

                MoveInvoke(dt);
                t += dt;
                if (t > 1000f)
                {
                    IsEvacs = true;
                    pathfinder.grid.InitWeight();
                }
            }
            tmpTime = t;
        }
        CheckEvacs();
    }
    void CheckEvacs()
    {
        //if (IsEvacs)
        {
            //float t = Time.time;
            //if (t - startTime < minTime)
            if (tmpTime < minTime)
            {
                //minTime = t - startTime;
                minTime = tmpTime;
                pathfinder.grid.MinPaths.Clear();
                for (int i = 0; i < evacsNum; i++)
                {
                    pathfinder.grid.MinPaths.Add(EvacuatersList[i].GetPath());
                }
            }
            //temp_path = new screenshot_info();
            /************
            time = tmpTime;
            temp_path.index = index;
            //time_list.Add(tmpTime);
            
            paths.Add(temp_path);*/////////////////////////////////0
            index++;

            Debug.LogWarning("Path " + pathIdx + ": " + EvacuaterNum + " peoples each point ..." + " Evacuation Time: " + tmpTime + "sec.");
            startTime = 0f;
        }
    }
    void Move(float timeAdj)
    {
        IsEvacs = true;
        for (int i = 0; i < EvacuatersList.Count; i++)
        {
            IsEvacs &= (EvacuatersList[i].Update(Time.deltaTime * timeAdj) < 0);
        }
    }
    void MoveInvoke(float time)
    {
        IsEvacs = true;
        for (int i = 0; i < EvacuatersList.Count; i++)
        {
            IsEvacs &= (EvacuatersList[i].Update(time) < 0);
        }
    }

    void ResetEvacuater()
    {
        pathfinder.grid.ResetFinalPaths();
        pathfinder.grid.InitWeight();
        EvacuatersList = new List<Evacuaters>();

    }
    void SetEvacuater(Vector3 position)
    {

        //start.position = hit.point;
        //GameObject evac = GameObject.Instantiate(GameObject.Find("Weight"));
        //Evacuaters sc = (Evacuaters)evac.GetComponent(typeof(Evacuaters));
        Evacuaters sc = new Evacuaters(EvacuaterNum);
        sc.SetParams(position, pathfinder.grid, new Vector3(0, 0, 0));
        sc.SetVelocity(TmpVelo);
        EvacuatersList.Add(sc);
        pathfinder.Calc(sc);


    }


    bool NextRoute()
    {
        //    for (int i = 0;)
        pathIdx++;
        if (Mathf.Pow(targetNum, evacsNum) == pathIdx)
        {
            //pathIdx = 0;
            return true;
        }
        IsEvacs = false;
        for (int i = 0; i < evacsNum; i++)
        {
            if (!EvacuatersList[i].NextPath())
            {
                //set next path!
                pathfinder.grid.ResetFinalPaths();
                //pathfinder.grid.InitWeight();
                for (int o = 0; o < evacsNum; o++)
                {
                    EvacuatersList[o].SetPath();
                    pathfinder.grid.AddPath(EvacuatersList[o].GetPath());

                }
                Moves();
                return false;
            }
            else
            {
                // carried
                // Set least evac's pathidx = 0
                for (int j = i; j > 0; j--)
                {
                    EvacuatersList[j].SetPath(0);
                }
                continue;
            }

        }

        pathfinder.grid.ResetFinalPaths();
        //pathfinder.grid.InitWeight();
        for (int o = 0; o < evacsNum; o++)
        {
            EvacuatersList[o].SetPath(0);
            pathfinder.grid.AddPath(EvacuatersList[o].GetPath());

        }
        Moves();
        return false;
    }


    void SetDanger(Ray castPnt)
    {
        RaycastHit hit;
        if (Physics.Raycast(castPnt, out hit, Mathf.Infinity, hitLayers))
        {
            obstacle.position = hit.point;

            pathfinder.grid.InitWeight();
            pathfinder.grid.Sensors = new Dictionary<int, Node>();

            Node o = pathfinder.grid.NodeFromWorldPosition(obstacle.position);
            Node s = new Node(false, o.Position, o.gridX, o.gridY);
            s.weight = 50;
            pathfinder.grid.AddSensor(s);
            pathfinder.grid.UpdateWeight(pathfinder.grid.GetSensorSequence(), 100f);
            //pathfinder.Calc();
            pathfinder.grid.UpdatePaths();
            for (int i = 0; i < EvacuatersList.Count; i++)
                pathfinder.Calc(EvacuatersList[i]);
            //pathfinder.grid.StorePaths(pathfinder.grid.TargetNodes.ToArray());
            accum = 0;
            preIdx = 0;
        }
    }

    void SavePositionsToJSON(Vector3[] poss)
    {
        NodePositions np = new NodePositions();
        np.positions = poss;
        JsonParser jp = new JsonParser();
        jp.Save(np, "test");
    }
}
