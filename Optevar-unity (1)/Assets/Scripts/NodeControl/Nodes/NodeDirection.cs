using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class NodeDirection : NodeManager
{
    protected override string prefabName { get => "DirectionSensor"; }

    public override string DisplayName { get => "방향지시등: " + PhysicalID; }

    // 화살표 오브젝트, default direction(=up)=z-axis direction.
    private GameObject directionObject;

    // up,down,left,right for enable direction, else disabled.
    private string direction;
    [JsonIgnore]
    public string Direction
    {
        get => direction;
        set
        {
            direction = value;
            directionObject.SetActive(true);
            switch (value)
            {
                case "up":
                    directionObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case "down":
                    directionObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case "left":
                    directionObject.transform.rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case "right":
                    directionObject.transform.rotation = Quaternion.Euler(0, +90, 0);
                    break;
                default:
                    direction = "up";
                    directionObject.SetActive(false);
                    break;
            }
        }
    }

    protected override void Init()
    {
        directionObject = gameObject.transform.GetChild(0).gameObject;
        directionObject.SetActive(false);
    }
}
