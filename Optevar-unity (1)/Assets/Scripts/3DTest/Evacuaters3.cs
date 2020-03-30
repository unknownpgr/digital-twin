using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;

// Only dependant to simulationManager.
public class Evacuaters3
{
    int number;
    public int curNum = 0;
    int pathIdx;
    //grid
    List<Node3[]> paths;
    Node3[] curPath;  // All movable nodes.
    int curPIdx;
    int curPStart;
    float[][] distancesOfCell;
    float velocity = 1f; // std velo
    float upVelo = 0.3f;
    float downVelo = 0.38f;

    public bool isEvacuating;
    public float WaitingTime = 0f;
    public float NextActingTime = 0f;

    public Evacuaters3(int n, Vector3 start)
    {
        paths = new List<Node3[]>();
        number = n;
        curNum = n;
    }

    public void InitPath()
    {
        this.WaitingTime = this.distancesOfCell[pathIdx][0];
        this.NextActingTime = this.WaitingTime;
        this.curPath = paths[pathIdx];
        this.curNum = this.number;
        this.curPIdx = 0;
        this.curPStart = 0;
        this.isEvacuating = false;
    }
    public void SetVelocity(float _velo)
    {
        this.velocity = _velo;
    }
    public void SetPaths(List<Node3[]> p)
    {
        paths = p;
        pathIdx = 0;
        List<float[]> tmpF = new List<float[]>();
        for (int i = 0; i < p.Count; i++)
        {
            List<float> tmpF2 = new List<float>();
            float tmpF3 = 0f;
            float dist = 0f;
            float deg = 0f;
            bool isM;
            for (int j = 0; j < p[i].Length - 1; j++)
            {
                dist = p[i][j].GetRealDistance(p[i][j + 1]);
                tmpF3 = dist / this.velocity;
                deg = Mathf.Rad2Deg * (Mathf.Asin((Mathf.Abs(p[i][j + 1].position.y - p[i][j].position.y) / dist)));
                isM = (p[i][j + 1].position.y > p[i][j].position.y);
                if ((deg > 45f))
                {
                    if (isM)
                        tmpF3 /= this.upVelo;
                    else
                        tmpF3 /= this.downVelo;
                }
                tmpF2.Add(tmpF3);
            }

            tmpF2.Add(tmpF2[tmpF2.Count - 1]); // EOF
            tmpF.Add(tmpF2.ToArray());
        }
        this.distancesOfCell = tmpF.ToArray();
    }
    public Node3[] GetPath()
    {
        return paths[pathIdx];
    }
    public float GetDistance()
    {
        float ret = 0f;
        for (int i = 0; i < paths[pathIdx].Length - 1; i++)
            ret += paths[pathIdx][i].GetRealDistance(paths[pathIdx][i + 1]);
        return ret;
    }
    public bool NextPath()
    {
        pathIdx++;
        if (pathIdx == paths.Count)
        {
            pathIdx = 0;
            return true; // carry
        }
        else
        {
            return false; // not carry
        }
    }
    public void SetPath(int idx)
    {
        pathIdx = idx;
        curNum = number;
        for (int i = 0; i < number; i++)
        {

        }
    }
    public void SetPath()
    {
        curNum = number;
    }

    // float time 
    public void Update(float curTime, float second)
    {
        // 다음 칸 이동 가능?
        if (curPath.Length - 1 > curPIdx)
        {
            if (curPath[curPIdx + 1].weight == 0)
            {
                curPIdx++;
                curPath[curPIdx].weight = 1;
                if (curPIdx - curPStart >= number)
                {
                    // 꼬리 줄이기
                    curPath[curPStart].weight = 0;
                    curPStart++;
                }
                // this.WaitingTime = calc
                this.WaitingTime = this.distancesOfCell[pathIdx][curPIdx];
                this.NextActingTime = this.WaitingTime + curTime;
            }
            else
            {
                // 다음 차례 기다리기 (막혔을 경우임)
                this.NextActingTime = second + 0.01f; // 다음 실행 시각은 다 다음 차례 + 0.1초
            }
        }
        else
        {
            //// N: 경로의 끝?
            //////// Y: 꼬리 줄이기; 다음 이동 기다리기(yield return WaitTime());
            //////// N: 기다리기(yield return null);
            if (curPath.Length - 1 == curPIdx)
            {
                // 꼬리 줄이기
                curPath[curPStart].weight = 0;
                curPStart++;
                curNum--; // 탈출
                isEvacuating = true;
                //this.WaitingTime = this.distancesOfCell[pathIdx][curPIdx];
                this.NextActingTime = this.WaitingTime + curTime;
            }
            else
            {
                // 다음 차례 기다리기 (막혔을 경우임)
                this.NextActingTime = second + 0.01f; // 다음 실행 시각은 다 다음 차례 + 0.1초
            }

        }
        // 대피 완료시
        if (curPStart == curPath.Length)
        {
            this.WaitingTime = -1f;
        }

        //return null;

    }
}