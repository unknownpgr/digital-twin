using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public static class BuildingManager
{
    // There is always only one building, therefore use static.
    // Building is therefore singletone.

    private class Floor
    {
        public float Height;
        public GameObject gameObject;
        public Floor(GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.Height = GetBounds(gameObject).center.y;
        }
    }

    public static GameObject building;
    private static int floorsCount;
    public static int FloorsCount { get => floorsCount; }
    public static List<GameObject> Floors = new List<GameObject>();

    public static GameObject LoadSkp(string fileName)
    {
        if (fileName == null) return null;
        fileName = fileName.ToLower().Replace(".skp", "");

        // Remove existing bulding
        if (BuildingManager.building != null)
        {
            // BuildingManager.building.GetComponent<NavMeshSurface>().RemoveData();
            GameObject.Destroy(BuildingManager.building);
            BuildingManager.building = null;
        }

        // Meta file of given skp file
        string filePath = "Models/" + fileName;
        string metaFilePath = Application.dataPath + "/Resources/" + filePath + ".skp.meta";

        if (!File.Exists(metaFilePath)) return null;
        string value = File.ReadAllText(metaFilePath, Encoding.UTF8);

        // Change value of meta file
        value = value.Replace("isReadable: 0", "isReadable: 1")
                        .Replace("importCameras: 1", "importCameras: 0");

        // Rewrite meta file
        File.WriteAllText(metaFilePath, value, Encoding.UTF8);

        // Load building model
        GameObject model = (GameObject)Resources.Load(filePath);
        if (model == null) return null;
        GameObject building = (GameObject)GameObject.Instantiate(model);
        building.name = "Building";

        // Delete all Camera Container (Scene in Sketch Up)
        DeleteCams(building);

        // Set layer to 'building'
        RecursiveSetLayer(UnityEngine.LayerMask.NameToLayer("building"), building);
        RecursiveSetCollider(building);

        // Get size of whole building
        Bounds bounds = GetBounds(building);
        Vector3 size = bounds.size;

        // Move building to center of plane,
        // and slightly lift to prevent plane duplication.
        size.y = -0.01f;
        building.GetComponent<Transform>().position -= size / 2;

        BuildingManager.building = building;

        // Set building floors
        floorsCount = building.transform.childCount;

        // If building has multiple floors
        if (floorsCount > 1)
        {
            // Sort floor by height

            // Convert gameobject to floor object and append to list
            List<Floor> floors = new List<Floor>();
            foreach (Transform floor in building.transform)
            {
                floors.Add(new Floor(floor.gameObject));
            }

            // Sort by height
            floors.Sort(delegate (Floor c1, Floor c2)
            {
                if (c1.Height > c2.Height) return 1;
                if (c1.Height < c2.Height) return -1;
                throw new Exception("Same height floor");
            });

            // Add to Floors
            foreach (Floor floor in floors)
            {
                // Debug.Log("H = " + floor.Height);
                Floors.Add(floor.gameObject);
            }
        }

        return building;
    }

    // Remove all cameras of given object
    private static void DeleteCams(GameObject obj)
    {
        Component[] comps = obj.transform.GetComponentsInChildren<Camera>();
        foreach (Component comp in comps) GameObject.Destroy(comp.gameObject);
    }

    // Recursively set layer of given object
    private static void RecursiveSetLayer(int layer, GameObject obj)
    {
        obj.layer = layer;
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            RecursiveSetLayer(layer, obj.transform.GetChild(i).gameObject);
        }
    }

    // Recursively set collider on gameobject
    private static void RecursiveSetCollider(GameObject obj)
    {
        if (obj.GetComponent<MeshFilter>() != null)
        {
            MeshCollider tmp = obj.AddComponent<MeshCollider>();
            tmp.sharedMesh = obj.GetComponent<MeshFilter>().mesh;
            tmp.convex = false;
            // tmp.static = true;
        }

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            RecursiveSetCollider(obj.transform.GetChild(i).gameObject);
        }
    }

    // Set NavMesh. used for routing
    public static void SetNavMesh()
    {
        // Bake NavMesh in runtime
        if (building == null) return;

        NavMeshSurface surface = building.AddComponent<NavMeshSurface>();
        NavMeshBuildSettings Setting = surface.GetBuildSettings();
        Setting.agentRadius = 0.22f;
        Setting.agentHeight = 1.7f;
        Setting.agentSlope = 45f;
        Setting.agentClimb = 1f;
        Setting.agentTypeID = surface.agentTypeID;
        surface.layerMask = 1 << LayerMask.NameToLayer("building");
        surface.BuildNavMesh(Setting);
    }

    // Get Bound of give obejct
    private static Bounds GetBounds(GameObject gameObject)
    {
        Bounds bounds = new Bounds();

        // Get renderers
        MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length > 0)
        {
            int i = 0;

            // Get a bound
            for (; i < renderers.Length; i++)
            {
                if (renderers[i].enabled)
                {
                    bounds = renderers[i].bounds;
                    break;
                }
            }

            // Extend bound to include another bounds
            for (; i < renderers.Length; i++)
            {
                if (renderers[i].enabled)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }
        }
        return bounds;
    }
}

