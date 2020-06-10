using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Const
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

    public const string AUTOSAVE_NAME_FORMAT = "AutoSave_{0}.json";

    public static string GetAutoSaveFileName(string buildingName)
    {
        buildingName = buildingName.Split('.')[0];
        return String.Format(AUTOSAVE_NAME_FORMAT, buildingName);
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

            default:
                return null;
        }

        return type;
    }
}
