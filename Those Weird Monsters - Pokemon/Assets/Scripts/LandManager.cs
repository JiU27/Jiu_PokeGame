using UnityEngine;
using System.Collections.Generic;

public class LandManager : MonoBehaviour
{
    public LandType currentLandType;
    public GameObject monsterPrefab;
    public Transform[] spawnPoints;

    public List<GameObject> landPrefabs;
    public List<Transform> landGeneratePositions;
    public float generationDistance = 10f;
    public float landCheckRadius = 5f; // 检查附近 Land 的半径

    private PokemonDataLoader dataLoader;
    private Transform player;
    private List<Transform> availablePositions;

    public GameObject oxygenStationPrefab;
    public float oxygenStationSpawnHeight = 0.5f; // 在地面上方的高度
    public int maxSpawnAttempts = 30; // 最大尝试生成次数

    private BackgroundMusicManager musicManager;

    void Start()
    {
        InitializeComponents();
        InitializeAvailablePositions();
        SpawnInitialMonsters();

        musicManager = FindObjectOfType<BackgroundMusicManager>();
        if (musicManager == null)
        {
            Debug.LogError("BackgroundMusicManager not found in the scene!");
        }
        else
        {
            musicManager.SetLandType(currentLandType);
        }
    }

    void InitializeComponents()
    {
        dataLoader = FindObjectOfType<PokemonDataLoader>();
        if (dataLoader == null)
        {
            Debug.LogError("PokemonDataLoader not found in the scene!");
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found in the scene! Make sure the player has the 'Player' tag.");
        }
    }

    void InitializeAvailablePositions()
    {
        availablePositions = new List<Transform>();
        foreach (Transform pos in landGeneratePositions)
        {
            if (pos != null && !IsLandNearby(pos.position))
            {
                availablePositions.Add(pos);
            }
        }
        Debug.Log($"Initialized {availablePositions.Count} available positions for land generation.");
    }

    bool IsLandNearby(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, landCheckRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Land"))
            {
                return true;
            }
        }
        return false;
    }

    void Update()
    {
        CheckAndGenerateLand();
    }

    void SpawnInitialMonsters()
    {
        if (dataLoader == null) return;

        List<Pokemon> landPokemon = dataLoader.GetPokemonByLandType(currentLandType);
        if (landPokemon.Count == 0)
        {
            Debug.LogWarning("No Pokemon found for the current land type!");
            return;
        }

        for (int i = 0; i < spawnPoints.Length && i < landPokemon.Count; i++)
        {
            Pokemon randomPokemon = landPokemon[Random.Range(0, landPokemon.Count)];
            GameObject monster = Instantiate(monsterPrefab, spawnPoints[i].position, Quaternion.identity);
            monster.GetComponent<MonsterBehaviour>().SetPokemon(randomPokemon);
        }
    }

    void CheckAndGenerateLand()
    {
        if (player == null || availablePositions.Count == 0) return;

        for (int i = availablePositions.Count - 1; i >= 0; i--)
        {
            Transform generatePosition = availablePositions[i];
            if (generatePosition == null) continue;

            if (IsLandNearby(generatePosition.position))
            {
                availablePositions.RemoveAt(i);
                Debug.Log($"Removed position {generatePosition.position} due to nearby Land.");
                continue;
            }

            if (Vector3.Distance(player.position, generatePosition.position) <= generationDistance)
            {
                GenerateLand(generatePosition);
                availablePositions.RemoveAt(i);
            }
        }
    }

    void GenerateLand(Transform generatePosition)
    {
        if (landPrefabs == null || landPrefabs.Count == 0)
        {
            Debug.LogWarning("No land prefabs available!");
            return;
        }

        GameObject selectedPrefab = landPrefabs[Random.Range(0, landPrefabs.Count)];
        if (selectedPrefab != null)
        {
            GameObject newLand = Instantiate(selectedPrefab, generatePosition.position, generatePosition.rotation);
            newLand.tag = "Land";
            Debug.Log($"Generated new land '{selectedPrefab.name}' at position: {generatePosition.position}");

            // 尝试生成 OxygenStation
            TrySpawnOxygenStation(newLand);
        }
        else
        {
            Debug.LogError("Selected land prefab is null!");
        }
    }

    void TrySpawnOxygenStation(GameObject land)
    {
        if (Random.value > 0.5f) // 50% 概率生成 OxygenStation
        {
            Vector3 spawnPosition = FindValidSpawnPosition(land);
            if (spawnPosition != Vector3.zero)
            {
                GameObject oxygenStation = Instantiate(oxygenStationPrefab, spawnPosition, Quaternion.identity, land.transform);
                OxygenStation stationScript = oxygenStation.GetComponent<OxygenStation>();
                if (stationScript != null)
                {
                    stationScript.SetActive(Random.value > 0f); // 50% 概率激活
                }
            }
        }
    }

    Vector3 FindValidSpawnPosition(GameObject land)
    {
        Bounds landBounds = land.GetComponent<Collider>().bounds;
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(landBounds.min.x, landBounds.max.x),
                landBounds.max.y + oxygenStationSpawnHeight,
                Random.Range(landBounds.min.z, landBounds.max.z)
            );

            if (!Physics.CheckSphere(randomPoint, 0.5f)) // 检查是否有碰撞
            {
                return randomPoint;
            }
        }
        Debug.LogWarning("Failed to find a valid spawn position for OxygenStation");
        return Vector3.zero;
    }
}