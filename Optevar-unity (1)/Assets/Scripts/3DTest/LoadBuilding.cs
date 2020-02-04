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

public class LoadBuilding : MonoBehaviour
{
    string path = "Models/";
    GameObject building;
    public GameObject LoadSkp(string _s)
    {
        if (building != null)
        {
            building.AddComponent<NavMeshSurface>().RemoveData();
            Destroy(building);
        }


        string fullPath = path + _s;
        string ext = "skp";
        Debug.Log("Loading");
        if (File.Exists(Application.dataPath + "/Resources/" + fullPath + "." + ext + ".meta"))
        {
            String value = File.ReadAllText(Application.dataPath + "/Resources/" + fullPath + "." + ext + ".meta", Encoding.UTF8);
            value = value.Replace("isReadable: 0", "isReadable: 1");
            value = value.Replace("importCameras: 1", "importCameras: 0");
            File.WriteAllText(Application.dataPath + "/Resources/" + fullPath + "." + ext + ".meta", value, Encoding.UTF8);
        }
        else
        {
            Resources.Load(fullPath);
            if (File.Exists(Application.dataPath + "/Resources/" + fullPath + "." + ext + ".meta"))
            {
                String value = File.ReadAllText(Application.dataPath + "/Resources/" + fullPath + "." + ext + ".meta", Encoding.UTF8);
                value = value.Replace("isReadable: 0", "isReadable: 1");
                value = value.Replace("importCameras: 1", "importCameras: 0");
                File.WriteAllText(Application.dataPath + "/Resources/" + fullPath + "." + ext + ".meta", value, Encoding.UTF8);
            }
        }
        GameObject obj = Instantiate(Resources.Load(fullPath)) as GameObject;
        obj.name = "Building";
        Debug.Log(obj.ToString());
        // Delete all Camera Container (Scene in Sketch Up)
        DeleteCams(obj);

        // Set Bounds
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            BuildingObjectManager tmp = obj.transform.GetChild(i).gameObject.AddComponent<BuildingObjectManager>();
            tmp.GetBounds();
        }


        // Set layer to 'building'
        RecursiveSetLayer(UnityEngine.LayerMask.NameToLayer("building"), obj);
        RecursiveSetCollider(obj);
        building = obj;
        return obj;

    }

    void DeleteCams(GameObject _obj)
    {
        Component[] comps = _obj.transform.GetComponentsInChildren<Camera>();
        for (int i = 0; i < comps.Length; i++)
        {
            Destroy(comps[i].gameObject);
        }
    }

    void RecursiveSetLayer(int _layer, GameObject _obj)
    {
        _obj.layer = _layer;
        for (int i = 0; i < _obj.transform.childCount; i++)
        {
            RecursiveSetLayer(_layer, _obj.transform.GetChild(i).gameObject);
        }
    }
    void RecursiveSetCollider(GameObject _obj)
    {
        if (!SetCollider(_obj))
            for (int i = 0; i < _obj.transform.childCount; i++)
            {
                RecursiveSetCollider(_obj.transform.GetChild(i).gameObject);
            }
    }
    bool SetCollider(GameObject _obj)
    {
        bool ret = false;
        for (int i = 0; i < _obj.transform.childCount; i++)
        {
            if (_obj.transform.GetChild(i).GetComponent<MeshFilter>() != null)
            {
                MeshCollider tmp = _obj.transform.GetChild(i).gameObject.AddComponent<MeshCollider>();
                tmp.sharedMesh = _obj.transform.GetChild(i).GetComponent<MeshFilter>().mesh;
                tmp.convex = false;
                ret = true;
            }
        }
        return ret;
    }

    public void SetNavMesh()
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
}

