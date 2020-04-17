using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class MistakeMark : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;

    [Header("PopIn")]
    [SerializeField] private float popInDuration = 0.25f;
    [SerializeField] private float popInMaxScale = 2.0f;

    [Header("Reveal")]
    [SerializeField] private Image correctImage;
    [SerializeField] private Image incorrectImage;
    [SerializeField] private float revealDuration = 0.25f;
    [SerializeField] private float revealMaxScale = 4.0f;

    [Header("Highlight")] 
    [SerializeField] private float highlightPunchScale = 0.5f;
    [SerializeField] private float highlightDuration = 0.25f;

    private RectTransform _rectTransform;
    public RectTransform rectTransform => 
        _rectTransform = _rectTransform ? _rectTransform : GetComponent<RectTransform>();
    
    void Start()
    {
        GetComponent<CanvasGroup>()
            .DOFade(0.0f, popInDuration)
            .From()
            .SetEase(Ease.OutExpo)
            .SetUpdate(isIndependentUpdate: true);

        Transform tf = transform;
        tf.DOKill(complete: true);
        tf.localScale = Vector3.one;
        tf
            .DOScale(popInMaxScale, popInDuration)
            .From()
            .SetEase(Ease.OutExpo)
            .SetUpdate(isIndependentUpdate: true);
    }
    
    public YieldInstruction RevealCorrectness(bool wasCorrect)
    {
        Image imageToReveal = wasCorrect ? correctImage : incorrectImage;

        imageToReveal.enabled = true;
        imageToReveal.DOKill(complete: false);
        Color color = imageToReveal.color;
        color.a = 0.0f;
        imageToReveal.color = color;
        var fade = imageToReveal
            .DOFade(1.0f, revealDuration)
            .SetEase(Ease.OutExpo)
            .SetUpdate(isIndependentUpdate: true);

        Transform tf = imageToReveal.transform;
        tf.DOKill(complete: true);
        tf.localScale = Vector3.one;
        var zoom = tf
            .DOScale(revealMaxScale, revealDuration)
            .From()
            .SetEase(Ease.OutExpo)
            .SetUpdate(isIndependentUpdate: true);

        return DOTween
            .Sequence()
            .Join(fade)
            .Join(zoom)
            .SetUpdate(isIndependentUpdate: true)
            .WaitForCompletion();
    }
    
    public YieldInstruction Highlight()
    {
        transform.DOKill(complete: true);
        return transform
            .DOPunchScale(new Vector3(highlightPunchScale, highlightPunchScale, highlightPunchScale), highlightDuration)
            .WaitForCompletion();
    }

    public void SetColor(Color color)
    {
        if (backgroundImage)
        {
            backgroundImage.color = color;
        }
    }
}
