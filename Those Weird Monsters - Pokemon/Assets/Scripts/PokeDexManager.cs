using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using TMPro;
using UnityEngine.Networking;

public class PokeDexManager : MonoBehaviour
{
    public static PokeDexManager instance;

    public TextMeshPro nameText;
    public TextMeshPro typeText;
    public TextMeshPro choice1Text;
    public TextMeshPro choice2Text;
    public TextMeshPro hintText;
    public SpriteRenderer pokePhoto;

    private Dictionary<int, Pokemon> discoveredPokemon = new Dictionary<int, Pokemon>();
    private Pokemon currentPokemon;
    private string correctType;
    private LandType currentLandType;

    public Camera mainCamera;
    public float raycastDistance = 100f;
    public Color raycastColor = Color.red;
    public LayerMask raycastLayerMask = -1;

    private bool isInteractionEnabled = true;
    private bool hasPokemonSelected = false;
    private float cooldownTimer = 0f;
    private const float cooldownDuration = 3f;
    private bool isRenamingMode = false;
    private string newName = "";

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
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No camera found. Please assign a camera to the PokeDexManager or tag your main camera as 'MainCamera'.");
            }
        }

        ClearDisplay();
    }

    void Update()
    {
        if (mainCamera == null) return;

        if (isRenamingMode)
        {
            HandleRenaming();
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, raycastColor);

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, raycastDistance, raycastLayerMask))
            {
                MonsterBehaviour monster = hit.collider.GetComponent<MonsterBehaviour>();
                if (monster != null)
                {
                    Pokemon pokemon = monster.GetPokemon();
                    if (discoveredPokemon.ContainsKey(pokemon.id))
                    {
                        Debug.Log($"Pokemon already discovered: {pokemon.name.english}");
                        Debug.Log($"Stored information: {JsonUtility.ToJson(discoveredPokemon[pokemon.id], true)}");
                    }
                    DisplayPokemon(pokemon);
                }
            }
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                EnableChoices();
            }
        }

        if (isInteractionEnabled && hasPokemonSelected)
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
    }

    private void HandleRenaming()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // 退格键
            {
                if (newName.Length > 0)
                {
                    newName = newName.Substring(0, newName.Length - 1);
                }
            }
            else if (c == '\n' || c == '\r') // 回车键
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


    private void ClearDisplay()
    {
        nameText.text = "";
        typeText.text = "";
        choice1Text.text = "";
        choice2Text.text = "";
        hintText.text = "";
        pokePhoto.sprite = null;
        hasPokemonSelected = false;
        isInteractionEnabled = true;
    }

    public void DisplayPokemon(Pokemon pokemon)
    {
        currentPokemon = pokemon;
        hasPokemonSelected = true;
        isInteractionEnabled = true;
        isRenamingMode = false;

        if (discoveredPokemon.ContainsKey(pokemon.id))
        {
            DisplayDiscoveredPokemon(pokemon);
        }
        else
        {
            DisplayNewPokemon(pokemon);
        }
    }


    private void DisplayNewPokemon(Pokemon pokemon)
    {
        nameText.text = pokemon.name.english;
        typeText.text = "Type: ________";
        correctType = pokemon.type[UnityEngine.Random.Range(0, pokemon.type.Length)];
        string wrongType = GetRandomWrongType(correctType);
        SetColoredTypeText(choice1Text, correctType);
        SetColoredTypeText(choice2Text, wrongType);
        if (UnityEngine.Random.value <= 0.5f)
        {
            (choice1Text.text, choice2Text.text) = (choice2Text.text, choice1Text.text);
        }
        hintText.text = "";
        pokePhoto.sprite = null;
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
        string[] allTypes = { "Normal", "Fire", "Water", "Electric", "Grass", "Ice", "Fighting", "Poison", "Ground", "Flying", "Psychic", "Bug", "Rock", "Ghost", "Dragon", "Dark", "Steel", "Fairy" };
        List<string> wrongTypes = new List<string>(allTypes);
        wrongTypes.Remove(correctType);
        return wrongTypes[UnityEngine.Random.Range(0, wrongTypes.Count)];
    }

    public void MakeGuess(int choice)
    {
        if ((choice == 1 && choice1Text.text.Contains(correctType)) || (choice == 2 && choice2Text.text.Contains(correctType)))
        {
            typeText.text = $"Type: {correctType}";
            hintText.text = "Give it a name you like(Type, Enter to Confirm)";
            StartCoroutine(LoadImage(currentPokemon.image.thumbnail));
            currentPokemon.foundInLandType = currentLandType;
            discoveredPokemon[currentPokemon.id] = currentPokemon;
            isInteractionEnabled = false;
            isRenamingMode = true;
            newName = "";

            // 立即清除 Choice1 和 Choice2 的文本
            choice1Text.text = "";
            choice2Text.text = "";
        }
        else
        {
            hintText.text = "Hmm, that doesn't seem right.";
            DisableChoices();
        }
    }

    private void ConfirmRenaming()
    {
        if (currentPokemon != null && !string.IsNullOrEmpty(newName))
        {
            currentPokemon.name.english = newName;
            discoveredPokemon[currentPokemon.id] = currentPokemon;
            isRenamingMode = false;
            DisplayDiscoveredPokemon(currentPokemon);

            // 清除 Hint 文本
            hintText.text = "";
        }
    }

    private void DisplayDiscoveredPokemon(Pokemon pokemon)
    {
        nameText.text = $"Name: {pokemon.name.english}";
        typeText.text = $"Type: {string.Join(", ", pokemon.type)}";
        choice1Text.text = "";
        choice2Text.text = "";
        hintText.text = ""; // 不再显示 "Give it a name you like"
        StartCoroutine(LoadImage(pokemon.image.thumbnail));
        isInteractionEnabled = false;
    }

    private void DisableChoices()
    {
        choice1Text.text = "";
        choice2Text.text = "";
        isInteractionEnabled = false;
        cooldownTimer = cooldownDuration;
    }

    private void EnableChoices()
    {
        DisplayNewPokemon(currentPokemon);
        isInteractionEnabled = true;
        hintText.text = "";
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