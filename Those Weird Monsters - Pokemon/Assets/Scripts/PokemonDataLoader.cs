using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;

// 定义宝可梦数据模型
[Serializable]
public class Pokemon
{
    public int id;
    public Name name;
    public string[] type;
    public Image image;
    public Base baseStats;
    public LandType foundInLandType { get; set; }

    [Serializable]
    public class Name
    {
        public string english;
    }

    [Serializable]
    public class Image
    {
        public string thumbnail;
        public string hires;
    }

    [Serializable]
    public class Base
    {
        public int HP;
        public int Attack;
        public int Defense;
        public int SpAttack;
        public int SpDefense;
        public int Speed;

        public int GetTotal()
        {
            return HP + Attack + Defense + SpAttack + SpDefense + Speed;
        }
    }

    public float GetCaptureTime()
    {
        int total = baseStats.GetTotal();
        // 假设最大总和为 720，最小为 180
        float normalizedTotal = Mathf.Clamp01((total - 180f) / 540f);
        return Mathf.Lerp(2.5f, 5f, normalizedTotal);
    }
}

public enum LandType
{
    GrassLand,
    IceLand,
    FireLand,
    Night,
    RockLand
}

// 数据加载器
[System.Serializable]
public class PokemonList
{
    public List<Pokemon> pokemons;
}

public class PokemonDataLoader : MonoBehaviour
{
    public TextAsset pokedexJson;
    public List<Pokemon> allPokemon;

    void Awake()
    {
        string jsonString = pokedexJson.text;
        PokemonList pokemonList = JsonUtility.FromJson<PokemonList>("{\"pokemons\":" + jsonString + "}");
        allPokemon = pokemonList.pokemons;
    }

    public List<Pokemon> GetPokemonByLandType(LandType landType)
    {
        switch (landType)
        {
            case LandType.GrassLand:
                return allPokemon.Where(p => p.type.Intersect(new[] { "Grass", "Bug", "Poison" }).Any()).ToList();
            case LandType.IceLand:
                return allPokemon.Where(p => p.type.Intersect(new[] { "Water", "Ice" }).Any()).ToList();
            case LandType.FireLand:
                return allPokemon.Where(p => p.type.Contains("Fire")).ToList();
            case LandType.Night:
                return allPokemon.Where(p => p.type.Intersect(new[] { "Ghost", "Dark" }).Any()).ToList();
            case LandType.RockLand:
                return allPokemon.Where(p => p.type.Intersect(new[] { "Rock", "Ground" }).Any()).ToList();
            default:
                return new List<Pokemon>();
        }
    }
}