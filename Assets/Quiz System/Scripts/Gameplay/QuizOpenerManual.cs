using QuizSystem.Events;
using QuizSystem.SO;
using UnityEngine;

namespace QuizSystem.Gameplay
{
    public class QuizOpenerManual : MonoBehaviour
    {
        [SerializeField] private QuizData _quizData;

        private void Start()
        {
            Open();
        }

        public void Open()
        {
            QuizEvents.OnOpenQuiz?.Invoke(_quizData);
        }
    } 
}
