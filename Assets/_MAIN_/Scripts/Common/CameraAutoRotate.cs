using UnityEngine;
using DG.Tweening;

public class CameraAutoRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationAmount = 180f;   // How much to rotate per cycle
    public float tweenDuration = 3f;      // Duration of each rotation segment
    public Ease easeType = Ease.Linear;

    private Tweener rotationTween;

    void Start()
    {
        StartRandomRotation();
    }

    void StartRandomRotation()
    {
        RotateToRandomDirection();
    }

    void RotateToRandomDirection()
    {
        // Pick a new random normalized rotation direction
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        // Convert direction to rotation target
        Vector3 targetRotation = transform.eulerAngles + randomDirection * rotationAmount;

        rotationTween = transform
            .DORotate(targetRotation, tweenDuration, RotateMode.FastBeyond360)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                // After finishing rotation, pick a new random direction
                RotateToRandomDirection();
            });
    }

    void OnDestroy()
    {
        if (rotationTween != null)
            rotationTween.Kill();
    }
}
