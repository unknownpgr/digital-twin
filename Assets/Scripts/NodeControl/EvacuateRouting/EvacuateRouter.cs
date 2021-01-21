using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class is used to calculate optimal evacuate route.
 */
public class EvacuateRouter
{
    private const float VELOCITY = 4.0f; // How fast evacuators are? (Unit: m/s)
    private const float DENCITY = 1.0f;  // How many people can be in 1m? (Unit: head/m)


    private const int AREA_UNCHECKED = 0x01;
    private const int AREA_CHECKED = 0x02;

    private readonly Dictionary<string, int> nodeToIndex = new Dictionary<string, int>();
    private readonly List<NodeArea> areas;
    private readonly List<NodeExit> exits;

    // distance[start][end] = distance between start and end
    private readonly float[][] distanceMap;
    private readonly Vector3[][][] routeMap;

    private readonly int[] areaState;
    private readonly float[] optimalTime;

    private readonly List<EvacuateRoute> routes = new List<EvacuateRoute>();

    // Check if simulation conducted.
    private bool isUsed = false;

    public EvacuateRouter()
    {
        // Initialzie nodeToIndex dictionary and distance 2D array.

        // nodeToIndex dictionary can be used for both NodeArea and NodeExit
        // because physical id is unique for all NodeManagers.
        areas = NodeManager.GetNodesByType<NodeArea>();
        exits = NodeManager.GetNodesByType<NodeExit>();

        distanceMap = new float[areas.Count][];
        routeMap = new Vector3[areas.Count][][];

        areaState = new int[areas.Count];
        optimalTime = new float[exits.Count];

        for (int i = 0; i < areas.Count; i++)
        {
            nodeToIndex[areas[i].PhysicalID] = i;
            distanceMap[i] = new float[exits.Count];
            routeMap[i] = new Vector3[exits.Count][];
            areaState[i] = AREA_UNCHECKED;

            for (int j = 0; j < exits.Count; j++)
            {
                distanceMap[i][j] = float.MaxValue;
                routeMap[i][j] = null;
            }
        }
        for (int i = 0; i < exits.Count; i++)
        {
            nodeToIndex[exits[i].PhysicalID] = i;
        }
    }

    private void ExceptIfUsed()
    {
        // To make debugging easier, An evacuate router can be used only once.
        if (isUsed) throw new Exception("This router has been already used. An evacuate router can be used only once.");
    }

    private void ExceptINotfUsed()
    {
        if (!isUsed) throw new Exception("The routes are not calculated yet. This method can be called after routes are calculated.");
    }

    private static float GetRouteLength(Vector3[] path)
    {
        float length = 0;
        for (int i = 0; i < path.Length - 1; i++)
        {
            length += Vector3.Distance(path[i], path[i + 1]);
        }
        return length;
    }

    public void AddRoute(NodeArea start, NodeExit end, Vector3[] route)
    {
        // Because a router can be used only once, it cannot be modified after the simulation.
        ExceptIfUsed();

        int startIndex = nodeToIndex[start.PhysicalID];
        int endIndex = nodeToIndex[end.PhysicalID];


        routeMap[startIndex][endIndex] = route;
        distanceMap[startIndex][endIndex] = GetRouteLength(route);
    }

    public EvacuateRoute[] CalculateOptimalRoute()
    {
        string distMapStr = "";
        for (int i = 0; i < areas.Count; i++)
        {
            for (int j = 0; j < exits.Count; j++)
            {
                distMapStr += distanceMap[i][j] + "\t";
            }
            distMapStr += "\n";
        }
        Debug.Log(distMapStr);


        ExceptIfUsed();
        isUsed = true;

        /*
         * Let areasCount = N and exitsCount = M.
         * Then, there are M^N available routing cases.
         * Therefore, the time complexity for naive algorithm is O(M^N), which is extreamly large.
         * If N=M=18, It would take than several minutes and if N=M=10, It may take more than several hours.
         * 
         * It is unrealistic; cannot be used.
         * Therefore, use greedy algorithm instead.
         * 
         * 1. Assume that there are only one group.
         * 2. Find the group that can be completly evacuated first.
         * 3. Considering the first group, among the rest of the groups, find the group that can evacuate the first.
         * 4. Repeat above steps until find routes for all groups.
         * 
         * 'Considering the first group' in Step 3 means :
         * When a group reached at a exit,
         *   a. Suppose that there are no group reached at the exit.
         *     - Then this group can just go.
         *   b. Suppose that there are alreay a group arrived at the exit.
         *     - This group goes behind the group that arrived first.
         *     - The time it takes is mathematically same as when the both group use exit at the same time
         *       with time division algorithm, such as round-robin.
         * 
         * Time complexity of this algorithm is O(N*N*M), which is much less then O(N^M).
         * For N=M=50, calculation will be end in several milliseconds.
         */

        for (int _ = 0; _ < areas.Count; _++)
        {
            int currentOptimalArea = -1;
            int currentOptimalExit = -1;
            float currentOptimalTime = float.MaxValue;

            for (int area = 0; area < areas.Count; area++)
            {
                // Skip already processed area
                if (areaState[area] != AREA_UNCHECKED) continue;

                for (int exit = 0; exit < exits.Count; exit++)
                {
                    /*
                     * Calculate the time takes to the first evacuator reaches at exit.
                     * reachTime : The time it takes the first person in the group to reach the exit.
                     * evacuateTime : The time it takes for everyone in the group to evacuate after the first person reached at exit.
                     * 
                     * Suppose that a group is consist of N people and the dencity of this group is D.
                     * Therefore, the length of the group would be N/D.
                     * Suppose that this group moves at a consistance velocity of V m/s.
                     * Therefore the time it takes for the whole group evacuate is (N/D)/(V).
                     */
                    float reachTime = distanceMap[area][exit] / VELOCITY;
                    float evacuateTime = Math.Max(optimalTime[exit], reachTime) + (areas[area].Num / DENCITY)/VELOCITY;
                    if (evacuateTime < currentOptimalTime)
                    {
                        currentOptimalArea = area;
                        currentOptimalExit = exit;
                        currentOptimalTime = evacuateTime;
                    }
                }
            }

            areaState[currentOptimalArea] = AREA_CHECKED;

            if (currentOptimalArea >= 0)
            {
                optimalTime[currentOptimalExit] = currentOptimalTime;
                routes.Add(new EvacuateRoute(
                    areas[currentOptimalArea],
                    exits[currentOptimalExit],
                    routeMap[currentOptimalArea][currentOptimalExit],
                    currentOptimalTime
               ));
            }
            else
            {
                break;
            }
        }

        return GetRoutes();
    }

    public EvacuateRoute[] GetRoutes()
    {
        ExceptINotfUsed();
        routes.Sort();
        return routes.ToArray();
    }
}