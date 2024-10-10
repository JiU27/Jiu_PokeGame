using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PokeDexManager : MonoBehaviour
{
    public static PokeDexManager instance;

    public TextMeshPro nameText;
    public TextMeshPro typeText;
    public TextMeshPro choice1Text;
    public TextMeshPro choice2Text;
    public TextMeshPro hintText;
    public SpriteRenderer pokePhoto;
    public Slider dataLoadingSlider;

    private Dictionary<int, Pokemon> discoveredPokemon = new Dictionary<int, Pokemon>();
    private Pokemon currentPokemon;
    private string correctType;
    private LandType currentLandType;

    public Camera mainCamera;
    public float raycastDistance = 100f;
    public Color raycastColor = Color.red;
    public LayerMask raycastLayerMask = -1;

    private bool isInteractionEnabled = true;
    private bool isCapturing = false;
    private float captureProgress = 0f;
    private float captureTime;
    private bool isRenamingMode = false;
    private string newName = "";
    private bool isGuessingType = false;

    private float interactionResetTimer = 0f;
    private const float interactionResetDelay = 2f;
    private GameStatistics gameStatistics;

    private Dictionary<string, Color> typeColors = new Dictionary<string, Color>
    {
        {"Normal", new Color(0.658f, 0.658f, 0.658f)},
        {"Fire", new Color(1f, 0.427f, 0.24f)},
        {"Water", new Color(0.386f, 0.607f, 0.937f)},
        {"Electric", new Color(0.984f, 0.823f, 0.156f)},
        {"Grass", new Color(0.484f, 0.803f, 0.313f)},
        {"Ice", new Color(0.588f, 0.847f, 0.847f)},
        {"Fighting", new Color(0.768f, 0.196f, 0.196f)},
        {"Poison", new Color(0.639f, 0.247f, 0.639f)},
        {"Ground", new Color(0.858f, 0.745f, 0.392f)},
        {"Flying", new Color(0.686f, 0.568f, 0.956f)},
        {"Psychic", new Color(0.952f, 0.427f, 0.517f)},
        {"Bug", new Color(0.658f, 0.723f, 0.133f)},
        {"Rock", new Color(0.713f, 0.627f, 0.231f)},
        {"Ghost", new Color(0.439f, 0.352f, 0.596f)},
        {"Dragon", new Color(0.439f, 0.352f, 0.796f)},
        {"Dark", new Color(0.439f, 0.352f, 0.296f)},
        {"Steel", new Color(0.721f, 0.721f, 0.815f)},
        {"Fairy", new Color(0.956f, 0.603f, 0.956f)}
    };

    void Awake()
    {
        dataLoadingSlider.gameObject.SetActive(false);
        gameStatistics = FindObjectOfType<GameStatistics>();

        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        ClearDisplay();
    }



    void Update()
    {
        if (mainCamera == null) return;

        if (isCapturing)
        {
            if (Input.GetMouseButton(0))
            {
                captureProgress += Time.deltaTime;
                // 归一化进度值
                float normalizedProgress = Mathf.Clamp01(captureProgress / captureTime);
                dataLoadingSlider.value = normalizedProgress;

                if (captureProgress >= captureTime)
                {
                    CompletePokemonCapture();
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (captureProgress < captureTime)
                {
                    CancelPokemonCapture();
                }
            }
        }
        else if (isRenamingMode)
        {
            HandleRenaming();
        }
        else if (isGuessingType)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                MakeGuess(1);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                MakeGuess(2);
            }
        }
        else if (!isInteractionEnabled)
        {
            interactionResetTimer += Time.deltaTime;
            if (interactionResetTimer >= interactionResetDelay)
            {
                ResetInteraction();
            }
        }
    }

    private void HandleRenaming()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // Backspace key
            {
                if (newName.Length > 0)
                {
                    newName = newName.Substring(0, newName.Length - 1);
                }
            }
            else if (c == '\n' || c == '\r') // Enter key
            {
                ConfirmRenaming();
                return;
            }
            else
            {
                newName += c;
            }
        }
        nameText.text = $"Name: {newName}";
    }

    private void DisplayDiscoveredPokemon(Pokemon pokemon)
    {
        nameText.text = $"Name: {pokemon.name.english}";
        typeText.text = $"Type: {string.Join(", ", pokemon.type)}";
        choice1Text.text = "";
        choice2Text.text = "";
        hintText.text = "Monster is in system";
        StartCoroutine(LoadImage(pokemon.image.thumbnail));
        isInteractionEnabled = false;
        interactionResetTimer = 0f; // 开始计时以重置交互
    }

    private void ResetInteraction()
    {
        isInteractionEnabled = true;
        interactionResetTimer = 0f;
        ClearDisplay();
    }

    private void ClearDisplay()
    {
        nameText.text = "";
        typeText.text = "";
        choice1Text.text = "";
        choice2Text.text = "";
        hintText.text = "";
        pokePhoto.sprite = null;
    }

    public void StartPokemonCapture(Pokemon pokemon)
    {
        if (!isInteractionEnabled) return;

        currentPokemon = pokemon;
        captureTime = currentPokemon.GetCaptureTime(); // 保留原有的计算
        dataLoadingSlider.maxValue = 1f; // 设置最大值为 1
        dataLoadingSlider.value = 0f;
        dataLoadingSlider.gameObject.SetActive(true);

        isCapturing = true;
        captureProgress = 0f;

        ClearDisplay();
        hintText.text = "Hold the mouse button to scan.";
        //gameStatistics.RecordPokemonEncounter(pokemon.id);
    }


    private void CompletePokemonCapture()
    {
        isCapturing = false;
        dataLoadingSlider.gameObject.SetActive(false);

        if (!discoveredPokemon.ContainsKey(currentPokemon.id))
        {
            StartNamingProcess();
        }
        else
        {
            DisplayDiscoveredPokemon(currentPokemon);
        }
    }

    private void CancelPokemonCapture()
    {
        isCapturing = false;
        dataLoadingSlider.gameObject.SetActive(false);
        hintText.text = "Scanning failed. Try again.";
        StartCoroutine(ResetCaptureAfterDelay(1f));
    }

    private IEnumerator ResetCaptureAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearDisplay();
        isInteractionEnabled = true;
    }

    private void StartNamingProcess()
    {
        isRenamingMode = true;
        newName = "";
        nameText.text = "Name: ";
        hintText.text = "Give it a name you like (Type, Enter to Confirm)";
    }

    private void ConfirmRenaming()
    {
        if (!string.IsNullOrEmpty(newName))
        {
            currentPokemon.name.english = newName;
            isRenamingMode = false;
            StartTypeGuessing();
        }
        else
        {
            hintText.text = "Please enter a valid name!";
        }
    }

    private void StartTypeGuessing()
    {
        nameText.text = currentPokemon.name.english;
        typeText.text = "Type: ________";
        correctType = currentPokemon.type[Random.Range(0, currentPokemon.type.Length)];
        string wrongType = GetRandomWrongType(correctType);
        SetColoredTypeText(choice1Text, correctType);
        SetColoredTypeText(choice2Text, wrongType);
        if (Random.value <= 0.5f)
        {
            (choice1Text.text, choice2Text.text) = (choice2Text.text, choice1Text.text);
        }
        hintText.text = "Press Q for left choice, E for right choice.";
        isGuessingType = true;
    }

    public void MakeGuess(int choice)
    {
        if (!isGuessingType) return;

        if ((choice == 1 && choice1Text.text.Contains(correctType)) || (choice == 2 && choice2Text.text.Contains(correctType)))
        {
            typeText.text = $"Type: {correctType}";
            hintText.text = "Correct! Monster in system!";
            StartCoroutine(LoadImage(currentPokemon.image.thumbnail));
            currentPokemon.foundInLandType = currentLandType;
            discoveredPokemon[currentPokemon.id] = currentPokemon;

            choice1Text.text = "";
            choice2Text.text = "";
            isGuessingType = false;
        }
        else
        {
            hintText.text = "Hmm, that doesn't seem right.";
        }
    }


    private void SetColoredTypeText(TextMeshPro textComponent, string type)
    {
        if (typeColors.TryGetValue(type, out Color color))
        {
            textComponent.text = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{type}</color>";
        }
        else
        {
            textComponent.text = type;
        }
    }

    private string GetRandomWrongType(string correctType)
    {
        string[] allTypes = typeColors.Keys.ToArray();
        List<string> wrongTypes = new List<string>(allTypes);
        wrongTypes.Remove(correctType);
        return wrongTypes[Random.Range(0, wrongTypes.Count)];
    }

    IEnumerator LoadImage(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                pokePhoto.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogError("Error loading image: " + webRequest.error);
            }
        }
    }

    public Dictionary<int, Pokemon> GetDiscoveredPokemon()
    {
        return discoveredPokemon;
    }

    public void SetCurrentLandType(LandType landType)
    {
        currentLandType = landType;
    }
}