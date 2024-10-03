using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class GameControl : MonoBehaviour
{
    public float gameTime = 300f; // 5 minutes by default, editable in inspector
    public Slider timerSlider;
    public GameObject signedMonsterPrefab;
    public ScrollRect scrollView;
    public Transform scrollViewContent;
    public GameObject gameUI; // 新增：对 GameUI 的引用
    public GameObject resultsUI;

    private float currentTime;
    private PokeDexManager pokeDexManager;
    private bool isGamePaused = false;

    void Start()
    {
        currentTime = gameTime;
        pokeDexManager = FindObjectOfType<PokeDexManager>();
        if (pokeDexManager == null)
        {
            Debug.LogError("PokeDexManager not found in the scene!");
        }

        InitializeTimer();
        InvokeRepeating("CheckAndRemoveLands", 1f, 1f);

        gameUI.SetActive(true);
        resultsUI.SetActive(false);
    }

    void Update()
    {
        if (!isGamePaused)
        {
            UpdateTimer();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

    }

    void TogglePause()
    {
        isGamePaused = !isGamePaused;
        if (isGamePaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        gameUI.SetActive(false);
        resultsUI.SetActive(true);
        DisplayResults();
    }

    void ResumeGame()
    {
        Time.timeScale = 1;
        gameUI.SetActive(true);
        resultsUI.SetActive(false);
        ClearResults();
    }

    void InitializeTimer()
    {
        if (timerSlider != null)
        {
            timerSlider.maxValue = gameTime;
            timerSlider.value = gameTime;
        }
    }

    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        if (timerSlider != null)
        {
            timerSlider.value = currentTime;
        }

        if (currentTime <= 0)
        {
            EndGame();
        }
    }


    void CheckAndRemoveLands()
    {
        GameObject[] lands = GameObject.FindGameObjectsWithTag("Land");
        if (lands.Length > 3)
        {
            GameObject farthestLand = lands
                .OrderByDescending(land => Vector3.Distance(land.transform.position, GameObject.FindGameObjectWithTag("Player").transform.position))
                .First();
            Destroy(farthestLand);
            Debug.Log("Removed farthest land: " + farthestLand.name);
        }
    }


    private bool isGameEnded = false;

    void EndGame()
    {
        if (isGameEnded) return;
        isGameEnded = true;

        CancelInvoke("CheckAndRemoveLands");

        gameUI.SetActive(false);
        resultsUI.SetActive(true);

        PauseScene();
        DisplayResults();
    }

    void PauseScene()
    {
        MonoBehaviour[] sceneObjects = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour obj in sceneObjects)
        {
            if (obj.gameObject.GetComponent<RectTransform>() == null)
            {
                obj.enabled = false;
            }
        }
        Time.timeScale = 0;
    }

    void DisplayResults()
    {
        StartCoroutine(DisplayResultsCoroutine());
    }

    private IEnumerator DisplayResultsCoroutine()
    {
        yield return null;

        if (pokeDexManager == null || scrollViewContent == null)
        {
            Debug.LogError("PokeDexManager or scrollViewContent is null");
            yield break;
        }

        ClearResults();

        var discoveredPokemon = pokeDexManager.GetDiscoveredPokemon();
        Debug.Log($"Number of discovered Pokemon: {discoveredPokemon.Count}");

        foreach (var pokemon in discoveredPokemon)
        {
            GameObject signedMonster = Instantiate(signedMonsterPrefab, scrollViewContent);
            SignedPokemonDisplay display = signedMonster.GetComponent<SignedPokemonDisplay>();
            if (display != null)
            {
                display.SetPokemonData(pokemon.Value);
                Debug.Log($"Displayed Pokemon: {pokemon.Value.name.english}");
            }
            else
            {
                Debug.LogError("SignedPokemonDisplay component not found on prefab!");
            }

            // 确保新实例化的对象不会重叠
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollViewContent);
            yield return null; // 等待一帧，让布局更新
        }

        // 最后再次刷新布局
        StartCoroutine(RefreshLayoutCoroutine());
    }

    private IEnumerator RefreshLayoutCoroutine()
    {
        yield return null; // 等待下一帧

        if (scrollViewContent == null)
        {
            Debug.LogError("scrollViewContent is null in RefreshLayoutCoroutine");
            yield break;
        }

        // 强制刷新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollViewContent);
        Canvas.ForceUpdateCanvases();

        // 重新计算 Content 的高度
        float totalHeight = 0;
        foreach (RectTransform child in scrollViewContent)
        {
            totalHeight += child.rect.height;
        }

        // 设置 Content 的高度
        RectTransform contentRectTransform = scrollViewContent.GetComponent<RectTransform>();
        if (contentRectTransform != null)
        {
            contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalHeight);
        }
        else
        {
            Debug.LogError("contentRectTransform is null");
            yield break;
        }

        // 再次强制刷新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollViewContent);
        Canvas.ForceUpdateCanvases();

        // 重置滚动位置到顶部
        ScrollRect scrollRect = scrollView.GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }
        else
        {
            Debug.LogError("ScrollRect component not found");
        }

        Debug.Log($"Layout refreshed. Content height: {totalHeight}");
    }

    private void ClearResults()
    {
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddTime(float amount)
    {
        currentTime = Mathf.Min(currentTime + amount, gameTime);
        if (timerSlider != null)
        {
            timerSlider.value = currentTime;
        }
    }


    System.Collections.IEnumerator LoadImageFromURL(string url, Image image)
    {
        UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((UnityEngine.Networking.DownloadHandlerTexture)request.downloadHandler).texture;
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}