using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphRenderer : MonoBehaviour
{
    GameObject graph;
    RectTransform graphTransform;
    List<GameObject> points = new List<GameObject>();

    void Start()
    {
        graph = GameObject.Find("Graph");
        graphTransform = graph.GetComponent<RectTransform>();
    }

    float T = 0;
    float dt = 1.0f;
    float timeout = 0;
    void Update()
    {
        // Update time
        T += Time.deltaTime;
        if (timeout > 0) timeout -= Time.deltaTime;
        else
        {
            List<Vector2> sineWave = new List<Vector2>();
            for (float t = 0; t < 7; t += 0.5f)
            {
                sineWave.Add(new Vector2(T + t, Mathf.Sin(T + t)));
            }
            SetGraph(sineWave);
            timeout = dt;
        }
    }

    void SetGraph(List<Vector2> dots)
    {
        // Remove existing lines
        foreach (GameObject point in points) Destroy(point);

        // Calculate scale factor
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (Vector2 dot in dots)
        {
            min.x = Mathf.Min(min.x, dot.x);
            min.y = Mathf.Min(min.y, dot.y);
            max.x = Mathf.Max(max.x, dot.x);
            max.y = Mathf.Max(max.y, dot.y);
        }

        Vector2 delta = max - min;
        delta.x = 1.0f / delta.x;
        delta.y = 1.0f / delta.y;

        for (int i = 0; i < dots.Count - 1; i++)
        {
            GameObject line = GetLine(graph);
            points.Add(line);
            Vector2 start = Vector2.Scale(Vector2.Scale(dots[i] - min, delta), graphTransform.sizeDelta);
            Vector2 end = Vector2.Scale(Vector2.Scale(dots[i + 1] - min, delta), graphTransform.sizeDelta);
            SetLinePosition(line, start, end);
        }
    }

    private static GameObject GetLine(GameObject parent)
    {
        GameObject image = new GameObject();
        image.AddComponent<CanvasRenderer>();
        RectTransform rectTransform = image.AddComponent<RectTransform>();
        Image mImage = image.AddComponent<Image>();
        mImage.color = new Color32(0, 0, 0, 128);
        rectTransform.SetParent(parent.GetComponent<RectTransform>());
        rectTransform.localScale = new Vector3(1, 1, 1);
        rectTransform.anchoredPosition = new Vector2(500, 500);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0, 0.5f);
        return image;
    }

    private static void SetLinePosition(GameObject line, Vector3 start, Vector3 end, float lineWidth = 2.0f)
    {
        RectTransform rectTransform = line.GetComponent<RectTransform>();
        Vector3 differenceVector = end - start;
        float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
        rectTransform.sizeDelta = new Vector2(differenceVector.magnitude, lineWidth);
        rectTransform.localPosition = Vector2.zero;
        rectTransform.anchoredPosition = start;
        rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
