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

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public void DisplayPokemon(Pokemon pokemon)
    {
        currentPokemon = pokemon;
        if (discoveredPokemon.ContainsKey(pokemon.id))
        {
            DisplayDiscoveredPokemon(pokemon);
        }
        else
        {
            DisplayNewPokemon(pokemon);
        }
    }

    private void DisplayDiscoveredPokemon(Pokemon pokemon)
    {
        nameText.text = pokemon.name.english;
        typeText.text = $"Type: {string.Join(", ", pokemon.type)}";
        choice1Text.text = "";
        choice2Text.text = "";
        hintText.text = "";
        StartCoroutine(LoadImage(pokemon.image.thumbnail));
    }

    private void DisplayNewPokemon(Pokemon pokemon)
    {
        nameText.text = pokemon.name.english;
        typeText.text = "Type: ________";
        correctType = pokemon.type[UnityEngine.Random.Range(0, pokemon.type.Length)];
        string wrongType = GetRandomWrongType(correctType);
        if (UnityEngine.Random.value > 0.5f)
        {
            choice1Text.text = correctType;
            choice2Text.text = wrongType;
        }
        else
        {
            choice1Text.text = wrongType;
            choice2Text.text = correctType;
        }
        hintText.text = "";
        pokePhoto.sprite = null;
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
        if ((choice == 1 && choice1Text.text == correctType) || (choice == 2 && choice2Text.text == correctType))
        {
            typeText.text = $"Type: {correctType}";
            hintText.text = "";
            StartCoroutine(LoadImage(currentPokemon.image.thumbnail));
            discoveredPokemon.Add(currentPokemon.id, currentPokemon);
        }
        else
        {
            hintText.text = "Hmm, that doesn't seem right.";
        }
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

    void Update()
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