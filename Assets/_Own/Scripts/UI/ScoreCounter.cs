using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScoreCounter : MonoBehaviour
{
    private TMP_Text _text;
    private TMP_Text text => _text = _text ? _text : GetComponentInChildren<TMP_Text>();

    private BottomBlockTransitions _transitions;
    private BottomBlockTransitions transitions => _transitions = _transitions ? _transitions : GetComponentInChildren<BottomBlockTransitions>();

    [SerializeField] private float punchAmount = 0.5f;
    [SerializeField] private float punchDuration = 0.25f;

    public YieldInstruction Show()
    {
        return transitions ? _transitions.Show() : null;
    }

    public YieldInstruction Hide()
    {
        return transitions ? _transitions.Hide() : null;
    }
    
    public void SetCount(int numCorrectIntervals, int numTotalIntervals)
    {
        if (!text)
        {
            return;
        }

        _text.text = $"{numCorrectIntervals}/{numTotalIntervals}";

        Transform tf = _text.transform;
        tf.DOKill(complete: true);
        tf.DOPunchScale(new Vector3(punchAmount, punchAmount, punchAmount), punchDuration);
    }
}
