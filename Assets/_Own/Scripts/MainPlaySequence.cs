using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Video;

public struct MistakeButtonClick
{
    public ulong mistakeId;
    public float timestamp;
    public bool wasCorrect;
    public int hitIntervalIndex;
    public MistakeMark mistakeMark;
}

[System.Serializable]
public struct GameConfig
{
    public string videoPath;
    public MistakeButtonSettings[] buttons;
}

[System.Serializable]
public struct MistakeButtonSettings
{
    public ulong mistakeId;
    public string name;
    public string iconPath;
    public Color color;
    public RangeFloat[] intervals;

    public MistakeButtonSettings(ulong mistakeId, string name, string iconPath, Color color, RangeFloat[] intervals)
    {
        this.mistakeId = mistakeId;
        this.name = name;
        this.iconPath = iconPath;
        this.color = color;
        this.intervals = intervals;
    }
}

public class MainPlaySequence : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private StartButtonTransitions startButtonTransitions;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private float mistakeTimingLeeway = 1.0f;
    [SerializeField] private bool isTutorial = false;

    [Header("Mistake marks")]
    [SerializeField] private MistakeMark mistakeMarkPrefab;
    [SerializeField] private Transform playbackIndicatorFillAreaTransform;
    
    [Header("Mistake interval indicators")]
    [SerializeField] private MistakeIntervalIndicatorGroup mistakeIntervalIndicatorGroupPrefab;
    [SerializeField] private Transform videoBottomGroupTransform;
    
    [Header("After video")]
    [SerializeField] private float delayBeforeTryAgainButtonShows = 1.0f;
    [SerializeField] private ScoreCounter scoreCounter;
    [SerializeField] private float minPassFraction = 0.9f;
    [SerializeField] private BottomBlockTransitions tryAgainButtonTransitions;
    [SerializeField] private BottomBlockTransitions tutorialButtonTransitions;
    [SerializeField] private Popup youPassedPopup;
    
    private readonly Dictionary<ulong, MistakeButtonSettings> mistakeButtonSettings = new Dictionary<ulong, MistakeButtonSettings>();
    private readonly Dictionary<ulong, MistakeIntervalIndicatorGroup> mistakeIntervalIndicatorGroups = new Dictionary<ulong, MistakeIntervalIndicatorGroup>();
    private readonly List<MistakeButtonClick> mistakeButtonClicks = new List<MistakeButtonClick>();

    public void Play()
    {
        Assert.IsNotNull(startButtonTransitions);
        Assert.IsNotNull(videoPlayer);
        Assert.IsNotNull(mistakeMarkPrefab);
        Assert.IsNotNull(mistakeIntervalIndicatorGroupPrefab);
        Assert.IsNotNull(playbackIndicatorFillAreaTransform);
        Assert.IsNotNull(scoreCounter);
        Assert.IsNotNull(tryAgainButtonTransitions);
        Assert.IsNotNull(youPassedPopup);

        StopAllCoroutines();
        StartCoroutine(PlaySequence());
    }
    
    public void ResetState()
    {
        StopAllCoroutines();
        
        UnsubscribeFromMistakeButtonClicks();

        foreach (var click in mistakeButtonClicks)
        {
            Destroy(click.mistakeMark.gameObject);
        }
        mistakeButtonClicks.Clear();

        foreach (var kvp in mistakeIntervalIndicatorGroups)
        {
            Destroy(kvp.Value.gameObject);
        }
        mistakeIntervalIndicatorGroups.Clear();
        
        startButtonTransitions.ResetState();
        scoreCounter.Hide();
        scoreCounter.SetCount(0, 0);
        tryAgainButtonTransitions.Hide();
        if (tutorialButtonTransitions)
        {
            tutorialButtonTransitions.Hide();
        }
        videoPlayer.Stop();
    }

    private IEnumerator PlaySequence()
    {
        yield return startButtonTransitions.FadeOut();
        
        SubscribeToMistakeButtonClicks();
        yield return PlayVideo();
        UnsubscribeFromMistakeButtonClicks();

        yield return scoreCounter.Show();

        int numIntervals = mistakeButtonSettings.Sum(kvp => kvp.Value.intervals.Length);
        int numCorrectIntervals = 0;
        scoreCounter.SetCount(0, numIntervals);
        for (int i = 0; i < mistakeButtonClicks.Count; ++i)
        {
            if (mistakeButtonClicks[i].wasCorrect)
            {
                numCorrectIntervals += 1;
                scoreCounter.SetCount(numCorrectIntervals, numIntervals);
                yield return mistakeButtonClicks[i].mistakeMark.Highlight();
            }
        }

        yield return new WaitForSeconds(delayBeforeTryAgainButtonShows);

        bool didPass = numIntervals <= 0 || (float)numCorrectIntervals / numIntervals >= minPassFraction;
        if (didPass)
        {
            yield return youPassedPopup.Show();
        }
        else
        {
            YieldInstruction tryAgainButtonShow = tryAgainButtonTransitions.Show();
            if (!isTutorial && tutorialButtonTransitions)
            { 
                yield return tutorialButtonTransitions.Show();
            }
            yield return tryAgainButtonShow;
        }
    }

    private IEnumerator PlayVideo()
    {
        bool isVideoFinished = false;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
        void OnVideoFinished(VideoPlayer sender)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            isVideoFinished = true;
        }

        if (isTutorial)
        {
            yield return new WaitUntil(() => videoPlayer.isPlaying);
            var pairs = mistakeButtonSettings
                .SelectMany(kvp => kvp.Value.intervals
                    .Select(interval => (id: kvp.Key, color: kvp.Value.color, interval: IntervalForChecking(interval))))
                .OrderBy(tuple => tuple.interval.min)
                .ToQueue();

            while (!isVideoFinished && pairs.Count > 0 && pairs.Peek().interval.min <= videoPlayer.length)
            {
                (ulong mistakeId, Color color, RangeFloat interval) = pairs.Dequeue();

                yield return new WaitUntil(() => videoPlayer.time >= interval.min);

                var group = GetIntervalIndicatorGroup(mistakeId);
                StartCoroutine(group.Add(videoPlayer, interval, color));
            }
        }

        yield return new WaitUntil(() => isVideoFinished);
    }

    private void SubscribeToMistakeButtonClicks()
    {
        foreach (var buttonUI in GetComponentsInChildren<MistakeButtonUI>())
        {
            if (!mistakeButtonSettings.ContainsKey(buttonUI.settings.mistakeId))
            {
                mistakeButtonSettings.Add(buttonUI.settings.mistakeId, buttonUI.settings);
            }
            
            buttonUI.onClicked += OnMistakeButtonClicked;
        }
    }

    private void UnsubscribeFromMistakeButtonClicks()
    {
        foreach (var buttonUI in GetComponentsInChildren<MistakeButtonUI>())
        {
            buttonUI.onClicked -= OnMistakeButtonClicked;
        }
    }
    
    private void OnMistakeButtonClicked(MistakeButtonUI sender, MistakeButtonSettings settings)
    {
        float timestamp = (float)videoPlayer.time;
        bool wasClickCorrect = WasClickCorrect(settings.mistakeId, timestamp, out int hitIntervalIndex);
        
        // If any clicks already covered this interval, don't create a new one, just highlight the old one.
        if (wasClickCorrect)
        {
            int clickIndex = mistakeButtonClicks.FindIndex(c =>
                c.wasCorrect &&
                c.mistakeId == settings.mistakeId &&
                c.hitIntervalIndex == hitIntervalIndex
            );
            if (clickIndex != -1)
            {
                mistakeButtonClicks[clickIndex].mistakeMark.Highlight();
                return;
            }
        }
        
        MistakeMark mark = Instantiate(mistakeMarkPrefab, playbackIndicatorFillAreaTransform);
        mark.SetColor(settings.color);
        
        float normalizedTime = (float)(timestamp / videoPlayer.length);
        RectTransform rectTransform = mark.rectTransform;
        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(normalizedTime, 1.0f);

        var click = new MistakeButtonClick
        {
            mistakeId = settings.mistakeId,
            timestamp = timestamp,
            wasCorrect = wasClickCorrect,
            hitIntervalIndex = hitIntervalIndex,
            mistakeMark = mark
        };
        mistakeButtonClicks.Add(click);
        mark.RevealCorrectness(click.wasCorrect);
    }
    
    private bool WasClickCorrect(ulong mistakeId, float timestamp, out int hitIntervalIndex)
    {
        hitIntervalIndex = -1;
        
        if (!mistakeButtonSettings.TryGetValue(mistakeId, out MistakeButtonSettings settings))
        {
            return false;
        }

        for (int i = 0; i < settings.intervals.Length; ++i)
        {
            if (IntervalForChecking(settings.intervals[i]).Contains(timestamp))
            {
                hitIntervalIndex = i;
                return true;
            }
        }

        return false;
    }
    
    private RangeFloat IntervalForChecking(RangeFloat interval)
    {
        RangeFloat newInterval = interval.Inflated(mistakeTimingLeeway);
        newInterval.max = Mathf.Floor(newInterval.max) + 1.0f;
        return newInterval;
    }
    
    private MistakeIntervalIndicatorGroup GetIntervalIndicatorGroup(ulong mistakeId)
    {
        if (!mistakeIntervalIndicatorGroups.TryGetValue(mistakeId, out var group))
        { 
            group = Instantiate(mistakeIntervalIndicatorGroupPrefab, videoBottomGroupTransform);
            mistakeIntervalIndicatorGroups.Add(mistakeId, group);
            StartCoroutine(group.Show());
        }

        return group;
    }
}
