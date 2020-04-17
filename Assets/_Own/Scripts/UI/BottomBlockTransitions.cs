using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class BottomBlockTransitions : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private LayoutElement layoutElement;

    [SerializeField] private float preferredWidth = 200.0f;
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float hideDuration = 0.25f;

    public YieldInstruction Show()
    {
        Assert.IsNotNull(canvasGroup);
        Assert.IsNotNull(layoutElement);

        layoutElement.DOKill();
        var size = layoutElement
            .DOPreferredSize(new Vector2(preferredWidth, layoutElement.preferredHeight), showDuration)
            .SetEase(Ease.InOutSine);
        
        canvasGroup.DOKill();
        var fade = canvasGroup
            .DOFade(1.0f, showDuration)
            .SetEase(Ease.InOutSine);
        
        return DOTween.Sequence()
            .Append(size)
            .AppendCallback(() => SetCanvasGroupEnabled(true))
            .Append(fade)
            .WaitForCompletion();
    }

    public YieldInstruction Hide()
    {
        Assert.IsNotNull(canvasGroup);
        Assert.IsNotNull(layoutElement);
        
        canvasGroup.DOKill();
        var fade = canvasGroup
            .DOFade(0.0f, hideDuration)
            .SetEase(Ease.InOutSine);

        layoutElement.DOKill();
        var size = layoutElement
            .DOPreferredSize(new Vector2(0.0f, layoutElement.preferredHeight), hideDuration)
            .SetEase(Ease.InOutSine);

        return DOTween.Sequence()
            .Append(fade)
            .AppendCallback(() => SetCanvasGroupEnabled(false))
            .Append(size)
            .WaitForCompletion();
    }

    private void SetCanvasGroupEnabled(bool groupEnabled)
    {
        canvasGroup.interactable = groupEnabled;
        canvasGroup.blocksRaycasts = groupEnabled;
    }
}
