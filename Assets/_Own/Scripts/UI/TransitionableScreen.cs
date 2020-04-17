using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.UI;

#pragma warning disable 0649

[RequireComponent(typeof(CanvasGroup))]
public class TransitionableScreen : MonoBehaviour
{
    protected static readonly Stack<TransitionableScreen> previousScreens = new Stack<TransitionableScreen>();
    protected static TransitionableScreen currentScreen;

    [SerializeField] private bool startSelected;
    [SerializeField] private bool deactivateOnTransitionOut;

    [SerializeField] private UnityEvent onTransitionIn  = new UnityEvent();
    [SerializeField] private UnityEvent onTransitionOut = new UnityEvent();
    
    [SerializeField] private FadeZoom fadeZoom;

    [SerializeField] private CanvasGroup _canvasGroup;
    protected CanvasGroup canvasGroup => _canvasGroup = _canvasGroup ? _canvasGroup : GetComponent<CanvasGroup>();

    public bool wasSelectedLastFrame { get; private set; }
    public bool isCurrentlySelected => this == currentScreen;
    
    protected virtual void Start()
    {
        if (startSelected)
        {
            TransitionIn();
        }
        else
        {
            OnStartUnselected();
        }
    }

    protected virtual void LateUpdate()
    {
        wasSelectedLastFrame = isCurrentlySelected;
    }

    public void TransitionIn()
    {
        if (isCurrentlySelected) return;

        if (currentScreen)
        {
            Deactivate(currentScreen);
            previousScreens.Push(currentScreen);
        }

        currentScreen = this;
        Activate(this);
    }

    public void TransitionOut()
    {
        if (!isCurrentlySelected) return;
        
        Deactivate(this);
        previousScreens.Push(this);
        currentScreen = null;
    }

    public void TransitionToPreviousScreen() => TransitionToPrevious();

    public static void TransitionToPrevious()
    {
        if (previousScreens.Count == 0) return;

        if (currentScreen != null)
        {
            Deactivate(currentScreen);
        }

        currentScreen = previousScreens.Pop();
        Activate(currentScreen);
    }
    
    private static void Activate(TransitionableScreen screen)
    {
        screen.gameObject.SetActive(true);

        var canvasGroup = screen.canvasGroup;
        if (canvasGroup)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        screen.OnTransitionIn();
    }

    private static void Deactivate(TransitionableScreen screen)
    {
        var canvasGroup = screen.canvasGroup;
        if (canvasGroup)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (screen.deactivateOnTransitionOut)
        {
            screen.gameObject.SetActive(false);
        }

        screen.OnTransitionOut();
    }

    protected virtual void OnStartUnselected() {}

    protected virtual void OnTransitionIn()
    {
        fadeZoom.FadeIn(canvasGroup, canvasGroup.transform);
        onTransitionIn.Invoke();
    }

    protected virtual void OnTransitionOut()
    {
        fadeZoom.FadeOut(canvasGroup, canvasGroup.transform);
        onTransitionOut.Invoke();
    }

    protected void SelectFirstButton()
    {
        var button = GetComponentInChildren<Button>();
        if (button) button.Select();
    }
}