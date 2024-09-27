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
        // ����Ϸ��ʼʱ������Ϊ "Player" �� GameObject
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
        // ����ҵ�����ң��ù���ʼ�ճ������
        if (playerTransform != null)
        {
            // ���㳯����ҵķ���
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0; // ����Y�᲻�䣬��ֹ������б

            // ����������Ҳ���ͬһλ�ã��Ž�����ת
            if (directionToPlayer != Vector3.zero)
            {
                // ����һ���µ���ת��ʹ�����������
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

                // ƽ����ת
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