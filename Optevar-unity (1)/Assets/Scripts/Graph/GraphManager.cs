using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{   
    private static List<GraphManager> list = new List<GraphManager>();  

public static void SetGraph(int graphIndex,List<Vector2> values){
    if(list.Count>graphIndex){
        list[graphIndex].SetGraph(values);
    }
}

    List<RectTransform> points = new List<RectTransform>();
    Rect size;

    // Start is called before the first frame update
    void Start()
    {
        list.Add(this);
        size= this.GetComponent<RectTransform>().rect;
        Color color = new Color(Random.Range(0,255)/255.0f,Random.Range(0,255)/255.0f,Random.Range(0,255)/255.0f);
        for(int i=0;i<30;i++){
            GameObject NewObj = new GameObject();
            Image NewImage = NewObj.AddComponent<Image>();
            RectTransform rect = NewObj.GetComponent<RectTransform>();

            // Random color
            NewImage.color = color;

            // Bottom-left
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.SetParent(this.transform);
            rect.localScale = new Vector2(1,1);
            points.Add(rect);

            NewObj.SetActive(false);
        }
        StartCoroutine(Test());
    }

    IEnumerator Test()
    {
        float t = Random.Range(0,5);
        while(true){
            List<Vector2> list = new List<Vector2>();
            for(int i =0;i<t*2+2;i++){
                list.Add(new Vector2(i,Mathf.Cos((i+t)/5.0f)*50+50));
            }
            SetGraph(list);
            t+=.5f;
            yield return new WaitForSeconds(1);
        }
    }

    public void SetGraph(List<Vector2> newPoints){
        Vector2 maxValue = new Vector2(-999999,-999999);

        int end = (int)Mathf.Min(newPoints.Count-1,29);
        for(int i =0;i<end+1;i++){
            Vector2 point = newPoints[i];
            maxValue.x = Mathf.Max(maxValue.x,point.x);
            maxValue.y = Mathf.Max(maxValue.y,point.y);
        }

        Vector2 scaler = new Vector2(size.width/maxValue.x,size.height/maxValue.y);

        for(int i =0;i<end;i++){
points[i].gameObject.SetActive(true);

            Vector2 pointA = Vector2.Scale(newPoints[i],scaler);
            Vector2 pointB = Vector2.Scale(newPoints[i+1],scaler);

            Vector2 differenceVector = pointB - pointA;
 RectTransform rect = points[i];

            rect.pivot = new Vector2(0, 0.5f);
            rect.sizeDelta = new Vector2(differenceVector.magnitude, 2);
            
            float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
            rect.localPosition = pointA;
            rect.localRotation = Quaternion.Euler(0, 0, angle);
        }
        for(int i =end;i<30;i++){
points[i].gameObject.SetActive(false);
        }
    }
}
