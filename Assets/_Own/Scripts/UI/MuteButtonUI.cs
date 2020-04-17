using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Video;

public class MuteButtonUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Pop")]
    [SerializeField] private float punch = 1.0f;
    [SerializeField] private float punchDuration = 0.25f;
    [SerializeField] private int vibrato = 10;
    [SerializeField] private float elasticity = 1.0f;

    private bool isMuted = false;
    
    void Start()
    {
        Assert.IsNotNull(image);
        Assert.IsNotNull(soundOnSprite);
        Assert.IsNotNull(soundOffSprite);
        Assert.IsNotNull(videoPlayer);
        
        isMuted = Enumerable.Range(0, videoPlayer.audioTrackCount)
            .Any(i => videoPlayer.GetDirectAudioMute((ushort)i));
        SetMuted(isMuted);
    }

    public void Toggle()
    {
        SetMuted(!isMuted);
    }

    private void SetMuted(bool newIsMuted)
    {
        videoPlayer.SetDirectAudioMute(0, newIsMuted);
        for (ushort i = 1; i < videoPlayer.audioTrackCount; ++i)
        {
            videoPlayer.SetDirectAudioMute(i, newIsMuted);
        }

        image.sprite = newIsMuted ? soundOffSprite : soundOnSprite;

        if (newIsMuted == isMuted)
        {
            return;
        }

        isMuted = newIsMuted;
        transform.DOPunchScale(
            new Vector3(punch, punch, punch),
            punchDuration,
            vibrato,
            elasticity);
    }
}
