using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SignedPokemonDisplay : MonoBehaviour
{
    public Image pokemonImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI findPositionText;

    public void SetPokemonData(Pokemon pokemon)
    {
        if (pokemon == null) return;

        if (pokemonImage != null)
        {
            StartCoroutine(LoadImageFromURL(pokemon.image.thumbnail, pokemonImage));
        }

        if (nameText != null)
        {
            nameText.text = pokemon.name.english;
        }

        if (typeText != null)
        {
            typeText.text = string.Join(", ", pokemon.type);
        }

        if (findPositionText != null)
        {
            findPositionText.text = pokemon.foundInLandType.ToString();
        }
    }

    private System.Collections.IEnumerator LoadImageFromURL(string url, Image image)
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