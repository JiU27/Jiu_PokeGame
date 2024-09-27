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

    private float currentTime;
    private PokeDexManager pokeDexManager;

    void Start()
    {
        currentTime = gameTime;
        pokeDexManager = FindObjectOfType<PokeDexManager>();
        if (pokeDexManager == null)
        {
            Debug.LogError("PokeDexManager not found in the scene!");
        }

        InitializeTimer();
        InvokeRepeating("CheckAndRemoveLands", 1f, 1f); // Check every second

        // 游戏开始时的 UI 设置
        if (gameUI != null) gameUI.SetActive(true);
        if (scrollView != null) scrollView.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdateTimer();
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


    void PauseScene()
    {
        // 暂停所有非 UI 物体
        MonoBehaviour[] sceneObjects = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour obj in sceneObjects)
        {
            if (obj.gameObject.GetComponent<RectTransform>() == null) // 不是 UI 元素
            {
                obj.enabled = false;
            }
        }

        // 暂停物理引擎
        Time.timeScale = 0;
    }

    private bool isGameEnded = false;

    void EndGame()
    {
        if (isGameEnded) return; // 防止多次调用
        isGameEnded = true;

        CancelInvoke("CheckAndRemoveLands");

        // 切换 UI 显示
        if (gameUI != null) gameUI.SetActive(false);
        if (scrollView != null) scrollView.gameObject.SetActive(true);

        // 暂停场景
        PauseScene();

        // 显示结果
        StartCoroutine(DisplayResultsCoroutine());
    }

    private IEnumerator DisplayResultsCoroutine()
    {
        yield return null; // 等待一帧，确保 UI 更新

        if (pokeDexManager == null || scrollViewContent == null)
        {
            Debug.LogError("PokeDexManager or scrollViewContent is null");
            yield break;
        }

        // 清除现有内容
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var pokemon in pokeDexManager.GetDiscoveredPokemon())
        {
            if (scrollViewContent == null)
            {
                Debug.LogError("scrollViewContent became null during instantiation");
                yield break;
            }

            GameObject signedMonster = Instantiate(signedMonsterPrefab, scrollViewContent);
            SignedPokemonDisplay display = signedMonster.GetComponent<SignedPokemonDisplay>();
            if (display != null)
            {
                display.SetPokemonData(pokemon.Value);
            }
            else
            {
                Debug.LogError("SignedPokemonDisplay component not found on prefab!");
            }
        }

        // 启动协程来处理布局刷新
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
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollViewContent.transform);
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
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollViewContent.transform);
        Canvas.ForceUpdateCanvases();

        // 重置滚动位置到顶部
        ScrollRect scrollRect = scrollView != null ? scrollView.GetComponent<ScrollRect>() : null;
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