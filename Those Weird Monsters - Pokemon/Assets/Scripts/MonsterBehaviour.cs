using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class MonsterBehaviour : MonoBehaviour
{
    private Pokemon pokemon;
    public SpriteRenderer monsterSprite;
    private Transform playerTransform;
    private NavMeshAgent agent;
    private bool isMouseOver = false;
    [SerializeField] private EnumStatus currentStatus;  // Make currentStatus visible in the Inspector
    private float fleeTimer = 0f;
    private float observeTimer = 5f;
    private float playerDetectionTimer = 0f;
    public float fleeDuration = 5f;
    public float curiosityRange = 10f;
    public float fleeRange = 5f;
    public float territoryRange = 15f;
    public float throwOutForce = 10f;
    public AudioClip warningClip;
    private AudioSource audioSource;
    private GameObject targetSlot = null;
    public List<GameObject> emotionPrefabs;  // List of emotion prefabs
    private GameStatistics gameStatistics;

    public enum EnumStatus
    {
        Weak,
        Normal,
        Curious,
        Strong
    }

    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogWarning("Player GameObject not found in the scene.");
        }

        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        SetInitialStatus();

        // Adjust BoxCollider to match Sprite size
        AdjustColliderToSpriteSize();

        // Check overlap with Land and adjust position
        StartCoroutine(CheckAndAdjustPosition());

        // Start random behavior coroutine for normal state
        StartCoroutine(PerformNormalBehavior());

        gameStatistics = FindObjectOfType<GameStatistics>();
    }

    void Update()
    {
        // Always face the player
        if (playerTransform != null)
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0; // Ignore vertical rotation
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }

        // Handle current status behavior
        HandleStatusBehavior();

        // Update status based on player presence
        UpdatePlayerPresence();

        // Handle raycast on mouse click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                MonsterBehaviour monster = hit.collider.GetComponent<MonsterBehaviour>();
                if (monster != null)
                {
                    // Trigger interaction with the selected Pokemon
                    Debug.Log($"Interacting with: {monster.pokemon.name.english}");
                    PokeDexManager.instance.StartPokemonCapture(monster.pokemon);
                }
            }
        }
    }

    private void FacePlayer()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0; // Ignore vertical rotation
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private void HandleStatusBehavior()
    {
        switch (currentStatus)
        {
            case EnumStatus.Weak:
                HandleWeakState();
                break;
            case EnumStatus.Normal:
                // Normal monsters perform random behaviors
                break;
            case EnumStatus.Curious:
                HandleCuriousState();
                break;
            case EnumStatus.Strong:
                HandleStrongState();
                break;
        }
    }

    private void UpdatePlayerPresence()
    {
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(playerTransform.position, transform.position);
            if (distanceToPlayer < curiosityRange || distanceToPlayer < fleeRange || distanceToPlayer < territoryRange)
            {
                playerDetectionTimer = 5f;  // Reset timer when player is detected
                UpdateStatusBasedOnProximity(distanceToPlayer);
            }
            else
            {
                playerDetectionTimer -= Time.deltaTime;
                if (playerDetectionTimer <= 0f)
                {
                    currentStatus = EnumStatus.Normal;
                }
            }
        }
    }

    private void HandleWeakState()
    {
        float distanceToPlayer = Vector3.Distance(playerTransform.position, transform.position);
        if (distanceToPlayer < fleeRange && fleeTimer <= 0f)
        {
            // Start fleeing
            Vector3 fleeDirection = (transform.position - playerTransform.position).normalized;
            Vector3 fleeTarget = transform.position + fleeDirection * fleeRange;
            agent.SetDestination(fleeTarget);
            fleeTimer = fleeDuration;
        }

        if (fleeTimer > 0f)
        {
            fleeTimer -= Time.deltaTime;
            if (fleeTimer <= 0f)
            {
                // Enter normal state for 5 seconds before returning to weak state
                currentStatus = EnumStatus.Normal;
                observeTimer = 5f;
            }
        }
        else if (currentStatus == EnumStatus.Normal)
        {
            observeTimer -= Time.deltaTime;
            if (observeTimer <= 0f)
            {
                currentStatus = EnumStatus.Weak;
            }
        }
    }

    private void HandleCuriousState()
    {
        float distanceToPlayer = Vector3.Distance(playerTransform.position, transform.position);
        if (distanceToPlayer < curiosityRange)
        {
            agent.SetDestination(playerTransform.position);
            if (distanceToPlayer <= agent.stoppingDistance)
            {
                // Stop moving and look curious around the player
                agent.ResetPath();
            }
        }
    }

    private void HandleStrongState()
    {
        // Draw the territory range in the game for visualization
        Debug.DrawLine(transform.position, transform.position + Vector3.forward * territoryRange, Color.red);
        Debug.DrawLine(transform.position, transform.position + Vector3.back * territoryRange, Color.red);
        Debug.DrawLine(transform.position, transform.position + Vector3.left * territoryRange, Color.red);
        Debug.DrawLine(transform.position, transform.position + Vector3.right * territoryRange, Color.red);

        // If there is no current target, find one
        if (targetSlot == null)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, territoryRange);
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Player") || collider.GetComponent<MonsterBehaviour>()?.currentStatus != EnumStatus.Strong)
                {
                    targetSlot = collider.gameObject;
                    break;
                }
            }
        }
        else
        {
            // If a target is assigned, handle the throw out behavior
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(warningClip);
            }

            StartCoroutine(HandleThrowOut(targetSlot));
        }
    }

    private IEnumerator HandleThrowOut(GameObject target)
    {
        yield return new WaitForSeconds(2f);

        if (target != null)
        {
            Debug.Log($"Fighting! GameObject: {gameObject.name}");

            Vector3 direction = (target.transform.position - transform.position).normalized;
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(direction * throwOutForce, ForceMode.Impulse);
                gameStatistics.RecordStrongAttack();
            }

            // Clear the target slot after throwing out the target
            targetSlot = null;
        }
    }

    public void SetPokemon(Pokemon newPokemon)
    {
        pokemon = newPokemon;
        StartCoroutine(LoadImage(pokemon.image.hires));
        SetInitialStatus();
    }

    private void SetInitialStatus()
    {
        if (pokemon != null)
        {
            currentStatus = (EnumStatus)System.Enum.Parse(typeof(EnumStatus), pokemon.DetermineStatus().ToString());
        }
        else
        {
            Debug.LogWarning("Pokemon data is not set correctly.");
        }
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
                AdjustColliderToSpriteSize();
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

    private IEnumerator CheckAndAdjustPosition()
    {
        Collider landCollider;
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            yield break;
        }

        do
        {
            landCollider = Physics.OverlapBox(transform.position, boxCollider.size / 2, transform.rotation, LayerMask.GetMask("Land")).FirstOrDefault();
            if (landCollider != null)
            {
                transform.position += Vector3.up * 0.1f;
                yield return new WaitForSeconds(0.05f);
            }
        } while (landCollider != null);
    }

    private void AdjustColliderToSpriteSize()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null && monsterSprite != null && monsterSprite.sprite != null)
        {
            boxCollider.size = new Vector3(monsterSprite.sprite.bounds.size.x, monsterSprite.sprite.bounds.size.y, boxCollider.size.z);
            boxCollider.center = new Vector3(monsterSprite.sprite.bounds.center.x, monsterSprite.sprite.bounds.center.y, boxCollider.center.z);
        }
    }

    private IEnumerator PerformNormalBehavior()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5f, 10f));

            if (currentStatus == EnumStatus.Normal)
            {
                int behavior = Random.Range(0, 2);
                switch (behavior)
                {
                    case 0:
                        // Random movement within a specified range
                        Vector3 randomTarget = transform.position + new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
                        agent.SetDestination(randomTarget);
                        Debug.Log($"{gameObject.name} is moving randomly to {randomTarget}");
                        break;
                    case 1:
                        // Find a nearby Pokémon to interact with
                        Collider[] nearbyMonsters = Physics.OverlapSphere(transform.position, 10f);
                        GameObject otherMonster = nearbyMonsters.FirstOrDefault(col => col.GetComponent<MonsterBehaviour>() && col.gameObject != gameObject)?.gameObject;
                        if (otherMonster != null)
                        {
                            float interactionDistance = 2f; // 设置一个合理的交互距离
                            Vector3 directionToOther = (otherMonster.transform.position - transform.position).normalized;
                            Vector3 interactionPoint = otherMonster.transform.position - directionToOther * interactionDistance;

                            agent.SetDestination(interactionPoint);
                            Debug.Log($"{gameObject.name} is moving to interact with {otherMonster.name} at {interactionPoint}");

                            // 等待直到达到交互点或无法继续移动
                            yield return new WaitUntil(() =>
                                Vector3.Distance(transform.position, interactionPoint) <= agent.stoppingDistance ||
                                !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);

                            // 检查是否真的接近了目标
                            if (Vector3.Distance(transform.position, otherMonster.transform.position) <= interactionDistance + 1f)
                            {
                                Debug.Log($"{gameObject.name} has reached interaction point near {otherMonster.name}, showing emotion");
                                ShowEmotion();

                                // 让两个怪物面对彼此
                                Vector3 lookDirection = otherMonster.transform.position - transform.position;
                                lookDirection.y = 0;
                                if (lookDirection != Vector3.zero)
                                {
                                    transform.rotation = Quaternion.LookRotation(lookDirection);
                                }

                                // 让另一个怪物也显示情感并面向这个怪物
                                MonsterBehaviour otherBehavior = otherMonster.GetComponent<MonsterBehaviour>();
                                if (otherBehavior != null)
                                {
                                    otherBehavior.ShowEmotion();
                                    lookDirection = -lookDirection;
                                    if (lookDirection != Vector3.zero)
                                    {
                                        otherMonster.transform.rotation = Quaternion.LookRotation(lookDirection);
                                    }
                                }

                                // 稍作停留
                                yield return new WaitForSeconds(2f);
                            }
                            else
                            {
                                Debug.Log($"{gameObject.name} couldn't reach {otherMonster.name}, interaction failed");
                            }
                        }
                        else
                        {
                            Debug.Log($"{gameObject.name} couldn't find a nearby monster to interact with");
                        }
                        break;
                }
            }
            else
            {
                Debug.Log($"{gameObject.name} is not in Normal state, current status: {currentStatus}");
            }
        }
    }

    public void ShowEmotion()
    {
        if (emotionPrefabs != null && emotionPrefabs.Count > 0)
        {
            // Calculate position in front of and above the monster
            Vector3 emotionPosition = transform.position + transform.forward * 0.1f + Vector3.up * 1f;

            // Get a random emotion prefab
            GameObject emotionPrefab = emotionPrefabs[Random.Range(0, emotionPrefabs.Count)];

            // Instantiate the emotion
            GameObject emotion = Instantiate(emotionPrefab, emotionPosition, Quaternion.identity);

            // Make the emotion face the camera
            emotion.transform.LookAt(Camera.main.transform);

            Debug.Log($"Emotion generated for {gameObject.name} at position {emotionPosition}");

            // Destroy the emotion after 2 seconds
            Destroy(emotion, 4f);
        }
        else
        {
            Debug.LogWarning("Emotion prefabs list is empty or not assigned for " + gameObject.name);
        }
    }

    private void UpdateStatusBasedOnProximity(float distanceToPlayer)
    {
        if (pokemon != null)
        {
            currentStatus = (EnumStatus)System.Enum.Parse(typeof(EnumStatus), pokemon.DetermineStatus().ToString());
        }
        else
        {
            if (distanceToPlayer < fleeRange)
            {
                currentStatus = EnumStatus.Weak;
            }
            else if (distanceToPlayer < curiosityRange)
            {
                currentStatus = EnumStatus.Curious;
            }
            else if (distanceToPlayer < territoryRange)
            {
                currentStatus = EnumStatus.Strong;
            }
        }
    }
}