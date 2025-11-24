using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ProfileIconData
{
    public Sprite sprite;
    public int reqXP;
    public bool isLocked;
}

public class MenuManager : MonoBehaviour
{
    public List<ProfileIconData> profileIconsData;
    public GameObject profilePicItemPrefab;
    public Transform profilePicItemParent;
    public TMP_Text mainUsername, totalXPText, mainTotalXPTxt;
    public Image mainProfilePic;
    public TMP_InputField usernameInput;
    public CanvasGroup userProfileCG;

    public Image musicIcon;
    public Sprite muteSpite, nonMuteSprite;

    private List<ProfilePicItem> profilePicItems = new List<ProfilePicItem>();
    private Coroutine saveUsernameRoutine;

    private void Start()
    {
        SetupUserProfile();
    }

    public void ToggleMusic()
    {
        if(GameManager.Instance.ToggleMusic())
        {
            musicIcon.sprite = nonMuteSprite;
        }
        else
        {
            musicIcon.sprite = muteSpite;
        }
    }

    public void SetupUserProfile()
    {
        mainUsername.text = GameManager.Instance.playerData.username;
        mainProfilePic.sprite = profileIconsData[GameManager.Instance.playerData.profilePicIdx].sprite;
        usernameInput.SetTextWithoutNotify(GameManager.Instance.playerData.username);
        totalXPText.text = $"XP: {GameManager.Instance.playerData.xp}";
        mainTotalXPTxt.text = $"XP: {GameManager.Instance.playerData.xp}";

        for (int i = 0; i < profileIconsData.Count; i++)
        {
            ProfileIconData profileIconData = profileIconsData[i];
            GameObject profilePicObj = Instantiate(profilePicItemPrefab, profilePicItemParent);
            ProfilePicItem item = profilePicObj.GetComponent<ProfilePicItem>();
            profilePicItems.Add(item);

            item.image.sprite = profileIconData.sprite;

            if (profileIconData.isLocked)
            {
                if (GameManager.Instance.playerData.xp >= profileIconData.reqXP)
                {
                    item.Unlock();
                }
                else
                {
                    item.Lock();
                    item.SetXPRequired(GameManager.Instance.playerData.xp, profileIconData.reqXP);
                }
            }
            else
            {
                item.Unlock();
            }

                // ✅ Create a local copy of i
                int index = i;

            item.btn.onClick.AddListener(() => {
                // Deselect all first
                foreach (var p in profilePicItems)
                    p.ToggleSelect(false);

                // Then select this one
                item.ToggleSelect(true);
                mainProfilePic.sprite = profileIconsData[index].sprite;
                GameManager.Instance.SaveProfilePicIndex(index);
            });
        }

        profilePicItems[GameManager.Instance.playerData.profilePicIdx].ToggleSelect(true);
    }

    public void CheckIfAnyUnlocked()
    {
        for(int i=0; i<profileIconsData.Count; i++)
        {
            ProfileIconData profileIconData = profileIconsData[i];
            ProfilePicItem item = profilePicItems[i];
            if (GameManager.Instance.playerData.xp >= profileIconData.reqXP)
            {
                item.Unlock();
            }
            else
            {
                item.Lock();
                item.SetXPRequired(GameManager.Instance.playerData.xp, profileIconData.reqXP);
            }
        }
    }


    public void OnUsernameChange(string str)
    {
        mainUsername.text = str;

        if (saveUsernameRoutine != null)
            StopCoroutine(saveUsernameRoutine);

        saveUsernameRoutine = StartCoroutine(SaveUsernameDelay());
    }

    private IEnumerator SaveUsernameDelay()
    {
        yield return new WaitForSeconds(1f); // wait 1 s after last keystroke
        GameManager.Instance.SaveUsername(usernameInput.text);
        saveUsernameRoutine = null;
    }

    public void FadeUserProfile(bool action)
    {
        userProfileCG.blocksRaycasts = action;
        UIUtility.Fade(userProfileCG, action ? 1: 0, 1f);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
