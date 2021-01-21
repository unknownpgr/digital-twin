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

    public class Floor
    {
        public float Height;            // Center height of building. used for floor sort
        public GameObject gameObject;   // Gameobject of the floor
        public Bounds bounds;           // Bound of the floor

        public Floor(GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.bounds = GetBounds(gameObject);
            this.Height = bounds.center.y;
        }

        public void SetVisible(bool visibility)
        {
            gameObject.SetActive(visibility);
//            SetTransparency(gameObject.transform,visibility);
        }
    }

    private class NavMeshBaker : MonoBehaviour
    {
        void Start()
        {
            Invoke("BakeNavMesh", 0.1f);
        }

        // Bake NavMesh in runtime. Caution : It is used in Start method.
        private void BakeNavMesh()
        {
            NavMeshSurface surface = gameObject.AddComponent<NavMeshSurface>();
            NavMeshBuildSettings Setting = surface.GetBuildSettings();
            Setting.agentRadius = 0.22f;
            Setting.agentHeight = 1.7f;
            Setting.agentSlope = 45f;
            Setting.agentClimb = 1f;
            Setting.agentTypeID = surface.agentTypeID;
            surface.layerMask = 1 << LayerMask.NameToLayer("building");
            surface.BuildNavMesh(Setting);
        }

        // By using Monobehavior script, bake NavMesh after a few frames.
        public static bool BakeNavMesh(GameObject gameObject)
        {
            if (gameObject.GetComponent<NavMeshBaker>() != null) return false;
            gameObject.AddComponent<NavMeshBaker>();
            return true;
        }
    }

    public static GameObject building;
    private static int floorsCount;
    public static int FloorsCount { get => floorsCount; }
    public static List<Floor> Floors = new List<Floor>();
    public static Bounds BuildingBound;

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

        // Load building model and instantiate it.
        GameObject model = (GameObject)Resources.Load(filePath);
        if (model == null) return null;
        GameObject building = (GameObject)GameObject.Instantiate(model);
        building.name = "Building";

        DeleteCams(building);                                                       // Delete all Camera Container (Scene in Sketch Up)
        RecursiveSetLayer(UnityEngine.LayerMask.NameToLayer("building"), building); // Set layer to 'building'
        RecursiveSetCollider(building);                                             // Set collider

        // Get size of whole building
        BuildingBound = GetBounds(building);
        Vector3 size = BuildingBound.size;

        // Move building to center of plane,
        // and slightly lift to prevent plane duplication.
        size.y = -0.01f;
        building.GetComponent<Transform>().position -= size / 2;

        // Set building parent to Plane so that it renders after UI.
        building.transform.SetParent(GameObject.Find("Plane").transform);

        // Update BuildingBound because we moved building.
        BuildingBound = GetBounds(building);

        BuildingManager.building = building;

        // Set building floors
        floorsCount = building.transform.childCount;

        // Convert gameobject to floor object and append to list
        Floors = new List<Floor>();
        foreach (Transform floor in building.transform)
        {
            Floors.Add(new Floor(floor.gameObject));
        }

        // Sort by height
        Floors.Sort(delegate (Floor c1, Floor c2)
        {
            if (c1.Height > c2.Height) return 1;
            if (c1.Height < c2.Height) return -1;
            Debug.Log("Floor with same height detected.");
            return 0;
        });

        // Invoke bakeNavMesh after delay
        NavMeshBaker.BakeNavMesh(building);

        return building;
    }

    // Get floor that contains given posiiton.
    // First floor is 0, if position is out of building, return -1.
    public static int GetFloor(Vector3 position)
    {
        if (floorsCount == 1) return 0;
        for (int i = 0; i < floorsCount; i++)
        {
            if (Floors[i].bounds.Contains(position)) return i;
        }
        return -1;
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
        foreach (Transform child in obj.transform) RecursiveSetLayer(layer, child.gameObject);
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
        foreach (Transform child in obj.transform) RecursiveSetCollider(child.gameObject);
    }

    // Get Bound of give obejct
    private static Bounds GetBounds(GameObject gameObject)
    {
        Bounds bounds = new Bounds();

        // Get renderers
        MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

        // Get first enabled bound
        foreach (MeshRenderer renderer in renderers)
            if (renderer.enabled)
            {
                bounds = renderer.bounds;
                break;
            }

        // Extend bound to include another bounds
        foreach (MeshRenderer renderer in renderers)
            if (renderer.enabled)
            {
                bounds.Encapsulate(renderer.bounds);
            }
        return bounds;
    }

    // Set transparency of given object
    private static void SetTransparency(Transform obj, bool visible)
    {
        // Use DFS to recursivly set the transparency of given object and its childrens
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(obj);
        Transform current;
        while (queue.Count > 0)
        {
            current = queue.Dequeue();
            foreach (Renderer renderer in current.GetComponents<Renderer>())
            {
                foreach (Material material in renderer.materials)
                {
                    Color matColor = material.color;
                    if (visible)
                    {
                        material.SetOverrideTag("RenderType", "");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = -1;
                        matColor.a = 1;
                    }
                    else
                    {
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.EnableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        matColor.a = 0;
                    }
                    material.color = matColor;
                }
            }
            foreach (Transform child in current) queue.Enqueue(child);
        }
    }
}