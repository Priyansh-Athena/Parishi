using UnityEngine;
using TMPro;
using System.Collections;

public class CosmicStorySetter : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text titleText;
    public TMP_Text typeText;
    public TMP_Text storyText;

    [Header("Typewriter Settings")]
    public float charDelay = 0.02f;     // delay between characters
    public float wordDelay = 0.15f;     // extra delay after a whole word

    private void OnEnable()
    {
        SetStory();
    }

    public void SetStory()
    {
        if (GameManager.Instance == null || GameManager.Instance.cosmicStory == null)
        {
            Debug.LogError("Cosmic Story not found in GameManager!");
            return;
        }

        // Set title & type instantly
        titleText.text = GameManager.Instance.cosmicStory.title;
        typeText.text = GameManager.Instance.cosmicStory.type;

        // Start animated story reveal
        StopAllCoroutines();
        StartCoroutine(TypeStoryText(GameManager.Instance.cosmicStory.story));
    }

    private IEnumerator TypeStoryText(string fullText)
    {
        storyText.text = ""; // clear previous text

        string[] words = fullText.Split(' ');

        foreach (var word in words)
        {
            // Add characters of this word one by one
            foreach (char c in word)
            {
                storyText.text += c;
                yield return new WaitForSeconds(charDelay);
            }

            // Add the space after the word
            storyText.text += " ";

            // Longer delay after each *word*
            yield return new WaitForSeconds(wordDelay);
        }
    }

    public void IncreaseXP()
    {
        GameManager.Instance.AddXP(10);
    }
}
