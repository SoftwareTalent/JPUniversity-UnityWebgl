using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

// Sets the correct URL on the video player
public class GameVideo : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string path;
    
    void Start()
    {
        videoPlayer = videoPlayer ? videoPlayer : GetComponentInChildren<VideoPlayer>();
        if (!videoPlayer)
        {
            return;
        }
        
        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, path);
    }
}
