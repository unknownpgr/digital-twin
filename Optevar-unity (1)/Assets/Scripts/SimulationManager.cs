using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SimulationManager : ScriptableObject
{


    CustomGrid grid;
    public List<Evacuaters> EvacuatersList = new List<Evacuaters>();

    float minTime = 10000f;
    int pathIdx = -1;
    List<int> tmpEvacNumList = new List<int>();
    List<List<int>> evacNumList = new List<List<int>>();
    List<List<List<int>>> PrintList = new List<List<List<int>>>();
    public List<float> delayList = new List<float>();
    
    string savePath = "./Assets/Resources/test";


    // Set variables
    public void SetGrid(CustomGrid grid)
    {
        this.grid = grid;
    }

   // Simulate
   // 1. Set evacuaters.
   // 2. Update sensor data.
   // 3. Simulate.
    public void SetEvacuaters(List<AreaPositions> areas, List<EvacuaterNodeJson> Evacs)
    {
        
    }

    public void SetEvacuaters(List<AreaPositions> areas, Dictionary<string, int> areaNums)
    {
        EvacuatersList.Clear();
        for (int i = 0; i < areas.Count; i++)
        {

            if (areaNums[areas[i].areaId] > 0)
            {
                Evacuaters sc = new Evacuaters(areaNums[areas[i].areaId]);
                sc.SetParams(areas[i].position, grid, new Vector3(0, 0, 0));
                sc.SetVelocity(4);
                EvacuatersList.Add(sc);
                Calc(sc); // Get stored paths from start position and Store into the Evacuaters.
            }
        }
    }

   
   // Return path-time list.

    void Calc(Evacuaters Evacs)
    {
        //Debug.Log("Position of StartNode: " + grid.NodeFromWorldPosition(StartPos.position).gridX + ", " + grid.NodeFromWorldPosition(StartPos.position).gridY);
        //grid.ResetFinalPaths();
        //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        //sw.Start();
        List<Node[]> tmpL = new List<Node[]>();
        for (int i = 0; i < grid.TargetNodes.Count; i++)
        {
            tmpL.Add(grid.GetStoredPathFromPosition(Evacs.GetPosition(), i));

        }
        Evacs.SetPaths(tmpL.ToArray());
        //grid.AddPath(Evacs.GetPath(0));
        //sw.Stop();

        //perform += sw.ElapsedMilliseconds;
        //performCount++;
        //Debug.Log(sw.ElapsedMilliseconds.ToString() + "ms");
        //Debug.Log("Perform: " + perform / performCount);
    }
    void Calc(Evacuaters Evacs, int exitNodeIdx)
    {
        //Debug.Log("Position of StartNode: " + grid.NodeFromWorldPosition(StartPos.position).gridX + ", " + grid.NodeFromWorldPosition(StartPos.position).gridY);
        //grid.ResetFinalPaths();
        //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        //sw.Start();
        List<Node[]> tmpL = new List<Node[]>();
        tmpL.Add(grid.GetStoredPathFromPosition(Evacs.GetPosition(), exitNodeIdx));
        Evacs.SetPaths(tmpL.ToArray());
        grid.AddPath(Evacs.GetPath(0));
        //sw.Stop();

        //perform += sw.ElapsedMilliseconds;
        //performCount++;
        //Debug.Log(sw.ElapsedMilliseconds.ToString() + "ms");
        //Debug.Log("Perform: " + perform / performCount);
    }

    // Progress():
    // Check Evacuater start?
    // Y...NextRoute()
    // N...Init()
    bool IsEvacs = false;
    public bool Progress()
    {
        if (pathIdx == -1)
        {
            pathIdx = 0;
            grid.ResetFinalPaths();
            for (int o = 0; o < EvacuatersList.Count; o++)
            {
                EvacuatersList[o].SetPath(0);
                grid.AddPath(EvacuatersList[o].GetPath(0));

            }
            Moves();
            return false;
        }
        else
        {
            return NextRoute();
        }
    }
    


    void Moves()
    {
        float t = 0f;
        if (EvacuatersList.Count > 0)
        {
            float dt = 0.1f;
            evacNumList = new List<List<int>>();
            for (int i = 0; i < EvacuatersList.Count; i++)
                evacNumList.Add(new List<int>());
            List<int> tml = new List<int>();
            int tmi = 10000;
            
            while (!IsEvacs)
            {

                int tm = MoveInvoke(dt);
                if (tm < tmi) tmi = tm;
                tml.Add(tmi);
                for (int i = 0; i < EvacuatersList.Count; i++)
                    evacNumList[i].Add(EvacuatersList[i].CurNum);
                t += dt;
                if (t > 1000f)
                {
                    IsEvacs = true;
                    grid.InitWeight();
                }
            }

            if (t < 999f)
            {
                tmpEvacNumList.AddRange(tml);
            }
            else
            {
                tmpEvacNumList.Add(0);
            }
            PrintList.Add(evacNumList);
            delayList.Add(t);
        }
        CheckEvacs(t);
    }
    void CheckEvacs(float tmpTime)
    {
        //if (IsEvacs)
        {
            //float t = Time.time;
            //if (t - startTime < minTime)
            float DistSum = 0f;
            grid.FinalPaths.Clear();
            for (int i = 0; i < EvacuatersList.Count; i++)
            {
                grid.FinalPaths.Add(EvacuatersList[i].GetPath());
                DistSum += EvacuatersList[i].GetDistance();
            }
            if (tmpTime < minTime)
            {
                //minTime = t - startTime;
                minTime = tmpTime;
                grid.MinPaths.Clear();
                for (int i = 0; i < EvacuatersList.Count; i++)
                {

                    grid.MinPaths.Add(EvacuatersList[i].GetPath());

                }
            }
            
            Debug.LogWarning("Path " + pathIdx + ": " + EvacuatersList.Count + " peoples each point ..." + " Evacuation Time: " + tmpTime + "sec for Distance: " + DistSum);


            /*
            string tmps = "";
            float tmpMax = -1f;
            for (int i = 0; i < delayList.Count; i++) if (tmpMax < delayList[i] & delayList[i] < 100f) tmpMax = delayList[i];
            int t = 1;
            tmps += t + "," + delayList[0] + ",";

            for (int i = 0; i < evacNumList.Count; i++)
            {
                tmps += evacNumList[i] + ",";
                if (evacNumList[i] == 0)
                {
                    tmps += "\n";
                    if (t < delayList.Count)
                    {
                        tmps += (t + 1) + "," + delayList[t] + ",";
                        t++;
                    }
                }

            }
            string tmps2 = ",,";
            tmpMax *= 10;
            tmpMax += 1;
            for (int i = 0; i < tmpMax; i++) tmps2 += (i * 0.1) + ",";
            tmps = tmps2 + '\n' + tmps;

            System.IO.File.WriteAllText(savePath + ".csv", tmps);
            */
            grid.Liner();
//startTime = 0f;
        }
    }
    public void PrintOut()
    {
        string tmps = "";
        float tmpMax = -1f; // max header
        for (int i = 0; i < delayList.Count; i++) if (tmpMax < delayList[i] & delayList[i] < 100f) tmpMax = delayList[i];
        

        for (int i = 0; i < PrintList.Count; i++)
        {
            
            for (int j = 0; j < PrintList[i].Count; j++)
            {
                tmps += (i + 1) + "," + delayList[i] + ",";
                for (int o = 0; o < PrintList[i][j].Count; o++)
                {
                    tmps += PrintList[i][j][o] + ",";
                }
                tmps += "\n";
            }
            //if (t < delayList.Count)
            //{
                //tmps += (t + 1) + "," + delayList[t] + ",";
            //    t++;
            //}

        }
        string tmps2 = ",,";
        tmpMax *= 10;
        tmpMax += 1;
        for (int i = 0; i < tmpMax; i++) tmps2 += (i * 0.1) + ",";
        tmps = tmps2 + '\n' + tmps;

        System.IO.File.WriteAllText(savePath + ".csv", tmps);
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

    bool NextRoute()
    {
        //    for (int i = 0;)
        pathIdx++;
        if (Mathf.Pow(grid.GetTargetNodes().Length, EvacuatersList.Count) == pathIdx)
        {
            //pathIdx = 0;
            return true;
        }
        IsEvacs = false;
        for (int i = 0; i < EvacuatersList.Count; i++)
        {
            if (!EvacuatersList[i].NextPath())
            {
                //set next path!
                grid.ResetFinalPaths();
                grid.InitWeight();
                for (int o = 0; o < EvacuatersList.Count; o++)
                {
                    EvacuatersList[o].SetPath();
                    grid.AddPath(EvacuatersList[o].GetPath());

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

        grid.ResetFinalPaths();
        //pathfinder.grid.InitWeight();
        for (int o = 0; o < EvacuatersList.Count; o++)
        {
            EvacuatersList[o].SetPath(0);
            grid.AddPath(EvacuatersList[o].GetPath());

        }
        Moves();
        return false;

    }
}