using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(OnLoadFinished());
    }

    IEnumerator OnLoadFinished()
    {
        // Wait for 5 frame
        for (int i = 0; i < 5; i++) yield return new WaitForEndOfFrame();

        CanvasGroup cg = GetComponent<CanvasGroup>();
        float opacity = 1.0f;
        while (opacity > 0)
        {
            cg.alpha = opacity;
            opacity -= 0.05f;
            yield return new WaitForSeconds(0.01f);
        }

        gameObject.SetActive(false);
    }
}
