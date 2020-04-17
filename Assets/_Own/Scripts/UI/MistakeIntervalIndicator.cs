using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Video;

public class MistakeIntervalIndicator : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float delayAfterShowDuration = 0.25f;

    public IEnumerator Show(VideoPlayer player, RangeFloat interval, Color color)
    {
        Assert.IsNotNull(image);
        image.color = color;

        float NormalizedTime(float time) => (float)(time / player.length);

        var rt = GetComponent<RectTransform>();
        float anchorMinX = NormalizedTime(interval.min);
        rt.anchorMin = new Vector2(anchorMinX, 0.0f);
        rt.anchorMax = new Vector2(anchorMinX, 1.0f);
        
        while ((float)player.time <= interval.max)
        {
            rt.anchorMax = new Vector2(NormalizedTime(interval.Clamp((float)player.time)), 1.0f);
            yield return null;
        }
    }
}
