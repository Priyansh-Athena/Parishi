using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class NewsDataSetter : MonoBehaviour
{
    public TMP_Text titleTxt,descriptionTxt ,authorTxt, publishedAtTxt, pageNumber;
    public Image img;
    public Button readMoreButton;

    private int idx = 0;

    private void Start()
    {
        SetData(GameManager.Instance.astroNews.articles[idx]);
    }

    public void Next()
    {
        idx = (idx + 1) % 10;
        SetData(GameManager.Instance.astroNews.articles[idx]);
    }

    public void Previous()
    {
        idx = (idx == 0) ? 9 : idx - 1;
        SetData(GameManager.Instance.astroNews.articles[idx]);
    }


    public void SetData(Article article)
    {
        pageNumber.text = $"{idx + 1}/10";

        titleTxt.text = article.title;
        descriptionTxt.text = article.description;
        authorTxt.text = article.author;
        img.sprite = article.imageSprite;
        img.preserveAspect = true;
        publishedAtTxt.text = article.publishedAt;

        readMoreButton.onClick.RemoveAllListeners();
        readMoreButton.onClick.AddListener(() => Application.OpenURL(article.url));
    }

    public void IncreaseXP()
    {
        GameManager.Instance.AddXP(25);
    }
}
