using QuizSystem.Events;
using QuizSystem.SO;
using UnityEngine;

public class QuizManager : MonoBehaviour
{   
    public QuizResultData resultData;

    private void OnEnable()
    {
        QuizEvents.OnQuizIsFinished += OnQuizIsFinished;
    }
    private void OnDisable()
    {
        QuizEvents.OnQuizIsFinished -= OnQuizIsFinished;
    }

    private void OnQuizIsFinished()
    {
       GameManager.Instance.AddXP((int)resultData.correctAnswersPercentage);
    }
}
