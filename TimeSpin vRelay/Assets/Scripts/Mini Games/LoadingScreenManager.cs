using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : NetworkBehaviour
{
    public static LoadingScreenManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator LoadingScreenCoroutine(string startedScene)
    {
        // Se busca la pantalla de carga
        GameSceneManager.instance.ActivePlayersList();
        CanvasGroup loadingScreen = GameObject.FindGameObjectWithTag("PantallaCarga").GetComponent<CanvasGroup>();

        // Fade in (aparecer)
        yield return FadeCanvasGroup(loadingScreen, 1f); // De 0 (invisible) a 1 (visible)
        yield return new WaitForSeconds(4f); // Espera 4 segundos
        // Fade out (desaparecer)
        yield return FadeCanvasGroup(loadingScreen, 0f); // De 1 (visible) a 0 (invisible)

        if (IsHost)
        {
            // Una vez hecho esto, en funcion de la escena a la que se ha transicionado, se activa el minijuego adecuado y se avisa a los clientes
            switch (startedScene)
            {
                case "Prehistory": StartPrehistoryClientRpc(); break;
                case "Egipt": StartEgiptClientRpc(); break;
                case "Medieval": StartMedievalClientRpc(); break;
                case "Maya": StartMayaClientRpc(); break;
                case "Future": StartFutureClientRpc(); break;
            }
        }
    }

    private IEnumerator ScoresPanelTransitionCoroutine(string sceneName)
    {
        // Se busca el objeto con la pantalla de puntuaciones para activarlo
        CanvasGroup scoresScreen = GameObject.FindGameObjectWithTag("PantallaPuntuaciones").GetComponent<CanvasGroup>();

        // Se obtienen los textos de las posiciones y de las puntuaciones, para poder actualizarlos en función de los resultados del minijuego
        GameObject[] positionTextGOs = GameObject.FindGameObjectsWithTag("Puesto");
        List<TMP_Text> _positionTexts = new List<TMP_Text>();
        foreach (var text in positionTextGOs)
        {
            _positionTexts.Add(text.GetComponent<TMP_Text>());
        }
        GameObject[] scoreTextGOs = GameObject.FindGameObjectsWithTag("Puntuacion");
        List<TMP_Text> _scoreTexts = new List<TMP_Text>();
        foreach (var text in scoreTextGOs)
        {
            _scoreTexts.Add(text.GetComponent<TMP_Text>());
        }
        // Se mostrarán los datos de los jugadores según el orden de clasificación
        for (int i = 0; i < GameSceneManager.instance.orderedPlayers.Count; i++)
        {
            _positionTexts[i].text = GameSceneManager.instance.orderedPlayers[i].currentPosition.ToString() + "º - " +
                GameSceneManager.instance.orderedPlayers[i].characterNamePlayer.GetComponentInChildren<TMP_Text>().text; // Se obtiene el nombre del jugador 
            int totalPoints = GameSceneManager.instance.orderedPlayers[i].currentPoints + GameSceneManager.instance.orderedPlayers[i].pointsToAdd; // Se calculan los puntos totales que tendrá el jugador tras sumar los del minijuego actual
            _scoreTexts[i].text = GameSceneManager.instance.orderedPlayers[i].currentPoints.ToString() + " + " + GameSceneManager.instance.orderedPlayers[i].pointsToAdd.ToString() + " = " + totalPoints.ToString();
            // Se actualizan finalmente las puntuaciones del jugador, después de mostrar el estado anterior por pantalla
            GameSceneManager.instance.orderedPlayers[i].currentPoints = totalPoints;
            GameSceneManager.instance.orderedPlayers[i].pointsToAdd = 0;
        }
     
        // Fade in (aparecer)
        yield return FadeCanvasGroup(scoresScreen, 1f); // De 0 (invisible) a 1 (visible)
        yield return new WaitForSeconds(10f); // Espera 10 segundos mostrando las puntuaciones
        yield return FinalFade(sceneName);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration = 0.5f)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        // Se desaparece la pantalla negra inicial
        Image background = GameObject.FindGameObjectWithTag("Fundido").GetComponent<Image>();
        Color backgroundColor = background.color;
        backgroundColor.a = 0f;
        background.color = backgroundColor;
    }

    public IEnumerator FinalFade(string sceneName)
    {
        Image background = GameObject.FindGameObjectWithTag("Fundido").GetComponent<Image>();

        float elapsed = 0f;
        float duration = 0.5f;
        Color imageColor = background.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(0f, 1f, elapsed / duration);

            // Actualiza la opacidad de la imagen
            imageColor.a = newAlpha;
            background.color = imageColor;

            yield return null;
        }

        if (IsHost)
        {
            // Carga la siguiente escena
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    public void ServerSceneTransition(string sceneName)
    {
        // Avisa a los clientes para que realicen la transición a la escena adecuada
        SceneTransitionClientRpc(sceneName);
        // Si se ha terminado un minijuego, se irá a la escena del Lobby
        if (sceneName == "LobbyMenu")
        {
            // Actualiza las puntuaciones de los jugadores
            foreach (var player in GameSceneManager.instance.orderedPlayers)
            {
                player.currentPoints += player.pointsToAdd;
                player.pointsToAdd = 0;
            }
        }
    }

    [ClientRpc]
    public void SceneTransitionClientRpc(string sceneName)
    {
        Debug.Log("Realizando transición cliente");
        if (SceneManager.GetActiveScene().name == "LobbyMenu")
        {
            // Se hace el fundido hacia un minijuego desde el lobby
            StartCoroutine(FinalFade(sceneName));
        }
        else
        {
            // Se inicia el proceso de mostrar las puntuaciones y el fundido a la pantalla de carga
            StartCoroutine(ScoresPanelTransitionCoroutine(sceneName));
        }
    }

    [ClientRpc]
    private void StartPrehistoryClientRpc() { }

    [ClientRpc]
    private void StartEgiptClientRpc() { GridManager.Instance.runningGame = true; }

    [ClientRpc]
    private void StartMedievalClientRpc() { }

    [ClientRpc]
    private void StartMayaClientRpc() { RaceManager.instance.runningGame = true; }

    [ClientRpc]
    private void StartFutureClientRpc() { GravityManager.Instance.runningGame = true; }

}
