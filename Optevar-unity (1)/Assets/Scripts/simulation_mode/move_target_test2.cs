using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class move_target_test2 : MonoBehaviour
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

    // tmp for test
    List<int> tmpP = new List<int>();
    List<float> tmpF = new List<float>();
    string savep = "./Assets/Resources/test.csv";

    List<Vector3> testP = new List<Vector3>();



    /****************/
    float time;
    
    public Camera sub_camera;
    public Camera main_camera;
    private Texture2D scrshot_tecture;
    public GameObject image_panel;
    public GameObject contect;
    public Text time_text;
    public Image image_ob1;
    int mouseInterfaceFlag; // 0=defalut, 1=create, 2=simulate
    List<screenshot_attr> paths = new List<screenshot_attr>();
    screenshot_attr temp_path;


    void Start()
    {
        mouseInterfaceFlag = 0;
        pathfinderObj = GameObject.Find("GridManager");
        pathfinder = pathfinderObj.GetComponent<PathfinderController>();
        EvacuatersList = new List<Evacuaters>();

        /****************/
        main_camera.enabled = true;
        sub_camera.enabled = false;
        image_panel.active = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (mouseInterfaceFlag == 2)
        {

            if (NextRoute())
            {
                mouseInterfaceFlag = 0;
                pathfinder.grid.InitWeight();
                pathfinder.grid.InitDangerFlag();
                EvacuatersList.Clear();
                pathfinder.grid.InitLiner();
                IsEvacs = false;
                pathfinder.grid.ResetPaths();
                //paths.Clear();

            }

        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {//시뮬레이션 모든경로에 대해 한번에 돌리고 중간중간 log로 시간띄우기

                Vector3 mouse = Input.mousePosition;
                Ray castPnt = Camera.main.ScreenPointToRay(mouse);
                RaycastHit hit;

                if (Physics.Raycast(castPnt, out hit, Mathf.Infinity, hitLayers))
                {
                    if (startTime == 0) startTime = Time.time;

                }


                if (EvacuatersList.Count != evacsNum)
                {
                    IsEvacs = false;
                    pathfinder.grid.ResetPaths();
                    //pathfinder.grid.InitWeight();

                    pathIdx = 0;
                    evacsNum = EvacuatersList.Count;
                    targetNum = pathfinder.grid.TargetNodes.Count;
                    for (int o = 0; o < evacsNum; o++)
                    {
                        EvacuatersList[o].SetPath(0);
                        pathfinder.grid.AddPath(EvacuatersList[o].GetPath(0));

                    }
                    Moves();

                    

                    mouseInterfaceFlag = 2;
                }
                //else
                //{
                //    while (!NextRoute())
                //    {
                        //NextRoute();
                //    }
                //}


            }
            if (Input.GetMouseButtonDown(0))//대피자 객체생성
            {
                //if (startTime == 0) startTime = Time.time;
                Vector3 mouse = Input.mousePosition;
                Ray castPnt = Camera.main.ScreenPointToRay(mouse);
                RaycastHit hit;

                if (Physics.Raycast(castPnt, out hit, Mathf.Infinity, hitLayers))
                {
                    SetEvacuater(hit.point);

                }
            }
            if (Input.GetMouseButtonDown(2))
            {

                Ray cast = Camera.main.ScreenPointToRay(Input.mousePosition);

                minTime = 100000f;
                pathfinder.grid.InitWeight();
                pathfinder.grid.InitDangerFlag();
                EvacuatersList.Clear();
                pathfinder.grid.InitLiner();
                SetDanger(cast);
                IsEvacs = false;
                pathfinder.grid.ResetPaths();


            }
        }
    }
    /****************/
    
    IEnumerator screen_pixels()
    {
        

        sub_camera.enabled = true;
        image_panel.active = true;
        //scrshot_tecture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
        
        temp_path.scrshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);

        yield return new WaitForEndOfFrame();
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);//depth : 일단 24
        sub_camera.targetTexture = rt;
        //texture1
        sub_camera.Render();
        RenderTexture.active = rt;
        temp_path.scrshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        temp_path.scrshot.Apply();
        //Image_object1.GetComponent<MeshRenderer>().material.SetTexture("ScreenShot_texture1", Texture1);
        //Image_object1.GetComponent<Image>().material.SetTexture("ScreenShot_texture1", Texture1);
        //image_ob1.transform.SetParent(image_panel.transform, false);


        //image_panel
        //Debug.Log(image_panel.name);
        //Debug.Log("적용됨");

    }/*
    void screen_pixels()
    {


        sub_camera.enabled = true;
        image_panel.active = true;
        //scrshot_tecture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);

        temp_path.scrshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);

        //yield return new WaitForEndOfFrame();
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);//depth : 일단 24
        sub_camera.targetTexture = rt;
        //texture1
        sub_camera.Render();
        RenderTexture.active = rt;
        temp_path.scrshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        temp_path.scrshot.Apply();
        //Image_object1.GetComponent<MeshRenderer>().material.SetTexture("ScreenShot_texture1", Texture1);
        //Image_object1.GetComponent<Image>().material.SetTexture("ScreenShot_texture1", Texture1);
        //image_ob1.transform.SetParent(image_panel.transform, false);


        //image_panel
        //Debug.Log(image_panel.name);
        Debug.Log("적용됨");

    }*/

    void Moves()
    {
        if (EvacuatersList.Count > 0)
        {
            float dt = 0.1f;
            float t = 0f;
            List<int> tml = new List<int>();
            int tmi = 10000;
            while (!IsEvacs)
            {

                int tm = MoveInvoke(dt);
                if (tm < tmi) tmi = tm;
                tml.Add(tmi);
                t += dt;
                if (t > 1000f)
                {
                    IsEvacs = true;
                    pathfinder.grid.InitWeight();
                }
            }
            if (t < 999f)
            {
                tmpP.AddRange(tml);
            }
            else
            {
                tmpP.Add(0);
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
            pathfinder.grid.FinalPaths.Clear();
            for (int i = 0; i < evacsNum; i++)
                pathfinder.grid.FinalPaths.Add(EvacuatersList[i].GetPath());
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
            Debug.LogWarning("Path " + pathIdx + ": " + EvacuaterNum + " peoples each point ..." + " Evacuation Time: " + tmpTime + "sec.");
            string tmps = "";
            float tmpMax = -1f;
            tmpF.Add(tmpTime);
            for (int i = 0; i < tmpF.Count; i++) if (tmpMax < tmpF[i] & tmpF[i] < 100f) tmpMax = tmpF[i];
            int t = 1;
            tmps += t + "," + tmpF[0] + ",";

            

            for (int i = 0; i < tmpP.Count; i++)
            {
                tmps += tmpP[i] + ",";
                if (tmpP[i] == 0)
                {
                    tmps += "\n";
                    if (t < tmpF.Count)
                    {
                        tmps += (t + 1) + "," + tmpF[t] + ",";
                        t++;
                    }
                }

            }
            string tmps2 = ",,";
            tmpMax *= 10;
            tmpMax += 1;
            for (int i = 0; i < tmpMax; i++) tmps2 += (i * 0.1) + ",";
            tmps = tmps2 + '\n' + tmps;

            System.IO.File.WriteAllText(savep + ".csv", tmps);
            pathfinder.grid.Liner();
            startTime = 0f;

            /****************/
            temp_path = new screenshot_attr();
            temp_path.time = tmpTime;
            StartCoroutine(screen_pixels());
            //screen_pixels();
            sub_camera.enabled = false;
            paths.Add(temp_path);
            //index++;
            if (Mathf.Pow(targetNum, evacsNum) == paths.Count)
            {
                //여기에 띄우기

                //Debug.Log("총수" + paths.Count);

                paths.Sort(delegate (screenshot_attr x, screenshot_attr y) {
                    if (x.time > y.time) return 1;
                    else if (x.time < y.time) return -1;
                    return 0;
                });

                for (int r = 0; r < paths.Count; r++)
                {
                    Image new_image_ob;
                    Text new_text;
                    if (r == 0)
                    {
                        new_image_ob = image_ob1;
                        new_text = time_text;

                    }
                    else
                    {
                        new_image_ob = Instantiate(image_ob1, image_ob1.transform.position, Quaternion.identity);
                        new_text = Instantiate(time_text, image_ob1.transform.position, Quaternion.identity);
                    }
                    new_image_ob.transform.SetParent(contect.transform);
                    new_image_ob.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, image_ob1.rectTransform.rect.width);
                    new_image_ob.transform.localPosition = new Vector3(image_ob1.transform.localPosition.x, +image_ob1.transform.localPosition.y - (image_ob1.rectTransform.rect.height + 35) * r, image_ob1.transform.localPosition.z);
                    new_image_ob.transform.rotation = image_ob1.transform.rotation;
                    new_image_ob.transform.localScale = image_ob1.transform.localScale;
                    new_image_ob.GetComponent<Image>().sprite = Sprite.Create(paths[r].scrshot, new Rect(0, 0, Screen.width, Screen.height), new Vector2(0.5f, 0.5f));

                    new_text.transform.SetParent(contect.transform);
                    new_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, time_text.rectTransform.rect.width);
                    new_text.transform.localPosition = new Vector3(time_text.transform.localPosition.x, +time_text.transform.localPosition.y - (image_ob1.rectTransform.rect.height + 35) * r, time_text.transform.localPosition.z);
                    new_text.transform.rotation = time_text.transform.rotation;
                    new_text.transform.localScale = time_text.transform.localScale;
                    
                    new_text.GetComponent<Text>().text = "Time : " + paths[r].time.ToString();
                    //Debug.Log("r : " + r);
                    //Debug.Log("걸린시간 : " + paths[r].time);
                }
                
            }
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
    int MoveInvoke(float time)
    {
        int tmpd = 0;
        for (int i = 0; i < EvacuatersList.Count; i++)
        {
            int tmp = EvacuatersList[i].Update(time);
            tmpd += tmp;
        }
        if (tmpd <= 0) IsEvacs = true;
        return tmpd;
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
                pathfinder.grid.InitWeight();
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
            obstacle.position = new Vector3(hit.point.x, 0, hit.point.z);
            pathfinder.grid.Sensors = new Dictionary<int, Node>();
            Node o = pathfinder.grid.NodeFromWorldPosition(obstacle.position);
            Node s = new Node(false, o.Position, o.gridX, o.gridY);
            s.weight = 50;

            obstacle.localScale = new Vector3(s.weight / pathfinder.grid.ConstantOfDistance,
                s.weight / pathfinder.grid.ConstantOfDistance, s.weight / pathfinder.grid.ConstantOfDistance);
            pathfinder.grid.AddSensor(s);
            pathfinder.grid.UpdateWeight(pathfinder.grid.GetSensorSequence(), s.weight);
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
