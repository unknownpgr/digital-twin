using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    // Video player of video window
    public VideoPlayer videoPlayer = null;
    // Continaer for video
    private VideoClip videoClip = null;

    private void Start()
    {
    }

    public void PlayVideoClip()
    {
        videoPlayer.Play();
    }

    public void SetVideoClipWithDisasterState(bool isDisaster)
    { 
        if (isDisaster == true)
        {
            videoClip = Resources.Load<VideoClip>("Video/Laptop explodes in Letchworth office");
            videoPlayer.clip = videoClip;
        }
        else if (isDisaster == false)
        {
            videoClip = Resources.Load<VideoClip>("Video/sample1");
            videoPlayer.clip = videoClip;
        }

    }
}