using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerData
{
    public string username = "Anonymous";
    public int xp = 0;
    public int profilePicIdx;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public MythologyStory cosmicStory;
    public NewsResponse astroNews;
    [SerializeField] AstrologyNewsManager astrologyNewsManager;
    [SerializeField] CosmicStoriesDatabase cosmicStoriesDatabase;
    [SerializeField] QuizDataLoader quizDataLoader;
    [SerializeField] LoadingScreen loading;
    [SerializeField] CanvasGroup notificationCG;
    [SerializeField] TMP_Text notificationTxt;
    [SerializeField] AudioSource bgAudioSource;

    private string USERNAME_KEY = "Username", XP_KEY = "XP", PROFILE_PIC_INDEX_KEY = "ProfilePicIdx", STORY_DATA_KEY = "CurrentMythologyStory";

    public PlayerData playerData = new PlayerData();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }


        string username = PlayerPrefs.GetString(USERNAME_KEY);

        if (username == null || username == "")
            playerData.username = "Anonymous";
        else
            playerData.username = username;


        playerData.xp = PlayerPrefs.GetInt(XP_KEY);
        playerData.profilePicIdx = PlayerPrefs.GetInt(PROFILE_PIC_INDEX_KEY);
    }

    public bool ToggleMusic()
    {
        if(bgAudioSource.volume == 0)
        {
            bgAudioSource.volume = 0.5f;
            return true;
        }
        else
        {
            bgAudioSource.volume = 0f;
            return false;
        }
    }

    public void SaveUsername(string username)
    {
        playerData.username = username;
        notificationTxt.text = "Username updated to " + username;
        UIUtility.Fade(notificationCG, 1f, 0.5f, () =>
        {
            StartCoroutine(HideNotificationAfterDelay(2f));
        });
        PlayerPrefs.SetString(USERNAME_KEY, username);
        PlayerPrefs.Save();
    }

    public void AddXP(int deltaXP)
    {
        playerData.xp += deltaXP;
        notificationTxt.text = "Gained " + deltaXP + " XP!";
        UIUtility.Fade(notificationCG, 1f, 0.5f, () =>
        {
            StartCoroutine(HideNotificationAfterDelay(2f));
        });
        PlayerPrefs.SetInt(XP_KEY, playerData.xp);
        PlayerPrefs.Save();
    }

    public void SaveProfilePicIndex(int idx)
    {
        playerData.profilePicIdx = idx;
        notificationTxt.text = "Profile picture updated!";
        UIUtility.Fade(notificationCG, 1f, 0.5f, () =>
        {
            StartCoroutine(HideNotificationAfterDelay(2f));
        });
        PlayerPrefs.SetInt(PROFILE_PIC_INDEX_KEY, idx);
        PlayerPrefs.Save();
    }

    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UIUtility.Fade(notificationCG, 0f, 0.5f);
    }

    private void Start()
    {
        LoadNewDayData();
        StartCoroutine(LoadNews());
    }

    public void LoadNewDayData()
    {
        NewCosmicStory();
        NewQuiz();
    }

    public IEnumerator LoadNews()
    {
        loading.ShowLoadingScreen();

        yield return StartCoroutine(astrologyNewsManager.FetchNews());

        loading.HideLoadingScreen();
    }

    private void NewCosmicStory()
    {
        cosmicStory = cosmicStoriesDatabase.GetRandomStory();
    }

    private void NewQuiz()
    {
        quizDataLoader.LoadRandomFive();
    }
}
