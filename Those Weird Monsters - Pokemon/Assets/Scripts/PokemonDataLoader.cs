using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using Newtonsoft.Json.Linq; // 使用 JSON.Net 解析

// 定义宝可梦数据模型
[Serializable]
public class Pokemon
{
    public int id;
    public Name name;
    public string[] type;
    public PokemonImage image;
    public Dictionary<string, int> baseStats; // 使用字典来存储属性，能够处理带空格的键名
    public LandType foundInLandType { get; set; }

    [Serializable]
    public class Name
    {
        public string english;
    }

    [Serializable]
    public class PokemonImage
    {
        public string thumbnail;
        public string hires;
    }

    public EnumStatus DetermineStatus()
    {
        int attack = baseStats.ContainsKey("Attack") ? baseStats["Attack"] : 0;
        int spAttack = baseStats.ContainsKey("Sp. Attack") ? baseStats["Sp. Attack"] : 0;

        if (attack < 80 && spAttack < 80)
        {
            return EnumStatus.Weak;
        }
        else if ((attack >= 80 && attack <= 120) || (spAttack >= 80 && spAttack <= 120))
        {
            return EnumStatus.Normal;
        }
        else if (attack > 60 && attack <= 120 && spAttack > 60 && spAttack <= 120)
        {
            return EnumStatus.Curious;
        }
        else if (attack > 120 || spAttack > 120)
        {
            return EnumStatus.Strong;
        }
        else
        {
            return EnumStatus.Normal;  // 默认状态
        }
    }

    public float GetCaptureTime()
    {
        int total = baseStats.Values.Sum();
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

public enum EnumStatus
{
    Weak,
    Normal,
    Curious,
    Strong
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
        JArray jsonArray = JArray.Parse(jsonString);
        allPokemon = new List<Pokemon>();

        foreach (JObject item in jsonArray)
        {
            Pokemon pokemon = new Pokemon
            {
                id = (int)item["id"],
                name = item["name"].ToObject<Pokemon.Name>(),
                type = item["type"].ToObject<string[]>(),
                image = item["image"].ToObject<Pokemon.PokemonImage>(),
                baseStats = item["base"].ToObject<Dictionary<string, int>>()
            };
            allPokemon.Add(pokemon);
        }
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