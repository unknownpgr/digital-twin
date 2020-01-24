using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class VelocityCoef
{
    public float a1, a2, a3, a4, f, m;
    
    override public string ToString()
    {
        return "Velocity coefs: "
            + a1 + ", "
            + a2 + ", "
            + a3 + ", "
            + a4 + ", "
            + f + ", "
            + m;
    }
}

[SerializeField]
public class js<T>
{
    public T[] items;
}
public class WeightCalc
{
    JsonParser jsParser = new JsonParser();
    public float referenceVelocity = 1.0f; // male, 35 years old.
    VelocityCoef coefJson;
    Dictionary<string, List<float>> coef;
    
    public void init()
    {
        coef = new Dictionary<string, List<float>>();
        coefJson = jsParser.Load<VelocityCoef>("coefs");
        Debug.Log(coefJson.ToString());
        List<float> tmp = new List<float>();
        tmp.Add(coefJson.a1);
        tmp.Add(coefJson.a2);
        tmp.Add(coefJson.a3);
        tmp.Add(coefJson.a4);
        tmp.Add(coefJson.m);
        tmp.Add(coefJson.f);
        coef.Add("Velocity", tmp);
    }

    public float GetVelocity(int _age, int _sex)
    {
        // 0, 1, 2, 3 ... age
        // 0, 1 ... sex
        return coef["Velocity"][_age] * coef["Velocity"][_sex + 4] * referenceVelocity;
    }

    public float GetVelocity(Node _node)
    {
        return coef["Velocity"][_node.kindAge] * coef["Velocity"][_node.kindSex + 4] * referenceVelocity;
    }

    public float GetNodeVelocity(Node _node, float _d)
    {
        return GetVelocity(_node) / _d;
    }
}
