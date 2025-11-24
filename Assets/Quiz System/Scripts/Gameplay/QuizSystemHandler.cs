using QuizSystem.SO;
using UnityEngine;
using UnityEngine.SceneManagement;
using QuizSystem.Events;
using QuizSystem.Custom;
using QuizSystem.UI.Base;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QuizSystem.Gameplay
{
    public enum QuizLayout
    {
        Layout_01
    }
    public enum QuizLoadMode
    {
        Instantiate,
        Include,
        NewScene
    }

    public class QuizSystemHandler : MonoBehaviour
    {
        [Tooltip("Instantiate will spawn a new Quiz prefab, Include will include the Quiz prefab into your scene, NewScene will open a Quiz Scene")]
        [SerializeField] private QuizLoadMode _quizLoadMode;

        [Space]
        [ConditionalProperty("_quizLoadMode", (int)QuizLoadMode.NewScene)]
        [SerializeField] private LoadSceneMode _sceneLoadMode;

        // ------------------------------
        // Build-safe Scene Reference
        // ------------------------------
#if UNITY_EDITOR
        [SerializeField] private SceneAsset _quizScene;
#endif

        [SerializeField] private string _quizSceneName;   // Used in builds

        [Space]
        [SerializeField] private QuizSystemManager _quizPrefab;
        [SerializeField] private QuizResultUI _quizResultPrefab;

        [Space]
        [SerializeField] private QuizDataRef _quizDataRef;

        [Space]
        [SerializeField] private QuizSystemManager _currentQuizManager;
        [SerializeField] private QuizResultUI _currentQuizResultPanel;

        [Space]
        [SerializeField] private SharableBool _openQuizAutomatically;

        // ------------------------------
        // Sync SceneAsset → String when edited
        // ------------------------------
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_quizScene != null)
            {
                _quizSceneName = _quizScene.name;
            }
        }
#endif

        public QuizLoadMode LoadMode
        {
            get { return _quizLoadMode; }
            set
            {
                _quizLoadMode = value;
                HandleLoadModeChange();
            }
        }

        public void HandleLoadModeChange()
        {
            if (_quizLoadMode == QuizLoadMode.Include)
            {
                if (_currentQuizManager == null && _quizPrefab != null)
                {
                    _currentQuizManager = Instantiate(_quizPrefab);
                    _openQuizAutomatically.value = false;
                }

                if (_currentQuizResultPanel == null && _quizResultPrefab != null)
                {
                    _currentQuizResultPanel = Instantiate(_quizResultPrefab);
                }
            }
            else
            {
                if (_currentQuizManager != null)
                {
                    DestroyImmediate(_currentQuizManager.gameObject);
                    _currentQuizManager = null;
                }
                if (_currentQuizResultPanel != null)
                {
                    DestroyImmediate(_currentQuizResultPanel.gameObject);
                    _currentQuizResultPanel = null;
                }
            }
        }

        private void OnEnable()
        {
            QuizEvents.OnOpenQuiz += HandleQuizOpening;
            QuizEvents.OnCloseQuiz += HandleOnCloseQuiz;
        }

        private void OnDisable()
        {
            QuizEvents.OnOpenQuiz -= HandleQuizOpening;
            QuizEvents.OnCloseQuiz -= HandleOnCloseQuiz;
        }

        private void HandleQuizOpening(QuizData quizData)
        {
            _quizDataRef.chosenQuiz = quizData;
            OpenQuiz();
        }

        private void OpenQuiz()
        {
            switch (_quizLoadMode)
            {
                case QuizLoadMode.Instantiate:
                    if (_quizPrefab != null)
                    {
                        _openQuizAutomatically.value = false;
                        _currentQuizManager = Instantiate(_quizPrefab);
                        _currentQuizManager.Open();
                    }
                    if (_quizResultPrefab != null)
                    {
                        _currentQuizResultPanel = Instantiate(_quizResultPrefab);
                    }
                    break;

                case QuizLoadMode.Include:
                    if (_currentQuizManager != null)
                    {
                        _currentQuizManager.Open();
                    }
                    break;

                case QuizLoadMode.NewScene:
                    if (!string.IsNullOrEmpty(_quizSceneName))
                    {
                        _openQuizAutomatically.value = true;
                        SceneManager.LoadScene(_quizSceneName, _sceneLoadMode);
                    }
                    break;
            }
        }

        private void HandleOnCloseQuiz()
        {
            if (_quizLoadMode == QuizLoadMode.NewScene && _sceneLoadMode == LoadSceneMode.Additive)
            {
                if (!string.IsNullOrEmpty(_quizSceneName))
                {
                    SceneManager.UnloadSceneAsync(_quizSceneName);
                }
            }
        }
    }
}
