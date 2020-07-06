using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    private const int POINT_NUM = 30;

    readonly List<RectTransform> points = new List<RectTransform>();
    Rect size;

    // Start is called before the first frame update
    void Start()
    {
        size = GetComponent<RectTransform>().rect;
        Color color = new Color(Random.Range(0, 255) / 255.0f, Random.Range(0, 255) / 255.0f, Random.Range(0, 255) / 255.0f);
        for (int i = 0; i < POINT_NUM; i++)
        {
            GameObject NewObj = new GameObject();
            Image NewImage = NewObj.AddComponent<Image>();
            RectTransform rect = NewObj.GetComponent<RectTransform>();

            // Random color
            NewImage.color = color;

            // Bottom-left
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.SetParent(this.transform);
            rect.localScale = new Vector2(1, 1);
            points.Add(rect);

            NewObj.SetActive(false);
        }
    }

    public void SetGraph(List<Vector2> newPoints)
    {
        if (points.Count < POINT_NUM) return;

        Vector2 maxValue = new Vector2(-999999, -999999);

        int end = Mathf.Min(newPoints.Count - 1, 29);
        for (int i = 0; i < end + 1; i++)
        {
            Vector2 point = newPoints[i];
            maxValue.x = Mathf.Max(maxValue.x, point.x);
            maxValue.y = Mathf.Max(maxValue.y, point.y);
        }

        // If one of the maximum value is 0, then that graph is meaningless.
        if (maxValue.x == 0 || maxValue.y == 0) return;

        Vector2 scaler = new Vector2(size.width / maxValue.x, size.height / maxValue.y);

        for (int i = 0; i < end; i++)
        {
            points[i].gameObject.SetActive(true);

            Vector2 pointA = Vector2.Scale(newPoints[i], scaler);
            Vector2 pointB = Vector2.Scale(newPoints[i + 1], scaler);

            Vector2 differenceVector = pointB - pointA;
            RectTransform rect = points[i];

            rect.pivot = new Vector2(0, 0.5f);
            rect.sizeDelta = new Vector2(differenceVector.magnitude, 2);

            float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
            rect.localPosition = pointA;
            rect.localRotation = Quaternion.Euler(0, 0, angle);
        }
        for (int i = end; i < POINT_NUM; i++)
        {
            points[i].gameObject.SetActive(false);
        }
    }
}
