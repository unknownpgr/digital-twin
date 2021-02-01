using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class is used to calculate optimal evacuate route.
 */
public class EvacuateRouter
{
    private const float VELOCITY = 4.0f; // How fast evacuators are? (Unit: m/s)
    private const float SPACE = 2.0f;  // How large space a person takes? (Unit: m/head)

    private readonly Dictionary<string, int> nodeToIndex = new Dictionary<string, int>();
    private readonly List<NodeArea> areas = new List<NodeArea>();
    private readonly List<NodeExit> exits = new List<NodeExit>();

    // distance[start][end] = distance between start and end
    private readonly float[][] distanceMap;
    private readonly Vector3[][][] routeMap;

    private readonly List<EvacuateScenario> scenarios = new List<EvacuateScenario>();

    // Check if simulation conducted.
    private bool isUsed = false;

    public EvacuateRouter()
    {
        // Initialzie nodeToIndex dictionary and distance 2D array.

        // nodeToIndex dictionary can be used for both NodeArea and NodeExit
        // because physical id is unique for all NodeManagers.

        // routeMap[area][exit]    = route from area to exit
        // distanceMap[area][exit] = length of routeMap[area][exit]

        int areasCount = NodeManager.GetNodesByType<NodeArea>().Count;
        int exitsCount = NodeManager.GetNodesByType<NodeExit>().Count;

        routeMap = new Vector3[areasCount][][];
        distanceMap = new float[areasCount][];

        for (int i = 0; i < areasCount; i++)
        {
            distanceMap[i] = new float[exitsCount];
            routeMap[i] = new Vector3[exitsCount][];
            for (int j = 0; j < exitsCount; j++)
            {
                distanceMap[i][j] = -1;
                routeMap[i][j] = null;
            }
        }
    }

    private void ExceptIfUsed()
    {
        // To make debugging easier, An evacuate router can be used only once.
        if (isUsed) throw new Exception("This router has been already used. An evacuate router can be used only once.");
    }

    private void ExceptIfNotUsed()
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

        if (route.Length < 2) throw new Exception("1-point or 0-point route added. A route must have more than 1 points.");

        int startIndex, endIndex;
        if (nodeToIndex.ContainsKey(start.PhysicalID)) startIndex = nodeToIndex[start.PhysicalID];
        else
        {
            startIndex = areas.Count;
            nodeToIndex[start.PhysicalID] = startIndex;
            areas.Add(start);
        }
        if (nodeToIndex.ContainsKey(end.PhysicalID)) endIndex = nodeToIndex[end.PhysicalID];
        else
        {
            endIndex = exits.Count;
            nodeToIndex[end.PhysicalID] = endIndex;
            exits.Add(end);
        }

