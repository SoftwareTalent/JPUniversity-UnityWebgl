using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Video;

public class MistakeIntervalIndicatorGroup : MonoBehaviour
{
    [SerializeField] private float showDuration = 0.25f;
    [SerializeField] private MistakeIntervalIndicator indicatorPrefab;
    
    public IEnumerator Show()
    {
        var rt = GetComponent<RectTransform>();
        yield return rt.DOSizeDelta(new Vector2(rt.sizeDelta.x, 0.0f), showDuration)
            .From()
            .SetEase(Ease.OutExpo)
            .WaitForCompletion();
    }

    public IEnumerator Add(VideoPlayer videoPlayer, RangeFloat timeInterval, Color color)
    {
        MistakeIntervalIndicator intervalIndicator = Instantiate(indicatorPrefab, transform);
        yield return intervalIndicator.Show(videoPlayer, timeInterval, color);
    }
}
