using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

class CompPV : IComparer<PrintableValue>
{
    public int Compare(PrintableValue a, PrintableValue b)
    {
        if (a.delay < b.delay) return -1;
        else if (a.delay == b.delay) return 0;
        else return 1;
    }
}

class PrintableValue
{
    public List<float> timestamp;
    public List<List<int>> evacs;
    public float delay;
    public PrintableValue(List<float> _timestamp, List<List<int>> _evacs, float _delay)
    {
        this.timestamp = _timestamp;
        this.evacs = _evacs;
        this.delay = _delay;
    }
    public int[] GetSumPrints(float maxTimeStamp, float delta)
    {
        
        int[] ret = new int[(int)(maxTimeStamp / delta)];
        int idx = 0;
        bool isEnd = false;
        for (int i = 0; i < (int)(maxTimeStamp / delta); i++)
        {
            if (!isEnd)
                while (this.timestamp[idx] <= i * delta) if (idx >= this.timestamp.Count - 1) { isEnd = true; break; } else idx++;
            ret[i] = 0;
            if (idx == 0)
                idx = 1;
            for (int e = 0; e < this.evacs.Count; e++)
                ret[i] += evacs[e][idx - 1];
            if (isEnd) idx = this.timestamp.Count;
        }
        return ret;
    }
}
public class SimulationManager3 : ScriptableObject
{
    float maxDelay = 1000f;
    public bool initEvacs = false;
    public bool isSimEnd = false;
    Grid3 grid;
    public List<Evacuaters3> EvacuatersList = new List<Evacuaters3>();

    float minTime = 10000f;
    int pathIdx = -1;
    int pathSize = 0;
    //List<int> tmpEvacNumList = new List<int>();
    //List<List<int>> evacNumList = new List<List<int>>();
    //List<List<List<int>>> PrintList = new List<List<List<int>>>();
    List<PrintableValue> PrintableList;
    int targetSize = 0;
    public List<float> delayList = new List<float>();

    string savePath;
    
    PriorityQueue simQ;
    


    // Set variables
    public void SetGrid(Grid3 grid)
    {
        this.grid = grid;
    }

   // Simulate
   // 1. Set evacuaters.
   // 2. Update sensor data.
   // 3. Simulate.

    public void AddEvacuater(Vector3 area, int nums, List<Node3[]> paths)
    {
        if (nums > 0)
        {
            Evacuaters3 sc = new Evacuaters3(
                nums, area, grid);
            sc.SetVelocity(4);
            sc.SetPaths(paths);
            EvacuatersList.Add(sc);
            targetSize = paths.Count;
        }
    }

    public void AddEvacuater(Vector3 area, int nums, List<Node3[]> paths, float _velo)
    {
        if (nums > 0)
        {
            Evacuaters3 sc = new Evacuaters3(
                nums, area, grid);
            sc.SetVelocity(_velo);
            sc.SetPaths(paths);
            EvacuatersList.Add(sc);
            targetSize = paths.Count;
        }
    }

    public void InitSimParam(int _pathSize)
    {
        InitSimQueue();
        InitPathSize(_pathSize);
        InitPrintableValue();

    }
    void InitSimQueue()
    {
        simQ = new PriorityQueue(EvacuatersList.Count, CompSimQ);
    }
    void InitPathSize(int _pathSize)
    {
        this.pathSize = _pathSize;
    }
    void InitPrintableValue()
    {
        PrintableList = new List<PrintableValue>(this.pathSize);
        savePath = Application.dataPath + "/Resources/";
    }
    /* DEPRECATED
    public void SetEvacuaters(List<AreaPositions> areas, Dictionary<int, int> areaNums)
    {
        EvacuatersList.Clear();
        for (int i = 0; i < areas.Count; i++)
        {

            if (areaNums[i] > 0)
            {
                Evacuaters3 sc = new Evacuaters3(
                    areaNums[i], areas[i].position, grid);
                
                sc.SetVelocity(4);
                EvacuatersList.Add(sc);
                //Calc(sc); // Get stored paths from start position and Store into the Evacuaters.
            }
        }
    }
    */

   
   // Return path-time list.

        /*DEPRECATED
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
    */

        
        
