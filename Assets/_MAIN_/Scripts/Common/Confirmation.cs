using DG.Tweening;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class Confirmation : MonoBehaviour
{
    [Header("Data")]
    public string instruction = "Are you sure you want to close the game?";
    [Range(0f, 1f)] public float duration = 0.5f;

    [Header("References")]
    public TMP_Text instructionTxt;
    public CanvasGroup cg;

    TweenCallback hideCallback;

    private void Start()
    {
        instructionTxt.text = instruction;
    }

    public void Show()
    {
        cg.blocksRaycasts = true;
        UIUtility.Fade(cg, 1f, duration);
    }

    public void Hide()
    {
        hideCallback += OnHideAnimationDone;
        UIUtility.Fade(cg, 0f, duration, hideCallback);
    }

    public void OnHideAnimationDone()
    {
        cg.blocksRaycasts = false;
    }
}
