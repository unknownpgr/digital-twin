using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextController : MonoBehaviour
{
    // public GameObject obj;
    private Text text;
    private Color color;
    public Image image;
    // Start is called before the first frame update
    void Start()
    {
        // text = obj.GetComponent<Text>();
        text = this.gameObject.GetComponent<Text>();
        Debug.Log(text.color.a);
        StartCoroutine(SetTextOpacity());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator SetTextOpacity()
    {
        image.color -= new Color(0.0f, 0.0f, 0.0f, 1.0f);
        image.color += new Color(0.0f, 0.0f, 0.0f, 1.0f);
        Debug.Log(image.color);
        yield return new WaitForSeconds(1.0f);

        float alphaValue;
        while (true)
        {
            image.color -= new Color(0.0f, 0.0f, 0.0f, 1.0f);
            text.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            yield return new WaitForSeconds(0.8f);
            image.color += new Color(0.0f, 0.0f, 0.0f, 1.0f);
            text.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
            yield return new WaitForSeconds(0.8f);

            /*
            alphaValue = text.color.a;
            if (alphaValue <= 0)
            {
                text.color += new Color(0.0f, 0.0f, 0.0f, 0.5f);
                
            }
            else if (alphaValue >= 1.0f)
            {
                text.color -= new Color(0.0f, 0.0f, 0.0f, 0.5f);
            }
            else
            {
                text.color -= new Color(0.0f, 0.0f, 0.0f, 0.5f);
            }
            Debug.Log(text.color.a);
            yield return new WaitForSeconds(1f);
            */
        }
    }
}
