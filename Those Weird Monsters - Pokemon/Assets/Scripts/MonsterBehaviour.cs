using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MonsterBehaviour : MonoBehaviour
{
    private Pokemon pokemon;
    public SpriteRenderer monsterSprite;

    public void SetPokemon(Pokemon newPokemon)
    {
        pokemon = newPokemon;
        // º”‘ÿ∏ﬂ«ÂÕºœÒ
        StartCoroutine(LoadImage(pokemon.image.hires));
    }

    public void OnClick()
    {
        PokeDexManager.instance.DisplayPokemon(pokemon);
    }

    IEnumerator LoadImage(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                monsterSprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogError("Error loading image: " + webRequest.error);
            }
        }
    }
}