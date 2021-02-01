using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    // 그래프는 총합을 한번만 그리거나, 아니면 여러가지로 그리게 될 수 도 있다.
    // 현재는 총합을 그리는 그래프 하나만 필요하다.
    // 그러므로, 쉽게 non-singletone으로 전환할 수 있게, 기존 클래스를 static으로 wrapping하기만 한다.

    private static GraphManager singleton;

    private readonly List<RectTransform> segment = new List<RectTransform>();
    private readonly List<RectTransform> scale = new List<RectTransform>();

    Rect size;

    // Start is called before the first frame update
    void Start()
    {
        // Graphmanager를 두 개 이상 생성할 수 있게 하는 제한은 없으므로, 완전한 싱글톤은 아니다.
        singleton = this;

        size = GetComponent<RectTransform>().rect;

        //Uncomment here to enable graph test
        //StartCoroutine(__Test());
    }

   private void AddSegment()
    {
        GameObject newObj = new GameObject();
        Image newImage = newObj.AddComponent<Image>();
        RectTransform rect = newObj.GetComponent<RectTransform>();

        // Bottom-left
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.SetParent(this.transform);
        rect.localScale = new Vector2(1, 1);
        segment.Add(rect);

        newImage.color = Color.white;

        newObj.SetActive(false);
    }

    private void AddScale()
    {
        GameObject newObj = new GameObject();
        Image newImage = newObj.AddComponent<Image>();
        RectTransform rect = newObj.GetComponent<RectTransform>();

        // Bottom-left
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.SetParent(this.transform);
        rect.localScale = new Vector2(1, 1);

        // Set size
        rect.sizeDelta = new Vector2(2, 16);

        scale.Add(rect);

        newImage.color = Color.white;

        newObj.SetActive(false);
    }

   private IEnumerator __Test()
    {
        List<Vector2> list = new List<Vector2>();
        DateTime baseTime = DateTime.Now;
        System.Random r = new System.Random();
        while (true)
        {
            list.Add(new Vector2((float)((DateTime.Now - baseTime).TotalMilliseconds), r.Next(0, 255)));
            SetGraph(list);
            yield return new WaitForSeconds(1);
        }
    }

    private void SetGraph(List<Vector2> newPoints)
    {
        while (segment.Count < newPoints.Count) AddSegment();

        Vector2 maxValue = new Vector2(-999999, -999999);
        for (int i = 0; i < newPoints.Count; i++)
        {
            Vector2 point = newPoints[i];
            maxValue.x = Mathf.Max(maxValue.x, point.x);
            maxValue.y = Mathf.Max(maxValue.y, point.y);
        }

        // If one of the maximum value is 0, then that graph is meaningless.
        if (maxValue.x == 0 || maxValue.y == 0) return;

        Vector2 scaler = new Vector2(size.width / maxValue.x, size.height / maxValue.y);

        for (int i = 0; i < segment.Count; i++)
        {
            if (i < newPoints.Count - 1)
            {
                segment[i].gameObject.SetActive(true);

                Vector2 pointA = Vector2.Scale(newPoints[i], scaler);
                Vector2 pointB = Vector2.Scale(newPoints[i + 1], scaler);

                Vector2 differenceVector = pointB - pointA;
                RectTransform rect = segment[i];

                rect.pivot = new Vector2(0, 0.5f);
                rect.sizeDelta = new Vector2(differenceVector.magnitude, 2);

                float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
                rect.localPosition = pointA;
                rect.localRotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                segment[i].gameObject.SetActive(false);
            }
        }

        // 인원수는 표시할 공간이 없으니 최댓값 표시하고,
        // 축 스케일은 10초에 하나씩 그리도록 한다.
        // 10초의 픽셀 간격은 10*1000*scaler.x이다. 입력 데이터 x값의 스케일이 milisecond이기 때문이다.
        while (scale.Count * 10000 < maxValue.x) AddScale();

        for(int i = 0; i < scale.Count; i++)
        {
            // 현재 스케일이 그래프 안에 있는 경우, 스케일을 표시한다.
            if (i * 10*1000 < maxValue.x)
            {
                Vector2 point = new Vector2(i * 10000 * scaler.x, 0);
                scale[i].gameObject.SetActive(true);
                scale[i].localPosition = point;
            }
            // 아닐 경우, 스케일을 지운다.
            else scale[i].gameObject.SetActive(false);
        }

        int maxY = (int)maxValue.y;
        GameObject.Find("label_y").GetComponent<Text>().text = "Number of people (Max=" + maxY+")";
    }

    public static void StaticSetGraph(List<Vector2> newPoints) => singleton?.SetGraph(newPoints);
}
