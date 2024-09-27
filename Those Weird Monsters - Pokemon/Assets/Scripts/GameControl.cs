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
    public GameObject gameUI; // �������� GameUI ������

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

        // ��Ϸ��ʼʱ�� UI ����
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
        // ��ͣ���з� UI ����
        MonoBehaviour[] sceneObjects = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour obj in sceneObjects)
        {
            if (obj.gameObject.GetComponent<RectTransform>() == null) // ���� UI Ԫ��
            {
                obj.enabled = false;
            }
        }

        // ��ͣ��������
        Time.timeScale = 0;
    }

    private bool isGameEnded = false;

    void EndGame()
    {
        if (isGameEnded) return; // ��ֹ��ε���
        isGameEnded = true;

        CancelInvoke("CheckAndRemoveLands");

        // �л� UI ��ʾ
        if (gameUI != null) gameUI.SetActive(false);
        if (scrollView != null) scrollView.gameObject.SetActive(true);

        // ��ͣ����
        PauseScene();

        // ��ʾ���
        StartCoroutine(DisplayResultsCoroutine());
    }

    private IEnumerator DisplayResultsCoroutine()
    {
        yield return null; // �ȴ�һ֡��ȷ�� UI ����

        if (pokeDexManager == null || scrollViewContent == null)
        {
            Debug.LogError("PokeDexManager or scrollViewContent is null");
            yield break;
        }

        // �����������
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

        // ����Э����������ˢ��
        StartCoroutine(RefreshLayoutCoroutine());
    }

    private IEnumerator RefreshLayoutCoroutine()
    {
        yield return null; // �ȴ���һ֡

        if (scrollViewContent == null)
        {
            Debug.LogError("scrollViewContent is null in RefreshLayoutCoroutine");
            yield break;
        }

        // ǿ��ˢ�²���
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollViewContent.transform);
        Canvas.ForceUpdateCanvases();

        // ���¼��� Content �ĸ߶�
        float totalHeight = 0;
        foreach (RectTransform child in scrollViewContent)
        {
            totalHeight += child.rect.height;
        }

        // ���� Content �ĸ߶�
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

        // �ٴ�ǿ��ˢ�²���
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollViewContent.transform);
        Canvas.ForceUpdateCanvases();

        // ���ù���λ�õ�����
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