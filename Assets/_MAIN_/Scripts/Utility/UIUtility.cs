using UnityEngine;
using DG.Tweening;

public static class UIUtility
{
    /// <summary>
    /// Fades a CanvasGroup to the target alpha over a given duration using DOTween.
    /// </summary>
    /// <param name="canvasGroup">The CanvasGroup to fade.</param>
    /// <param name="targetAlpha">The target alpha (0 = transparent, 1 = opaque).</param>
    /// <param name="duration">How long the fade should take (in seconds).</param>
    /// <param name="onComplete">Optional callback when the fade completes.</param>
    public static void Fade(CanvasGroup canvasGroup, float targetAlpha, float duration, TweenCallback onComplete = null)
    {
        if (canvasGroup == null)
        {
            Debug.LogWarning("UIUtility: CanvasGroup reference is null.");
            return;
        }

        // Kill any existing tween on this CanvasGroup to prevent overlap
        canvasGroup.DOKill();

        // Start fading
        Tween fadeTween = canvasGroup.DOFade(targetAlpha, duration);

        if (onComplete != null)
            fadeTween.OnComplete(onComplete);
    }
}
