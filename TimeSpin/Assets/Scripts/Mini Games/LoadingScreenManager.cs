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
        // Se busca la pantalla de carga y puntuaciones en la escena, solo en los clientes
        if (Application.platform != RuntimePlatform.LinuxServer)
        {
            GameSceneManager.instance.ActivePlayersList();
            GameObject loadingScreen = GameObject.FindGameObjectWithTag("PantallaCarga");
            // Se activa el objeto con la pantalla de carga
            foreach (Transform child in loadingScreen.transform)
            {
                child.gameObject.SetActive(true);
            }

            // Se gestionan los fundidos de la imagen y los textos
            // Se obtiene el fondo
            Image background = loadingScreen.GetComponentInChildren<Image>();
            // Se obtienen los dos textos de la pantalla
            List<TMP_Text> screenText = new List<TMP_Text>();
            TMP_Text loading = GameObject.FindGameObjectWithTag("Cargando").GetComponent<TMP_Text>();
            screenText.Add(loading);
            TMP_Text loadingText = GameObject.FindGameObjectWithTag("TextoCarga").GetComponent<TMP_Text>(); // Este texto después se podrá modificar en cada ocasión
            screenText.Add(loadingText);

            // Fade in (aparecer)
            yield return Fade(background, screenText, 1f, 0f, 1f); // De 0 (invisible) a 1 (visible)
            yield return new WaitForSeconds(4f); // Espera 4 segundos
            // Fade out (desaparecer)
            yield return Fade(background, screenText, 1f, 1f, 0f); // De 1 (visible) a 0 (invisible)
            // Se desactiva el objeto con la pantalla de carga
            foreach (Transform child in loadingScreen.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
        else
        {
            // La logica de espera se ejecuta en el servidor
            // Se esperan 5 segundos, para dar tiempo a la colocacion de cada escena
            yield return new WaitForSeconds(5f);
            // Una vez hecho esto, en funcion de la escena a la que se ha transicionado, se activa el minijuego adecuado y se avisa a los clientes
            switch (startedScene)
            {
                case "LobbyMenu":
                    StartLobbyClientRpc();
                    break;
                case "Prehistory":
                    StartPrehistoryClientRpc();
                    break;
                case "Egipt":
                    GridManager.Instance.runningGame = true; // Se inicia la logica del juego en el servidor
                    StartEgiptClientRpc();
                    break;
                case "Medieval":
                    StartMedievalClientRpc();
                    break;
                case "Maya":
                    RaceManager.instance.runningGame = true; // Se inicia la logica del juego en el servidor
                    StartMayaClientRpc();
                    break;
                case "Future":
                    GravityManager.Instance.runningGame = true; // Se inicia la logica del juego en el servidor
                    StartFutureClientRpc();
                    break;
            }
        }
    }

    private IEnumerator ScoresPanelTransitionCoroutine()
    {
        // Se busca el objeto con la pantalla de carga para activarlo
        GameObject loadingScreen = GameObject.FindGameObjectWithTag("PantallaPuntuaciones");
        // Se activa el objeto con la pantalla de carga
        foreach (Transform child in loadingScreen.transform)
        {
            child.gameObject.SetActive(true);
        }
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
        // Una vez actualizados los textos, se gestiona su aparición fundida
        List<TMP_Text> _scorePanelTexts = new List<TMP_Text>();
        // Se añaden los títulos
        GameObject[] titleTextGO = GameObject.FindGameObjectsWithTag("TituloPuntuacion");
        foreach (var title in titleTextGO)
        {
            _scorePanelTexts.Add(title.GetComponent<TMP_Text>());
        }
        // Se añaden los textos de las puntuaciones y las posiciones
        foreach (var text in _positionTexts)
        {
            _scorePanelTexts.Add(text);
        }
        foreach (var text in _scoreTexts)
        {
            _scorePanelTexts.Add(text);
        }
        // Se obtiene el fondo
        Image background = GameObject.FindGameObjectWithTag("FondoPuntuaciones").GetComponent<Image>();
        // Fade in (aparecer)
        yield return Fade(background, _scorePanelTexts, 1f, 0f, 1f); // De 0 (invisible) a 1 (visible)
        yield return new WaitForSeconds(10f); // Espera 10 segundos mostrando las puntuaciones
        yield return FinalFade();
    }

    private IEnumerator Fade(Image image, List<TMP_Text> texts, float startAlphaImage, float startAlphaTexts, float endAlpha, float duration = 0.5f)
    {
        float elapsed = 0f;

        // Establece el valor inicial de opacidad para la imagen y los textos
        Color imageColor = image.color;
        imageColor.a = startAlphaImage;
        image.color = imageColor;

        foreach (var text in texts)
        {
            Color textColor = text.color;
            textColor.a = startAlphaTexts;
            text.color = textColor;
        }

        // Realiza la interpolación de opacidad
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlphaImage = Mathf.Lerp(startAlphaImage, endAlpha, elapsed / duration);

            // Actualiza la opacidad de la imagen
            imageColor.a = newAlphaImage;
            image.color = imageColor;

            // Actualiza la opacidad de cada texto
            float newAlphaText = Mathf.Lerp(startAlphaTexts, endAlpha, elapsed / duration);
            foreach (var text in texts)
            {
                Color textColor = text.color;
                textColor.a = newAlphaText;
                text.color = textColor;
            }

            yield return null;
        }

        // Asegura el valor final de opacidad para la imagen y los textos
        imageColor.a = endAlpha;
        image.color = imageColor;

        foreach (var text in texts)
        {
            Color textColor = text.color;
            textColor.a = endAlpha;
            text.color = textColor;
        }
    }

    public IEnumerator FinalFade()
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
    }

    public IEnumerator ServerSceneTransition(string sceneName)
    {
        // Avisa a los clientes para que realicen la transición
        SceneTransitionClientRpc();
        // Si se encuentra en la escena de menú, sólo se hace el fundido
        if (SceneManager.GetActiveScene().name == "LobbyMenu")
        {
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            // Si se trata de algún minijuego, se realiza completo
            // Actualiza las puntuaciones de los jugadores
            foreach (var player in GameSceneManager.instance.orderedPlayers)
            {
                player.currentPoints += player.pointsToAdd;
                player.pointsToAdd = 0;
            }
            // Espera 0.5 segundos para que se realice el fundido en los clientes, después de esperar 11 segundos para que se muestren las puntuaciones y permanezcan 10 segundos 
            yield return new WaitForSeconds(11f);
        }
        // Carga la siguiente escena
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    [ClientRpc]
    public void SceneTransitionClientRpc()
    {
        if (SceneManager.GetActiveScene().name == "LobbyMenu")
        {
            // Se hace el fundido hacia un minijuego desde el lobby
            StartCoroutine(FinalFade());
        }
        else
        {
            // Se inicia el proceso de mostrar las puntuaciones y el fundido a la pantalla de carga
            StartCoroutine(ScoresPanelTransitionCoroutine());
        }
    }


    [ClientRpc]
    private void StartLobbyClientRpc()
    {

    }

    [ClientRpc]
    private void StartPrehistoryClientRpc()
    {

    }

    [ClientRpc]
    private void StartEgiptClientRpc()
    {
        // Se activa el minijuego
        GridManager.Instance.runningGame = true;
    }

    [ClientRpc]
    private void StartMedievalClientRpc()
    {
    }

    [ClientRpc]
    private void StartMayaClientRpc()
    {
        // Se activa el miniijuego
        RaceManager.instance.runningGame = true;
    }

    [ClientRpc]
    private void StartFutureClientRpc()
    {
        // Se activa el minijuego
        GravityManager.Instance.runningGame = true;
    }

}
