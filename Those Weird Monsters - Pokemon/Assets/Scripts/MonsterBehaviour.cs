using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MonsterBehaviour : MonoBehaviour
{
    private Pokemon pokemon;
    public SpriteRenderer monsterSprite;
    private Transform playerTransform;

    private bool isMouseOver = false;

    void Start()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Player GameObject not found in the scene.");
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0;
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }

        if (isMouseOver && Input.GetMouseButtonDown(0))
        {
            PokeDexManager.instance.StartPokemonCapture(pokemon);
        }
    }

    public void SetPokemon(Pokemon newPokemon)
    {
        pokemon = newPokemon;
        StartCoroutine(LoadImage(pokemon.image.hires));
    }

    public Pokemon GetPokemon()
    {
        return pokemon;
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
        }
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
    }
}