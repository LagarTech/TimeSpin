using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Leaderboards;
using UnityEngine;

public class Ranking_Menu : MonoBehaviour
{
    public static Ranking_Menu instance;
    [SerializeField] private TMP_Text[] _tableEntriesTexts;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateRanking()
    {
        StartCoroutine(GetOnlineRanking());
    }

    private IEnumerator GetOnlineRanking()
    {
        // Cargar las primeras 10 mejores puntuaciones del leaderboard
        var loadScoresTask = LeaderboardsService.Instance.GetScoresAsync("Time_Spin_Ranking",
            new GetScoresOptions { Limit = 10, IncludeMetadata = true });
        yield return new WaitUntil(() => loadScoresTask.IsCompleted);

        if (loadScoresTask.IsFaulted)
        {
            Debug.LogError("Error al cargar las puntuaciones: " + loadScoresTask.Exception);
        }
        else
        {
            Debug.Log("Puntuaciones cargadas con éxito:");
            int numEntry = 0;
            foreach (var entry in loadScoresTask.Result.Results)
            {
                // Se deserializan los datos
                Dictionary<string, string> entryData = JsonConvert.DeserializeObject<Dictionary<string, string>>(entry.Metadata);
                // Obtener el nombre del jugador desde el metadata
                entryData.TryGetValue("Name", out string playerName);
                // Obtener el personaje elegido por cada jugador desde el metadata
                entryData.TryGetValue("Character", out string playerCharacter);

                // ACTUALIZACIÓN DE LA UI
                // Se actualiza la entrada de la tabla con la posición, el nombre y la puntuación del jugador
                _tableEntriesTexts[numEntry].text = (numEntry + 1).ToString() + "º - " + playerName + "  ->  " + entry.Score;
               
                numEntry++;
            }
        }
    }

}