        routeMap[startIndex][endIndex] = route;
        distanceMap[startIndex][endIndex] = GetRouteLength(route);
    }

    /*
     * Get senarios sorted by its required time
     */
    public EvacuateScenario[] CalculateScenarios()
    {
        ExceptIfUsed();
        isUsed = true;

        // Search all cases
        int[] exitForArea = new int[areas.Count];
        int end = areas.Count - 1;
        for (; exitForArea[end] < exits.Count;)
        {
            bool isSenarioAvailable = true;
            for (int area = 0; area < areas.Count; area++)
            {
                if (distanceMap[area][exitForArea[area]] < 0)
                {
                    isSenarioAvailable = false;
                    break;
                }
            }

            if (isSenarioAvailable)
            {
                scenarios.Add(new EvacuateScenario(this, (int[])exitForArea.Clone()));
            }

            // Update case here
            exitForArea[0]++;
            for (int i = 0; i < end; i++)
            {
                if (exitForArea[i] == exits.Count)
                {
                    exitForArea[i] = 0;
                    exitForArea[i + 1]++;
                }
                else if (exitForArea[i] > exits.Count)
                {
                    throw new Exception("An impossible routing case exists.");
                }
            }
        }

        return GetScenarios();
    }

    public EvacuateScenario[] GetScenarios()
    {
        ExceptIfNotUsed();

        // Sort scenarios by required evacuation time
        scenarios.Sort();

        // Then remove same evacuateion time required scenarios
        List<EvacuateScenario> unduplicatedList = new List<EvacuateScenario>();
        unduplicatedList.Add(scenarios[0]);
        foreach (var scenario in scenarios)
        {
            float delta = Math.Abs(unduplicatedList[unduplicatedList.Count - 1].RequiredTime - scenario.RequiredTime);
            if (delta < 0.001f) continue;
            else unduplicatedList.Add(scenario);
        }
        return unduplicatedList.ToArray();
    }

    public class EvacuateScenario : IComparable<EvacuateScenario>
    {
        private readonly int[] exitForArea;
        private readonly EvacuateRouter router;

        public Vector3[][] Routes
        {
            get
            {
                List<Vector3[]> routes = new List<Vector3[]>();
                for (int area = 0; area < router.areas.Count; area++)
                {
                    if (router.areas[area].Num <= 0) continue;
                    routes.Add(router.routeMap[area][exitForArea[area]]);
                }
                return routes.ToArray();
            }
        }

        // Time it takes for all personnel to evacuate
        public float RequiredTime { get; } = -1;
        // Average time for each group to evacuate
        public float AverageEvacuateTime { get; } = -1;

        public Texture2D Screenshot { get; set; }

        public EvacuateScenario(EvacuateRouter router, int[] exitForArea)
        {
            this.router = router;
            this.exitForArea = exitForArea;

            float[] totalArriveTime = new float[router.exits.Count];   // Time takes to first person arrive at exit
            float[] totalCompleteTime = new float[router.exits.Count]; // Time takes to all people evacuate.

            for (int area = 0; area < router.areas.Count; area++)
            {
                // Pass empty area
                if (router.areas[area].Num <= 0) continue;

                int exit = exitForArea[area];
                float currentArriveTime = router.distanceMap[area][exit] / VELOCITY;
                float currentEvacuatTime = (router.areas[area].Num * SPACE) / VELOCITY;
                if (totalCompleteTime[exit] < 0.01f)
                {
                    // Case A. Initial case. No group reached this exit.
                    totalCompleteTime[exit] = currentArriveTime;
                    totalCompleteTime[exit] = currentArriveTime + currentEvacuatTime;
                }
                else if ((currentArriveTime + currentEvacuatTime) < totalArriveTime[exit])
                {
                    // Case B. Groups do not duplicates and current group comes first.
                    totalArriveTime[exit] = currentArriveTime;
                }
                else if (currentArriveTime > totalCompleteTime[exit])
                {
                    // Case C. Groups do not duplicates and current group comes last.
                    totalCompleteTime[exit] = currentArriveTime + currentEvacuatTime;
                }
                else
                {
                    // Case D. Groups duplicates.
                    totalArriveTime[exit] = Math.Min(totalArriveTime[exit], currentArriveTime);
                    totalCompleteTime[exit] += currentEvacuatTime;
                }

                RequiredTime = Math.Max(RequiredTime, totalCompleteTime[exit]);
                float avrTime = 0;
                foreach (float t in totalArriveTime) avrTime += t;
                foreach (float t in totalCompleteTime) avrTime += t;
                AverageEvacuateTime = avrTime / 2 / router.exits.Count;
            }
        }

        public int CompareTo(EvacuateScenario other)
        {
            return (int)((RequiredTime - other.RequiredTime) * 100);
        }

        public void Log()
        {
            string senarioString = "[" + RequiredTime + "]\n";
            for (int area = 0; area < exitForArea.Length; area++)
            {
                senarioString += router.areas[area].PhysicalID + " ===> " + router.exits[exitForArea[area]].PhysicalID + "\n";
            }
            senarioString += "\n";
            Debug.Log(senarioString);
        }
    }
}