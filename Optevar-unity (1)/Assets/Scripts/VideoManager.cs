using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer = null;

    public void HandleVideoPlayer()
    {
        if(videoPlayer.isPlaying == true)
        {
            videoPlayer.Stop();
        }
        else
        {
            videoPlayer.Play();
        }   
    }
}