using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class StartButtonTransitions : MonoBehaviour
{
    [SerializeField] private FadeZoom fadeZoom;

    public YieldInstruction FadeOut()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        return fadeZoom.FadeOut(canvasGroup, transform).WaitForCompletion();
    }
    
    public YieldInstruction ResetState()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        return fadeZoom.FadeIn(canvasGroup, transform).WaitForCompletion();
    }
}
