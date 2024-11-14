using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance;

    [SerializeField] private List<TMP_Text> _minigamesScores; // Textos para mostrar las puntuaciones de cada minijuego
    [SerializeField] private TMP_Text _totalScore; // Texto para mostrar la puntuación total
    [SerializeField] private GameObject _rankingButton; // Botón para mostrar el ranking
    [SerializeField] private GameObject _rankingPanel; // Panel con el ranking

    [SerializeField] private List<GameObject> _rankingSprites; // Objetos con los sprites de los personajes en el ranking
    [SerializeField] private List<Sprite> _characterSprites; // Sprites de cada personaje
    [SerializeField] private List<TMP_Text> _rankingPositions; // Lista con los textos para poner el puesto de los personajes
    [SerializeField] private List<TMP_Text> _rankingEntries; // Lista con los textos para mostrar los nombres y las puntuaciones

    private const int NUM_GAMES = 5;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Se realiza el registro del usuario una sola vez
        if (AuthenticationService.Instance.IsSignedIn) return;
        StartCoroutine(InitializeUnityServicesCoroutine());
    }

    private IEnumerator InitializeUnityServicesCoroutine()
    {
        var initializationTask = UnityServices.InitializeAsync();

        // Esperar a que termine la inicialización
        yield return new WaitUntil(() => initializationTask.IsCompleted);

        if (initializationTask.IsFaulted)
        {
            Debug.LogError("Error al inicializar Unity Services: " + initializationTask.Exception);
            yield break;
        }

        // Autenticación del usuario
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            var signInTask = AuthenticationService.Instance.SignInAnonymouslyAsync();
            yield return new WaitUntil(() => signInTask.IsCompleted);

            if (signInTask.IsFaulted)
            {
                Debug.LogError("Error al autenticar al usuario: " + signInTask.Exception);
            }
            else
            {
                Debug.Log("Usuario autenticado exitosamente.");
            }
        }
    }

    public void ShowResults()
    {
        StartCoroutine(ShowMinigamesResults());
    }

    private IEnumerator ShowMinigamesResults()
    {
        for(int i = 0; i < NUM_GAMES; i++)
        {
            yield return StartCoroutine(AnimateNumber(_minigamesScores[i], GameSceneManager.instance.pointsGames[i]));
        }
        yield return AnimateNumber(_totalScore, GameSceneManager.instance.totalPoints);
        _rankingButton.SetActive(true);

    }

    private IEnumerator AnimateNumber(TMP_Text displayText, int targetNumber)
    {
        float currentNumber = 0f;
        float elapsedTime = 0f;
        float animationDuration = 1f;

        while (elapsedTime < animationDuration)
        {
            // Incrementa el tiempo transcurrido
            elapsedTime += Time.deltaTime;

            // Calcula el progreso de la animación (de 0 a 1)
            float progress = Mathf.Clamp01(elapsedTime / animationDuration);

            // Incrementa el número desde 0 hasta el objetivo usando Lerp
            currentNumber = Mathf.Lerp(0, targetNumber, progress);

            // Muestra el número en el Text UI (si se ha asignado uno)
            if (displayText != null)
            {
                displayText.text = currentNumber.ToString("F0"); // Formato sin decimales
            }

            yield return null; // Espera hasta el siguiente frame
        }

        // Asegúrate de que el número final sea exactamente el objetivo
        currentNumber = targetNumber;
        if (displayText != null)
        {
            displayText.text = currentNumber.ToString("F0");
        }
    }

    public void ShowRanking()
    {
        StartCoroutine(GetOnlineRanking());
    }

    private IEnumerator GetOnlineRanking()
    {
        // Enviar la puntuación al leaderboard
        var submitScoreTask = LeaderboardsService.Instance.AddPlayerScoreAsync("Time_Spin_Ranking", 
            GameSceneManager.instance.totalPoints, 
            new AddPlayerScoreOptions { Metadata = new Dictionary<string, string>() { 
                { "Name", SelectionController.instance.GetName()},
                { "Character", SelectionController.instance.GetCharacterSelected().ToString()} 
            } });
        
        yield return new WaitUntil(() => submitScoreTask.IsCompleted);

        if (submitScoreTask.IsFaulted)
        {
            Debug.LogError("Error al enviar la puntuación: " + submitScoreTask.Exception);
            yield break;
        }
        else
        {
            Debug.Log("Puntuación enviada con éxito.");
        }

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
                // Puesto en el ranking
                _rankingPositions[numEntry].text = (numEntry + 1).ToString() + "º";
                // Sprite del jugador, en función del personaje escogido
                // Al ser un string, se tiene que actuar con un switch
                int characterSprite = 0;
                switch(playerCharacter)
                {
                    case "0": characterSprite = 0; break;
                    case "1": characterSprite = 1; break;
                    case "2": characterSprite = 2; break;
                    case "3": characterSprite = 3; break;
                    case "4": characterSprite = 4; break;
                    case "5": characterSprite = 5; break;
                    case "6": characterSprite = 6; break;
                    case "7": characterSprite = 7; break;
                    case "8": characterSprite = 8; break;
                    case "9": characterSprite = 9; break;
                    case "10": characterSprite = 10; break;
                    case "11": characterSprite = 11; break;
                    case "12": characterSprite = 12; break;
                    case "13": characterSprite = 13; break;
                }
                _rankingSprites[numEntry].SetActive(true);
                // Se muestra el sprite correspondiente en base a la información de la entrada de la tabla
                _rankingSprites[numEntry].GetComponent<Image>().sprite = _characterSprites[characterSprite];
                // Por último, se actualiza la entrada de la tabla con el nombre y la puntuación del jugador
                _rankingEntries[numEntry].text = playerName + "    " + entry.Score;
            }
        }
        _rankingPanel.SetActive(true);
    }

    public void BackLobby()
    {
        // Se resetea el estado inicial para comenzar una nueva partida
        GameSceneManager.instance.ResetState();
        // Se comienza la transición para volver al Lobby
        StartCoroutine(LoadingScreenManager.instance.FinalFade("LobbyMenu"));
    }
}
