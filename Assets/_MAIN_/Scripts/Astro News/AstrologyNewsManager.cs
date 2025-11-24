using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Article
{
    public string author;
    public string title;
    public string description;
    public string url;
    public string publishedAt;
    public string urlToImage;

    [HideInInspector]
    public Sprite imageSprite; // ✅ Sprite to store downloaded image
}

[System.Serializable]
public class NewsResponse
{
    public string status;
    public int totalResults;
    public List<Article> articles;
}

public class AstrologyNewsManager : MonoBehaviour
{
    [Header("API Settings")]
    [Tooltip("Get your free API key from https://newsapi.org/")]
    public string apiKey = "c5f154fbb8d64ae0b8a8d458cc9f3e4b";
    public string query = "astrology";
    public int totalFetchCount = 50;
    public int showCount = 10;

    public void Fetch() // ✅ Assigned to button
    {
        StartCoroutine(FetchNews());
    }

    public IEnumerator FetchNews()
    {
        int seed = System.DateTime.Now.DayOfYear;
        Random.InitState(seed);

        string url = $"https://newsapi.org/v2/everything?q={UnityWebRequest.EscapeURL(query)}&language=en&pageSize={totalFetchCount}&sortBy=publishedAt&apiKey={apiKey}";
        Debug.Log("Fetching news from: " + url);

        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching news: " + uwr.error);
                yield break;
            }

            string json = uwr.downloadHandler.text;
            json = json.Replace("\"articles\":[]", "\"articles\":{}");

            NewsResponse response = JsonUtility.FromJson<NewsResponse>(json);

            if (response != null && response.articles != null && response.articles.Count > 0)
            {
                var selected = response.articles.OrderBy(x => Random.value).Take(showCount).ToList();
                Debug.Log($"✅ News Fetched Successfully! ({selected.Count} articles)");

                // Download images and assign them as Sprites
                yield return StartCoroutine(DownloadImages(selected));

                GameManager.Instance.astroNews = new NewsResponse
                {
                    status = response.status,
                    totalResults = response.totalResults,
                    articles = selected
                };
            }
            else
            {
                Debug.LogWarning("No astrology news found.");
            }
        }
    }

    private IEnumerator DownloadImages(List<Article> articles)
    {
        foreach (var article in articles)
        {
            if (!string.IsNullOrEmpty(article.urlToImage))
            {
                using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(article.urlToImage))
                {
                    yield return uwr.SendWebRequest();

                    if (uwr.result == UnityWebRequest.Result.Success)
                    {
                        Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                        article.imageSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to load image for article: {article.title} ({uwr.error})");
                    }
                }
            }
            else
            {
                Debug.Log($"No image URL for: {article.title}");
            }
        }

        Debug.Log("✅ All article images processed.");
    }
}
