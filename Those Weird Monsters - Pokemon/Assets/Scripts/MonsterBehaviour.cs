using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using System.Collections;

public class MonsterBehaviour : MonoBehaviour
{
    private Pokemon pokemon;
    public Image monsterImage;

    public void SetPokemon(Pokemon newPokemon)
    {
        pokemon = newPokemon;
        // º”‘ÿ∏ﬂ«ÂÕºœÒ
        StartCoroutine(LoadImage(pokemon.image.hires, monsterImage));
    }

    public void OnClick()
    {
        PokeDexManager.instance.DisplayPokemon(pokemon);
    }

    IEnumerator LoadImage(string url, Image image)
    {
        WWW www = new WWW(url);
        yield return www;
        if (www.error == null)
        {
            Texture2D texture = www.texture;
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}

