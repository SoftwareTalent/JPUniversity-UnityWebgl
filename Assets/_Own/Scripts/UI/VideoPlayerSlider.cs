using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerSlider : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    [SerializeField] private Slider _slider;
    public Slider slider => _slider = _slider ? _slider : GetComponentInChildren<Slider>();

    void Update()
    {
        slider.value = videoPlayer && videoPlayer.length > 0.0 ? 
            (float)(videoPlayer.time / videoPlayer.length) :
            0.0f;
    }
}
