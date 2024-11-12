using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance;

    [SerializeField] private List<TMP_Text> _minigamesScores; // Textos para mostrar las puntuaciones de cada minijuego
    [SerializeField] private TMP_Text _totalScore; // Texto para mostrar la puntuación total
    [SerializeField] private GameObject _rankingButton; // Botón para mostrar el ranking
    [SerializeField] private GameObject _rankingPanel; // Panel con el ranking

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
        var submitScoreTask = LeaderboardsService.Instance.AddPlayerScoreAsync("Time_Spin_Ranking", GameSceneManager.instance.totalPoints);
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
        var loadScoresTask = LeaderboardsService.Instance.GetScoresAsync("Time_Spin_Ranking", new GetScoresOptions { Limit = 10 });
        yield return new WaitUntil(() => loadScoresTask.IsCompleted);

        if (loadScoresTask.IsFaulted)
        {
            Debug.LogError("Error al cargar las puntuaciones: " + loadScoresTask.Exception);
        }
        else
        {
            Debug.Log("Puntuaciones cargadas con éxito:");
            foreach (var entry in loadScoresTask.Result.Results)
            {
                Debug.Log($"Jugador: {entry.PlayerId}, Puntuación: {entry.Score}");
                // Aquí podrías actualizar la UI para mostrar cada entrada
            }
        }
        _rankingPanel.SetActive(true);
    }

    public void BackLobby()
    {
        // Se comienza la transición para volver al Lobby
        StartCoroutine(LoadingScreenManager.instance.FinalFade("LobbyMenu"));
    }
}
