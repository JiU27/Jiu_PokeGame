// LandManager.cs
using UnityEngine;
using System.Collections.Generic;

public class LandManager : MonoBehaviour
{
    public LandType currentLandType;
    public GameObject monsterPrefab;
    public Transform[] spawnPoints;
    public PokemonDataLoader dataLoader;

    void Start()
    {
        if (dataLoader == null)
        {
            Debug.LogError("PokemonDataLoader is not assigned!");
            return;
        }

        List<Pokemon> landPokemon = dataLoader.GetPokemonByLandType(currentLandType);

        if (landPokemon.Count == 0)
        {
            Debug.LogWarning("No Pokemon found for the current land type!");
            return;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (i < landPokemon.Count)
            {
                Pokemon randomPokemon = landPokemon[Random.Range(0, landPokemon.Count)];
                GameObject monster = Instantiate(monsterPrefab, spawnPoints[i].position, Quaternion.identity);
                monster.GetComponent<MonsterBehaviour>().SetPokemon(randomPokemon);
            }
            else
            {
                Debug.LogWarning("Not enough Pokemon for all spawn points!");
                break;
            }
        }
    }
}