using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideGUI : MonoBehaviour
{
    public GameObject BuildingObject;
    List<GameObject> FloorObjects;
    public List<float> FloorHeights;
    public List<Bounds> FloorBounds;
    GameObject Created;
    int floors;
    // Start is called before the first frame update
    void Start()
    {
        InitBuildingInfo();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HideFloor(int idx)
    {
        if (!(idx > 0 & idx <= floors)) return;
        Debug.Log(idx);
        Debug.Log(FloorHeights[idx - 1]);
        for (int i = 0; i < floors; i++)
        
            FloorObjects[i].SetActive(true);
        
        for (int i = floors; i > idx; i--)
            FloorObjects[i-1].gameObject.SetActive(false);

        for (int i = 0; i < Created.transform.childCount; i++)
        {
            Bounds b = FloorBounds[idx - 1];
            if (b.Contains(Created.transform.GetChild(i).transform.position)) {
                Created.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                Created.transform.GetChild(i).gameObject.SetActive(false);
            }

            /*
            if ((Created.transform.GetChild(i).transform.position.y < FloorObjects[idx - 1].transform.position.y + .83f)
                & (Created.transform.GetChild(i).transform.position.y > FloorObjects[idx - 1].transform.position.y - .83f))
            {
                Created.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                Created.transform.GetChild(i).gameObject.SetActive(false);
            }
            if (idx == HeightToFloor(Created.transform.GetChild(i).transform.position.y))
                Created.transform.GetChild(i).gameObject.SetActive(true);
                */
        }
        
    }
    public void InitBuildingInfo()//빌딩load한 후에 이 함수 실행되도록 바꿔야함
    {

        BuildingObject = GameObject.Find("Building");
        if (BuildingObject != null)
        {
            this.floors = BuildingObject.transform.childCount;
            FloorObjects = new List<GameObject>();
            FloorHeights = new List<float>();
            for (int i = 0; i < BuildingObject.transform.childCount; i++)
                FloorObjects.Add(BuildingObject.transform.GetChild(i).gameObject);
            FloorObjects.Sort(delegate (GameObject x, GameObject y)
            {
                if (x.transform.position.y > y.transform.position.y) return 1;
                else if (x.transform.position.y < y.transform.position.y) return -1;
                return 0;
            });
            for (int i = 0; i < FloorObjects.Count; i++)
            {
                FloorHeights.Add(FloorObjects[i].transform.position.y);
                FloorBounds.Add(FloorObjects[i].GetComponent<BuildingObjectManager>().GetBounds());
            }
        }
        
        if (Created == null)
            Created = GameObject.Find("all_objects");
        SetGUI();
    }
    public void InitBuildingInfo(GameObject _building)
    {
        BuildingObject = _building;
        if (BuildingObject != null)
        {
            this.floors = BuildingObject.transform.childCount;
            FloorObjects = new List<GameObject>();
            FloorHeights = new List<float>();

            for (int i = 0; i < BuildingObject.transform.childCount; i++)
                FloorObjects.Add(BuildingObject.transform.GetChild(i).gameObject);
            FloorObjects.Sort(delegate (GameObject x, GameObject y)
            {
                if (x.transform.position.y > y.transform.position.y) return 1;
                else if (x.transform.position.y < y.transform.position.y) return -1;
                return 0;
            });
            for (int i = 0; i < FloorObjects.Count; i++)
                FloorHeights.Add(FloorObjects[i].transform.position.y);
        }

        if (Created == null)
            Created = GameObject.Find("all_objects");
        SetGUI();
    }

    void SetGUI()
    {
        if (this.gameObject != null)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
                gameObject.transform.GetChild(i).gameObject.SetActive(true);
            for (int i = 0; i < gameObject.transform.childCount - floors; i++)
            {
                gameObject.transform.GetChild(i).gameObject.SetActive(false);
            }
            
        }
    }

    public int HeightToFloor(Vector3 _pos)
    {
        for (int i = 0; i < floors; i++)
            if (FloorBounds[i].Contains(_pos))
                return i + 1;

        for (int i = 0; i < floors; i++)
            if (Mathf.Abs(FloorHeights[i] - _pos.y) < 0.5f)
                return i + 1;
        int ret = -1;
        float diff = 100000f;
        for (int i = 0; i < floors; i++)
            if (diff > Mathf.Abs(FloorHeights[i] - _pos.y))
            {
                ret = i;
                diff = Mathf.Abs(FloorHeights[i] - _pos.y);
            }
        return ret + 1;
    }
}
