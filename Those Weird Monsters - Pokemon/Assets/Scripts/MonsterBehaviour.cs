using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MonsterBehaviour : MonoBehaviour
{
    private Pokemon pokemon;
    public SpriteRenderer monsterSprite;
    private Transform playerTransform;

    void Start()
    {
        // 在游戏开始时查找名为 "Player" 的 GameObject
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
        // 如果找到了玩家，让怪物始终朝向玩家
        if (playerTransform != null)
        {
            // 计算朝向玩家的方向
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0; // 保持Y轴不变，防止上下倾斜

            // 如果怪物和玩家不在同一位置，才进行旋转
            if (directionToPlayer != Vector3.zero)
            {
                // 创建一个新的旋转，使怪物面向玩家
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

                // 平滑旋转
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
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
}