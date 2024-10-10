using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameStatistics : MonoBehaviour
{
    public TextMeshProUGUI statisticsText;
    public float typingSpeed = 0.05f;

    private int recordedPokemonCount = 0;
    private HashSet<int> uniquePokemonEncountered = new HashSet<int>();
    private int strongAttackCount = 0;
    private HashSet<LandType> encounteredLandTypes = new HashSet<LandType>();
    private float oxygenStationTime = 0f;

    private PokeDexManager pokeDexManager;
    private GameControl gameControl;

    private void Start()
    {
        pokeDexManager = FindObjectOfType<PokeDexManager>();
        gameControl = FindObjectOfType<GameControl>();

        if (pokeDexManager == null || gameControl == null)
        {
            Debug.LogError("Required components not found!");
        }
    }

    public void RecordPokemonEncounter(int pokemonId)
    {
        uniquePokemonEncountered.Add(pokemonId);
    }

    public void RecordStrongAttack()
    {
        strongAttackCount++;
    }

    public void RecordLandType(LandType landType)
    {
        encounteredLandTypes.Add(landType);
    }

    public void AddOxygenStationTime(float time)
    {
        oxygenStationTime += time;
    }

    public void DisplayStatistics()
    {
        recordedPokemonCount = pokeDexManager.GetDiscoveredPokemon().Count;
        StartCoroutine(TypeStatistics());
    }

    private IEnumerator TypeStatistics()
    {
        string[] statistics = new string[]
        {
            $"Recorded Pok¨¦mon: {recordedPokemonCount}",
            $"Unique Pok¨¦mon encountered: {uniquePokemonEncountered.Count}",
            $"Strong Pok¨¦mon attacks: {strongAttackCount}",
            $"Land types encountered: {encounteredLandTypes.Count}",
            $"Time spent at Oxygen Stations: {oxygenStationTime:F2} seconds",
            $"Final Score: {CalculateScore():F2}"
        };

        statisticsText.text = "";
        foreach (string stat in statistics)
        {
            foreach (char c in stat)
            {
                statisticsText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }
            statisticsText.text += "\n\n";
            yield return new WaitForSeconds(0.5f);
        }
    }

    private float CalculateScore()
    {
        float score = 0f;
        score += recordedPokemonCount;
        score -= strongAttackCount * 0.5f;
        score += encounteredLandTypes.Count;
        score -= oxygenStationTime * 0.01f;
        return Mathf.Max(score, 0); // Ensure the score is not negative
    }
}