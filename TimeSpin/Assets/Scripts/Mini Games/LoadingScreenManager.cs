using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
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
        CanvasGroup loadingScreen = GameObject.FindGameObjectWithTag("PantallaCarga").GetComponent<CanvasGroup>();

        // Fade in (aparecer)
        yield return FadeCanvasGroup(loadingScreen, 1f, true); // De 0 (invisible) a 1 (visible)
        yield return new WaitForSeconds(4f); // Espera 4 segundos
        // Fade out (desaparecer)
        yield return FadeCanvasGroup(loadingScreen, 0f, true); // De 1 (visible) a 0 (invisible)

        if (startedScene == "") yield break; // En caso de que no se transicione a ninguna escena, no se continúa ejecutando

        // Una vez hecho esto, en funcion de la escena a la que se ha transicionado, se activa el minijuego adecuado
        switch (startedScene)
        {
            case "Prehistory": PrehistoryManager.Instance.runningGame = true; break;
            case "Egypt": GridManager.Instance.runningGame = true; break;
            case "Medieval": MedievalGameManager.Instance.runningGame = true;  break;
            case "Maya": RaceManager.instance.runningGame = true; break;
            case "Future": GravityManager.Instance.runningGame = true; break;
        }

        // Si se ha ido al lobby y se han jugado los 5 minijuegos, se pasa a la pantalla final de puntuaciones
        if (startedScene == "LobbyMenu" && GameSceneManager.instance.allGamesPlayed)
        {
            // Pantalla final de puntuaciones totales
        }
    }

    private IEnumerator ScoresPanelTransitionCoroutine(string sceneName, int result, int points, bool isRecord)
    {
        // Se busca el objeto con la pantalla de puntuaciones para activarlo
        CanvasGroup scoresScreen = GameObject.FindGameObjectWithTag("PantallaPuntuaciones").GetComponent<CanvasGroup>();

        // Se coloca la información del minijuego en la pantalla
        // Resultado (en unidades correctas)
        TMP_Text resultText = GameObject.FindGameObjectWithTag("Resultado").GetComponent<TMP_Text>();
        resultText.text = result.ToString();
        // Puntos
        TMP_Text pointsText = GameObject.FindGameObjectWithTag("Puntuacion").GetComponent<TMP_Text>();
        pointsText.text = points.ToString();
        // Se indica si es record

        // Se obtiene el texto y el total de puntos para hacer la animación
        int targetPoints = GameSceneManager.instance.totalPoints;
        TMP_Text currentPointsText = GameObject.FindGameObjectWithTag("PuntuacionActual").GetComponent<TMP_Text>();

        // Fade in (aparecer)
        yield return FadeCanvasGroup(scoresScreen, 1f, true); // De 0 (invisible) a 1 (visible)
        // Animación de la puntuación
        yield return AnimateNumber(currentPointsText, targetPoints);
        yield return new WaitForSeconds(5f); // Espera 5 segundos mostrando las puntuaciones
        yield return FinalFade(sceneName);
    }

    public IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, bool endScene, float duration = 0.5f)
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
        if (endScene)
        {
            Image background = GameObject.FindGameObjectWithTag("Fundido").GetComponent<Image>();
            Color backgroundColor = background.color;
            backgroundColor.a = 0f;
            background.color = backgroundColor;
        }
        else if (!endScene && targetAlpha == 0f)
        {
            canvasGroup.gameObject.SetActive(false);
        }
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
        // Carga la siguiente escena
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

    }

    public void SceneToLobbyTransition(string sceneName, int result, int points, bool isRecord)
    {
        // Si se está en un minijuego y se va a la escena del lobby, quiere decir que hay que sumar las puntuaciones
        // Se inicia el proceso de mostrar las puntuaciones y el fundido a la pantalla de carga
        StartCoroutine(ScoresPanelTransitionCoroutine(sceneName, result, points, isRecord));
    }

    public void SceneToGameTransition(string sceneName)
    {
        // Se hace el fundido a negro, dentro del lobby o yendo hacia un juego
        StartCoroutine(FinalFade(sceneName));
    }

}
