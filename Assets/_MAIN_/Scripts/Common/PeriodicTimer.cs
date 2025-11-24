using UnityEngine;
using UnityEngine.Events;

public class PeriodicTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("The time interval (in seconds) between each event trigger.")]
    public float timeInterval = 5f;

    private float timer = 0f;

    [Header("Event to Execute")]
    [Tooltip("The action(s) to run when the time interval is reached.")]
    public UnityEvent OnIntervalReached;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= timeInterval)
        {
            OnIntervalReached.Invoke();

            timer -= timeInterval;
        }
    }

    public void LogTimerEvent()
    {
        Debug.Log($"Periodic event triggered on '{gameObject.name}'! Interval: {timeInterval} seconds.");
    }
}