using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTarget : MonoBehaviour
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
    public Transform Targets;

    // tmp for test
    List<int> tmpP = new List<int>();
    List<float> tmpF = new List<float>();
    string savep = "./Assets/Resources/test";
    float st = 0;
    List<Vector3> testP = new List<Vector3>();
    void Start()
    {
        pathfinderObj = GameObject.Find("GridManager");
        pathfinder = pathfinderObj.GetComponent<PathfinderController>();
        EvacuatersList = new List<Evacuaters>();
        if (Targets != null)
        {
            pathfinder.grid.TargetNodes = new List<Node>();
            for (int i = 0; i < Targets.transform.childCount; i++)
                pathfinder.grid.TargetNodes.Add(
                    pathfinder.grid.NodeFromWorldPosition(Targets.transform.GetChild(i).transform.position));
        }
        pathfinder.grid.StorePaths();
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
        if (st != 0)
        {
            Debug.Log("Simulation time: " + (Time.time - st));
            st = 0;
        }
        if (Input.GetMouseButtonDown(1)) {

            Vector3 mouse = Input.mousePosition;
            Ray castPnt = Camera.main.ScreenPointToRay(mouse);
            RaycastHit hit;
            if (Physics.Raycast(castPnt, out hit, Mathf.Infinity, (1 << LayerMask.NameToLayer("Obstacle"))))
            {
                Debug.Log("Obs");
            }
            st = Time.time;
            Debug.Log(Time.time);
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
            }
            else
            {
                NextRoute();
            }
            //while (!NextRoute()) ;
            //evacsNum = 0;
        }
        if (Input.GetMouseButtonDown(0))
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
            
            minTime = 100000f;
            pathfinder.grid.InitWeight();
            pathfinder.grid.InitDangerFlag();
            EvacuatersList.Clear();
            pathfinder.grid.InitLiner();
            //SetDanger(cast);
            IsEvacs = false;
            pathfinder.grid.ResetPaths();

            //
            tmpF.Clear();
            tmpP.Clear();
            savep += Time.time;
        }
    }

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
            } else
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
            obstacle.position = o.Position;
            Node s = new Node(false, o.Position, o.gridX, o.gridY);
            s.weight = 50;
            
            //obstacle.localScale = new Vector3(s.weight / pathfinder.grid.ConstantOfDistance,
            //    s.weight / pathfinder.grid.ConstantOfDistance, s.weight / pathfinder.grid.ConstantOfDistance);
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
