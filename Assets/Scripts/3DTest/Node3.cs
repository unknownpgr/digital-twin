using UnityEngine;
using UnityEditor;

public class Node3
{
    public Vector3 position;
    public int x, y, z;
    public int weight = 0;
    public Node3()
    {
        
    }
    public Node3(Vector3 _pos)
    {
        position = _pos;
    }
    public Node3(Vector3 _pos, int x, int y, int z)
    {
        position = _pos;
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public float GetRealDistance(Node3 _b)
    {
        return Vector3.Distance(this.position, _b.position);
    }
}