using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Popup : MonoBehaviour
{
    [SerializeField] private CanvasGroup overlayCanvasGroup;
    [SerializeField] private RectTransform popupContainerTransform;
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float hideDuration = 0.5f;

    public YieldInstruction Show()
    {
        overlayCanvasGroup.interactable = true;
        overlayCanvasGroup.blocksRaycasts = true;
        var overlayFade = overlayCanvasGroup
            .DOFade(1.0f, showDuration)
            .SetEase(Ease.OutExpo);

        Vector2 anchoredPos = popupContainerTransform.anchoredPosition;
        anchoredPos.y = 0.0f;
        popupContainerTransform.anchoredPosition = anchoredPos;
        var move = popupContainerTransform
            .DOAnchorPosY(popupContainerTransform.rect.height, showDuration)
            .From()
            .SetEase(Ease.OutBack);
        
        return DOTween.Sequence()
            .Join(overlayFade)
            .Join(move)
            .WaitForCompletion();
    }
    
    public YieldInstruction Hide()
    {
        overlayCanvasGroup.interactable = false;
        overlayCanvasGroup.blocksRaycasts = false;
        var overlayFade = overlayCanvasGroup
            .DOFade(0.0f, hideDuration)
            .SetEase(Ease.InExpo);
        
        var move = popupContainerTransform
            .DOAnchorPosY(popupContainerTransform.rect.height, hideDuration)
            .SetEase(Ease.InBack);

        return DOTween.Sequence()
            .Join(overlayFade)
            .Join(move)
            .WaitForCompletion();
    }

    // Just Hide() doesn't get exposed to UnityEvents.
    public void HideNoReturnValue() => Hide();
}
