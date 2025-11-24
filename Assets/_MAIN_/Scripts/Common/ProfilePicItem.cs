using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePicItem : MonoBehaviour
{
    public Image image;
    public GameObject selected, locked;
    public TMP_Text xpRequiredTxt;
    public Button btn;


    public void ToggleSelect(bool action)
    {
        selected.SetActive(action);
    }

    public void Unlock()
    {
        btn.interactable = true;
        locked.SetActive(false);
    }

    public void Lock()
    {
        btn.interactable = false;
        locked.SetActive(true);
    }

    public void SetXPRequired(int xp, int reqXP)
    {
        xpRequiredTxt.text = $"XP: {xp}/{reqXP}";
    }
}