    // Progress():
    // Check Evacuater start?
    // Y...NextRoute()
    // N...Init()
    bool IsEvacs = false;
    public bool Progress()
    // true -> simulation ends up for all paths.
    // false -> It has next path.
    {
        if (pathIdx == -1)
        {
            pathIdx = 0;
            //grid.ResetFinalPaths();
            for (int o = 0; o < EvacuatersList.Count; o++)
            {
            //    EvacuatersList[o].SetPath(0);
            //    grid.AddPath(EvacuatersList[o].GetPath(0));

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
            List<float> timeList = new List<float>();
            List<List<int>> evacNumList = new List<List<int>>();
            for (int i = 0; i < EvacuatersList.Count; i++)
                evacNumList.Add(new List<int>());


            // 20191118 수정할 시뮬레이션 모듈
            // 우선순위 큐를 이용한다.
            // while 문 내부에서 우선순위 큐에 들어간 것들을 모두 소비
            // Evacuaters3.Update(): 움직임 -> 자기 WaitingTime 갱신
            // WaitingTime은 다음 움직일 절대시간임. (상대시간이 아님에 유의)
            // 우선순위큐에서 같은 Evac은 들어온 순서대로 처리되어야 함. 선입선출.
            // while 문 앞에 시뮬레이션 절대시간 변수 time를 선언해서 활용해야겠다.

            // while: if (PQ) time += PQ.top().Update(); PQ.pop(); PQ.push(PQ.top()); 

            // 시뮬레이션 결과로 얻어질 변수들은?
            // // 해당 시뮬레이션의 시간 경과, 중간중간의 대피자 인원 수 현황
            // TODO: 우선순위 큐 초기화 과정 추가할 것.

            // Modified simulation start
            simQ.Clear();
            for (int i = 0; i < EvacuatersList.Count; i++)
            {
                EvacuatersList[i].InitPath();
                simQ.Enqueue(EvacuatersList[i]);

            }
            timeList.Add(t);
            for (int i = 0; i < EvacuatersList.Count; i++)
                evacNumList[i].Add(EvacuatersList[i].curNum);
            while (simQ.Count() > 0)
            {
                Evacuaters3 tmpEvac = (Evacuaters3)simQ.Dequeue();
                if (simQ.Count() == 0)
                    tmpEvac.Update(t, t+0.01f);
                else
                    tmpEvac.Update(t, ((Evacuaters3)simQ.Peek()).NextActingTime);
                if (t < tmpEvac.NextActingTime)
                    t = tmpEvac.NextActingTime;
                // For printing
                if (tmpEvac.isEvacuating)
                {
                    tmpEvac.isEvacuating = false;
                    timeList.Add(t);
                    for (int i = 0; i < EvacuatersList.Count; i++)
                        evacNumList[i].Add(EvacuatersList[i].curNum);
                }
                
                //

                if (tmpEvac.WaitingTime > 0f) // 한 대피 그룹의 대피 완료 조건
                    simQ.Enqueue(tmpEvac);
                else
                {
                    t += 0;
                }
                if (t > maxDelay)
                    simQ.Clear();
            }
            grid.InitWeight();
            // simulation end

            /*DEPRECATED// simulation start
            while (!IsEvacs)
            {

                int tm = MoveInvoke(dt);
                if (tm < tmi) tmi = tm;
                tml.Add(tmi);
                for (int i = 0; i < EvacuatersList.Count; i++)
                    evacNumList[i].Add(EvacuatersList[i].curNum);
                t += dt;
                if (t > 1000f)
                {
                    IsEvacs = true;
                    grid.InitWeight();
                }
            }
            // sim end
            */
            /* DEP
            if (t < 999f)
            {
                tmpEvacNumList.AddRange(tml);
            }
            else
            {
                tmpEvacNumList.Add(0);
            }
            PrintList.Add(evacNumList); */
            PrintableList.Add(new PrintableValue(timeList, evacNumList, t));
            delayList.Add(t);
        }
        CheckEvacs(t);
    }
    // 현재 경로가 최적 경로인 지 시간으로 확인하는 코드
    // 경로의 시각화 기능 포함
    void CheckEvacs(float tmpTime)
    {
        {
            float DistSum = 0f;
            grid.FinalPaths.Clear();
            for (int i = 0; i < EvacuatersList.Count; i++)
            {
                grid.FinalPaths.Add(EvacuatersList[i].GetPath());
                DistSum += EvacuatersList[i].GetDistance();
            }
            if (tmpTime < minTime)
            {
                minTime = tmpTime;
                grid.MinPaths.Clear();
                for (int i = 0; i < EvacuatersList.Count; i++)
                {
                    grid.MinPaths.Add(EvacuatersList[i].GetPath());
                }
            }
            Debug.LogWarning("Path " + pathIdx + ": " + EvacuatersList.Count + " peoples each point ..." + " Evacuation Time: " + tmpTime + "sec for Distance: " + DistSum);
            grid.Liner();
        }
    }
    

    // 일정 시간의 움직임을 업데이트함
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

    // 다음 경로 시뮬레이션을 실행함
    bool NextRoute()
    {
        //    for (int i = 0;)
        pathIdx++;
        if (pathSize == pathIdx)
        {
            pathIdx = -1;
            isSimEnd = true;
            return true;
        }
        IsEvacs = false;
        for (int i = 0; i < EvacuatersList.Count; i++)
        {
            if (!EvacuatersList[i].NextPath())
            {
                //set next path! (Not carried)
                //grid.InitWeight();
                for (int o = 0; o < EvacuatersList.Count; o++)
                {
                    EvacuatersList[o].SetPath();
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

        //pathfinder.grid.InitWeight();
        for (int o = 0; o < EvacuatersList.Count; o++)
        {
            EvacuatersList[o].SetPath(0);
        }
        Moves();
        return false;

    }



    // TODO 20191120: 시뮬레이션 알고리즘을 수정함에 따라 데이터 구조가 바뀌었기 때문에 이 함수도 수정되어야 함.
    // 사용할 변수들:
    // 1. delayList: 
    public void PrintOut(string _text)
    {
        bool sumPrinting = true;
        string tmps = "";
        float tmpMax = -1f; // max header

        /* For all data
         delay,P1E1,P1E2,P1E3,P1Sum,P2E1,P2E2, ...
         0,10,10,10,30,10,...
         0.1,10,10,10,30,10,...
         0.2,10,9,10,29,10,...
         ...     
         */
        /* For sum data
        delay,P1Sum,P2Sum, ...
        0,30,30,...
        0.1,30,29,...
        0.2,29,27,...
        ...     
        */
        if (sumPrinting)
        {
            float delta = 0.1f;
            int lastRank = 5;
            if (PrintableList.Count < lastRank) lastRank = PrintableList.Count;
            PrintableList.Sort(new CompPV());
            for (int i = 0; i < lastRank; i++) if (tmpMax < PrintableList[i].delay & PrintableList[i].delay < maxDelay) tmpMax = PrintableList[i].delay;

            //Add delays
            tmps += ",";
            for (int i = 0; i < lastRank; i++)
                tmps += PrintableList[i].delay.ToString("N2") + ",";
            tmps += "\n";

            //Add header
            tmps += ",";
            for (int i = 0; i < lastRank; i++)
                tmps += "P" + (i + 1).ToString() + "Sum,";
            tmps += "\n";
            
            //Add data
            int[][] tmpData = new int[lastRank][];
            for (int j = 0; j < lastRank; j++)
            {
                tmpData[j] = PrintableList[j].GetSumPrints(tmpMax, delta);

            }
            for (int i = 0; i < (int)(tmpMax / delta); i++)
            {
                tmps += (i*delta).ToString("N1") + ",";
                for (int j = 0; j < lastRank; j++)
                {
                    tmps += tmpData[j][i].ToString() + ","; 
                }
                tmps += "\n";
            }
        }
        else
        {

        }
        if (_text == "") _text = "test";
        System.IO.File.WriteAllText(savePath + _text + ".csv", tmps);
        /* dep
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
        */
        
    }

    int CompSimQ(object a, object b)
    {
        if (((Evacuaters3)a).NextActingTime < ((Evacuaters3)b).NextActingTime) return -1;
        else if (((Evacuaters3)a).NextActingTime == ((Evacuaters3)b).NextActingTime) return 0;
        else return 1;
    }
}