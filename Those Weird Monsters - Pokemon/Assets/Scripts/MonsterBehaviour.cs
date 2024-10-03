using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.AI;

public class MonsterBehaviour : MonoBehaviour
{
    public enum MonsterStatus
    {
        Weak,
        Normal,
        Curious,
        Strong
    }

    private Pokemon pokemon;
    public SpriteRenderer monsterSprite;
    private Transform playerTransform;
    private bool isMouseOver = false;
    private NavMeshAgent agent;
    private MonsterStatus status;

    [Header("Behaviour Settings")]
    public float weakFleeRange = 5f;
    public float weakFleeTime = 10f;
    public float weakObservationTime = 2f;
    public float curiousApproachRange = 8f;
    public float curiousStopDistance = 3f;
    public float strongTerritoryRadius = 10f;

    [Header("Audio")]
    public AudioClip warningSound;
    private AudioSource audioSource;

    private bool isFlooding = false;
    private bool isObserving = false;
    private float strongWarningTimer = 0f;

    void Start()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        StartCoroutine(BehaviourLoop());
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

        if (status == MonsterStatus.Strong)
        {
            CheckTerritory();
        }
    }

    public void SetPokemon(Pokemon newPokemon)
    {
        pokemon = newPokemon;
        StartCoroutine(LoadImage(pokemon.image.hires));
        DetermineStatus();
    }

    private void DetermineStatus()
    {
        int attack = pokemon.baseStats.Attack;
        int spAttack = pokemon.baseStats.SpAttack;

        if (attack < 60 && spAttack < 60)
            status = MonsterStatus.Weak;
        else if (attack > 100 || spAttack > 100)
            status = MonsterStatus.Strong;
        else if (attack >= 60 && attack <= 100 && spAttack >= 60 && spAttack <= 100)
            status = MonsterStatus.Curious;
        else
            status = MonsterStatus.Normal;
    }

    private IEnumerator BehaviourLoop()
    {
        while (true)
        {
            switch (status)
            {
                case MonsterStatus.Weak:
                    yield return StartCoroutine(WeakBehaviour());
                    break;
                case MonsterStatus.Normal:
                    yield return StartCoroutine(NormalBehaviour());
                    break;
                case MonsterStatus.Curious:
                    yield return StartCoroutine(CuriousBehaviour());
                    break;
                case MonsterStatus.Strong:
                    yield return StartCoroutine(StrongBehaviour());
                    break;
            }
            yield return null;
        }
    }

    private IEnumerator WeakBehaviour()
    {
        while (true)
        {
            if (Vector3.Distance(transform.position, playerTransform.position) < weakFleeRange)
            {
                StartCoroutine(Flee());
                yield return new WaitForSeconds(weakFleeTime);
                yield return StartCoroutine(NormalBehaviour());
                yield return new WaitForSeconds(5f);
            }
            yield return null;
        }
    }

    private IEnumerator Flee()
    {
        isFlooding = true;
        float fleeTimer = 0f;
        while (fleeTimer < weakFleeTime)
        {
            Vector3 fleeDirection = transform.position - playerTransform.position;
            Vector3 fleePosition = transform.position + fleeDirection.normalized * 5f;
            agent.SetDestination(fleePosition);

            fleeTimer += Time.deltaTime;
            if (fleeTimer % 3f < 0.1f) // Every 3 seconds approx
            {
                yield return StartCoroutine(Observe());
            }
            yield return null;
        }
        isFlooding = false;
    }

    private IEnumerator Observe()
    {
        isObserving = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(weakObservationTime);
        agent.isStopped = false;
        isObserving = false;
    }

    private IEnumerator NormalBehaviour()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator CuriousBehaviour()
    {
        while (true)
        {
            if (Vector3.Distance(transform.position, playerTransform.position) < curiousApproachRange)
            {
                agent.stoppingDistance = curiousStopDistance;
                agent.SetDestination(playerTransform.position);
                while (agent.remainingDistance > agent.stoppingDistance)
                {
                    yield return null;
                }
                // Perform curious idle animation here
                yield return new WaitForSeconds(5f);
            }
            yield return null;
        }
    }

    private IEnumerator StrongBehaviour()
    {
        while (true)
        {
            yield return null; // The main logic is in Update and CheckTerritory
        }
    }

    private void CheckTerritory()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, strongTerritoryRadius);
        bool intruderDetected = false;

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player") ||
                (col.GetComponent<MonsterBehaviour>() != null &&
                 col.GetComponent<MonsterBehaviour>().status != MonsterStatus.Strong))
            {
                intruderDetected = true;
                strongWarningTimer += Time.deltaTime;

                if (strongWarningTimer > 2f)
                {
                    // Attack the intruder
                    agent.SetDestination(col.transform.position);
                    if (agent.remainingDistance < 0.5f)
                    {
                        Vector3 throwDirection = (col.transform.position - transform.position).normalized;
                        throwDirection.y = 0.5f; // Add some upward force
                        col.GetComponent<Rigidbody>().AddForce(throwDirection * 10f, ForceMode.Impulse);
                    }
                }
                else if (strongWarningTimer < 0.1f) // Play warning sound only once
                {
                    audioSource.PlayOneShot(warningSound);
                }
                break;
            }
        }

        if (!intruderDetected)
        {
            strongWarningTimer = 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (status == MonsterStatus.Strong)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, strongTerritoryRadius);
        }
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