using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Constants
{
    public const int NODE_SENSOR_TEMP = 0x21;
    public const int NODE_SENSOR_FIRE = 0x22;
    public const int NODE_SENSOR_SMOKE = 0x23;
    public const int NODE_SENSOR_EARTHQUAKE = 0x50;
    public const int NODE_SENSOR_FLOOD = 0x51;
    public const int NODE_EXIT = 0x52;
    public const int NODE_AREA = 0x53;
    public const int NODE_SIREN = 0x26;
    public const int NODE_DIRECTION = 0x27;
    public const int NODE_CCTV = 0x90;

    public const int PERIODIC_CHECK_TIME = 30;

    public static string IMAGE_FILENAME { get { return Application.dataPath + "/Resources/최적경로.png"; } }
    public const string IMAGE_SERVER = "https://disaster.urbanscience.uos.ac.kr";
    public const string IMAGE_KEY = "img";

    public const string AUTOSAVE_NAME_FORMAT = "AutoSave_{0}.json";

    public static string GetAutoSaveFileName(string buildingName)
    {
        buildingName = buildingName.Split('.')[0];
        return string.Format(AUTOSAVE_NAME_FORMAT, buildingName);
    }

    public static Type GetNodeTypeFromNumber(int typeNumber)
    {
        Type type;

        switch (typeNumber)
        {
            case NODE_SENSOR_TEMP:
            case NODE_SENSOR_FIRE:
            case NODE_SENSOR_SMOKE:
                type = typeof(NodeFireSensor);
                break;

            case NODE_DIRECTION:
                type = typeof(NodeDirection);
                break;

            case NODE_SENSOR_EARTHQUAKE:
                type = typeof(NodeEarthquakeSensor);
                break;

            case NODE_SENSOR_FLOOD:
                type = typeof(NodeFloodSensor);
                break;

            case NODE_EXIT:
                type = typeof(NodeExit);
                break;

            case NODE_AREA:
                type = typeof(NodeArea);
                break;

            case NODE_SIREN:
                type = typeof(NodeSound);
                break;

            case NODE_CCTV:
                type = typeof(NodeCCTV);
                break;

            default:
                return null;
        }

        return type;
    }
}
