using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager instance;

    // Variable que indica si es la primera vez que se entra en el jueg
    public bool initiatedGame = false;
    public bool gameStarted = false;
    public bool practiceStarted = false;
    public bool gameFinished = false;

    private const int NUM_GAMES = 5;
    [SerializeField] private bool[] _playedGames = new bool[NUM_GAMES]; // Lista en la que se va a marcar los minijuegos que se van jugando, para que no se puedan repetir
    private int _numPlayedGames = 0; // Contador que maneja el n�mero de minijuegos que ya se han jugado
    public bool allGamesPlayed = false; // Variable que gestiona cu�ndo termina la partida, al jugar los 5 minijuegos
    // C�lculo de puntuaciones de los minijuegos Egipto y Futuro
    const float MAX_SURVIVED_TIME = 120f;
    const int MAX_POINTS_EG_FT = 50;
    // C�lculo de puntuaciones del minijuego Maya
    const float MIN_RACE_TIME = 30f;
    const float MAX_RACE_TIME = 80f;
    const int MAX_POINTS_MY = 50;
    // Se almacenan los resultados de los juegos y los puntos asociados para mostrarlos al final
    [SerializeField] private int[] _resultsGames = new int[NUM_GAMES];
    public int[] pointsGames = new int[NUM_GAMES];

    public int totalPoints = 0;
    private int highScore = 0; // Puntuaci�n m�xima por defecto

    // Gesti�n de las escenas
    // Control de la escena en la que se encuentra el jugador

    private enum Scene
    {
        Lobby,
        Prehistory,
        Egypt,
        Medieval,
        Maya,
        Future
    }

    private string _previousScene = ""; // Se guarda una referencia a la escena en el instante anterior, para comprobar si se ha llevado a cabo alg�n cambio

    public GameObject playerPrefab;
    public Vector3 startingPositionLobby;
    [SerializeField] private Vector3 _startingPositionPrehistory;
    [SerializeField] private Vector3 _startingPositionEgypt;
    [SerializeField] private Vector3 _startingPositionMedieval;
    [SerializeField] private Vector3 _startingPositionMaya;
    [SerializeField] private Vector3 _startingPositionFuture;

    // Conservaci�n de los par�metros de configuraci�n
    public float MusicVolume;
    public float EffectsVolume;
    public float Brigthness = 1f;

    public bool initializedServices = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        // Se hace que el objeto navegue entre escenas y no se destruya
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        GetCurrentScene();
    }

    private void GetCurrentScene()
    {
        // Se obtiene el nombre de la escena
        string sceneName = SceneManager.GetActiveScene().name;
        // Si se sigue en la misma escena que antes, no se actualiza nada
        if (_previousScene == sceneName) return;
        // Gesti�n de las acciones necesarias para pasar de una escena a otra
        switch (sceneName)
        {
            case "LobbyMenu":
                // Se restaura la gravedad
                Physics.gravity = new Vector3(0, -9.81f, 0);
                // Se instancia al jugador en la posici�n de inicio adecuada
                if (gameStarted && !practiceStarted)
                {
                    Instantiate(playerPrefab, startingPositionLobby, Quaternion.identity);
                    GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>()._speed = 3f;
                }
                practiceStarted = false; // Se indica que no ya se ha terminado el minijuego de pr�ctica
                // Se eliminan las momias del minijuego anterior
                DespawnMummies();
                // Se eliminan los troncos del minijuego anterior
                DespawnTrunks();
                // Se apaga la l�mpara
                GameObject luz = GameObject.FindGameObjectWithTag("LuzFarol");
                if (luz != null) luz.GetComponent<Light>().intensity = 0;
                break;
            case "Prehistory":
                // Una vez se cambia de la primera escena, se indica que ya ha comenzado el juego
                gameStarted = true;
                // Se instancia al jugador en la posici�n de inicio adecuada
                Instantiate(playerPrefab, _startingPositionPrehistory, Quaternion.identity);
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>()._speed = 4f;
                break;
            case "Egypt":
                // Una vez se cambia de la primera escena, se indica que ya ha comenzado el juego
                gameStarted = true;
                // Se instancia al jugador en la posici�n de inicio adecuada con la escala adecuada
                Instantiate(playerPrefab, _startingPositionEgypt, Quaternion.identity).transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>()._speed = 2f;
                // Se activa la l�mpara
                Light farol = GameObject.FindGameObjectWithTag("LuzFarol").GetComponent<Light>();
                farol.intensity = 15;
                break;
            case "Medieval":
                // Una vez se cambia de la primera escena, se indica que ya ha comenzado el juego
                gameStarted = true;
                // Se instancia al jugador en la posici�n de inicio adecuada
                Instantiate(playerPrefab, _startingPositionMedieval, Quaternion.identity);
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>()._speed = 5f;
                break;
            case "Maya":
                // Una vez se cambia de la primera escena, se indica que ya ha comenzado el juego
                gameStarted = true;
                // Se instancia al jugador en la posici�n de inicio adecuada
                Transform _playerTransform = Instantiate(playerPrefab, _startingPositionMaya, Quaternion.identity).transform;
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>()._speed = 2f;
                // Se modifica la escala del personaje
                _playerTransform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                // Se hace que si el personaje es del propietario, la c�mara lo siga
                GameObject.FindGameObjectWithTag("FollowCamera").GetComponent<CinemachineVirtualCamera>().Follow = _playerTransform;

                break;
            case "Future":
                // Una vez se cambia de la primera escena, se indica que ya ha comenzado el juego
                gameStarted = true;
                // Se instancia al jugador en la posici�n de inicio adecuada
                Instantiate(playerPrefab, _startingPositionFuture, Quaternion.identity);
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>()._speed = 2f;
                break;
        }
        if (_previousScene != "")
        {
            // Se inicia la pantalla de carga en la escena a la que se haya transicionado
            StartCoroutine(LoadingScreenManager.instance.LoadingScreenCoroutine(sceneName));
        }
        _previousScene = sceneName;
    }

    private void DespawnMummies()
    {
        // Encontrar todos los objetos con la etiqueta "Momia"
        GameObject[] mummies = GameObject.FindGameObjectsWithTag("Momia");
        if (mummies.Length == 0) return;
        // Recorrer cada objeto y destruirlo
        foreach (GameObject mummy in mummies)
        {
            Destroy(mummy);
        }
    }

    private void DespawnTrunks()
    {
        // Encontrar todos los objetos con la etiqueta "Tronco"
        GameObject[] trunks = GameObject.FindGameObjectsWithTag("Tronco");
        if (trunks.Length == 0) return;
        // Recorrer cada objeto y despawnearlo si es un NetworkObject
        foreach (GameObject trunk in trunks)
        {
            Destroy(trunk);
        }
    }

    public void RegisterGameSelection(int gameID)
    {
        _playedGames[gameID] = true; // Se marca el juego como ya escogido
        _numPlayedGames++; // Se incrementa el n�mero de minijuegos jugados
        if (_numPlayedGames == NUM_GAMES)
        {
            allGamesPlayed = true; // Si se han jugado todos, se indica para poder terminar el juego
        }
    }

    public bool[] GetPlayedGames() { return _playedGames; }

    // GESTI�N DE LAS PUNTUACIONES DE LOS MINIJUEGOS
    public void GameOverEgyptFuture(float survivedTime, bool egypt)
    {
        // La puntuaci�n s�lo se calcula cuando no se est� en el modo de pr�ctica
        if (!practiceStarted)
        {
            // La puntuaci�n m�xima para estos minijuegos es de 50 puntos, si se aguantan los dos minutos
            // Se aplica la relaci�n de proporcionalidad
            int resultPoints = (int)(MAX_POINTS_EG_FT * survivedTime / MAX_SURVIVED_TIME);
            totalPoints += resultPoints;
            // Se almacena el resultado en una lista
            bool isRecord = false;

            if (egypt)
            {
                _resultsGames[1] = (int)survivedTime;
                pointsGames[1] = resultPoints;
                // Se comprueba si es record
                if (!practiceStarted)
                {
                    int recordPoints = PlayerPrefs.GetInt("Egypt");
                    if (resultPoints > recordPoints)
                    {
                        PlayerPrefs.SetInt("Egypt", resultPoints);
                        isRecord = true;
                    }
                }
            }
            else
            {
                _resultsGames[4] = (int)survivedTime;
                pointsGames[4] = resultPoints;
                // Se comprueba si es record
                if (!practiceStarted)
                {
                    int recordPoints = PlayerPrefs.GetInt("Future");
                    if (resultPoints > recordPoints)
                    {
                        PlayerPrefs.SetInt("Future", resultPoints);
                        isRecord = true;
                    }
                }

            }
            // Se pasa dicha informaci�n a la pantalla de puntuaciones para mostrarlo
            LoadingScreenManager.instance.SceneToLobbyTransition("LobbyMenu", (int)survivedTime, resultPoints, isRecord);
        }
        else
        {
            LoadingScreenManager.instance.SceneToLobbyTransition("LobbyMenu", 0, 0, false);
        }
    }

    public void GameOverMaya(float raceTime)
    {
        if (!practiceStarted)
        {
            // La puntuaci�n m�xima para estos minijuegos es de 50 puntos, si se tardan 50 segundos o menos
            int resultPoints = 0;
            if (raceTime <= MIN_RACE_TIME)
            {
                resultPoints = MAX_POINTS_MY;
            }
            else if (raceTime > MIN_RACE_TIME && raceTime <= MAX_RACE_TIME)
            {
                resultPoints = (int)(MAX_RACE_TIME - raceTime);
            }
            else
            {
                resultPoints = 0;
            }
            // Se suman los puntos acumulados
            totalPoints += resultPoints;


            // Se comprueba si es record
            bool isRecord = false;
            // Se comprueba si es record
            if (!practiceStarted)
            {
                int recordPoints = PlayerPrefs.GetInt("Maya");
                if (resultPoints > recordPoints)
                {
                    PlayerPrefs.SetInt("Maya", resultPoints);
                    isRecord = true;
                }
            }

            // Se almacena el resultado en la lista
            _resultsGames[3] = (int)raceTime;
            pointsGames[3] = resultPoints;

            // Se pasa dicha informaci�n a la pantalla de puntuaciones para mostrarlo
            LoadingScreenManager.instance.SceneToLobbyTransition("LobbyMenu", (int)raceTime, resultPoints, isRecord);
        }
        else
        {
            LoadingScreenManager.instance.SceneToLobbyTransition("LobbyMenu", 0, 0, false);
        }
    }

    public void GameOverPrehistoryMedieval(int points, int numDefeated, bool prehistory)
    {
        if (!practiceStarted)
        {
            // Como resultado, se tomar� el n�mero de espadas cogidas y el n�mero de dinosaurios derrotados
            bool isRecord = false;

            // La puntuaci�n de estos minijuegos viene determinada por la propia partida
            int resultPoints = points;
            totalPoints += resultPoints; // Se suman los puntos al total


            if (prehistory)
            {
                _resultsGames[0] = numDefeated;
                pointsGames[0] = resultPoints;
                // Se comprueba si es record
                if (!practiceStarted)
                {
                    int recordPoints = PlayerPrefs.GetInt("Prehistory");
                    if (resultPoints > recordPoints)
                    {
                        PlayerPrefs.SetInt("Prehistory", resultPoints);
                        isRecord = true;
                    }
                }
            }
            else
            {
                _resultsGames[2] = numDefeated;
                pointsGames[2] = resultPoints;
                // Se comprueba si es record
                if (!practiceStarted)
                {
                    int recordPoints = PlayerPrefs.GetInt("Medieval");
                    if (resultPoints > recordPoints)
                    {
                        PlayerPrefs.SetInt("Medieval", resultPoints);
                        isRecord = true;
                    }
                }
            }

            // Se pasa dicha informaci�n a la pantalla de puntuaciones para mostrarlo
            LoadingScreenManager.instance.SceneToLobbyTransition("LobbyMenu", numDefeated, resultPoints, isRecord);
        }
        else
        {
            LoadingScreenManager.instance.SceneToLobbyTransition("LobbyMenu", 0, 0, false);
        }
    }

    private void OnApplicationQuit()
    {
        // Guardar el r�cord en PlayerPrefs
        PlayerPrefs.SetInt("HighScore", highScore);
    }

    public void ResetState()
    {
        // Se indica que no se ha jugado ning�n minijuego y se resetean los resultados y puntuaciones
        allGamesPlayed = false;
        _numPlayedGames = 0;
        for (int i = 0; i < NUM_GAMES; i++)
        {
            _playedGames[i] = false;
            _resultsGames[i] = 0;
            pointsGames[i] = 0;
        }
        // Se resetea la puntuaci�n total
        totalPoints = 0;
        // Se indica que ha terminado la partida
        gameFinished = true;
        gameStarted = false;
    }

    public void LeaveGame()
    {
        // Se indica que no se ha jugado ning�n minijuego y se resetean los resultados y puntuaciones
        allGamesPlayed = false;
        _numPlayedGames = 0;
        for (int i = 0; i < NUM_GAMES; i++)
        {
            _playedGames[i] = false;
            _resultsGames[i] = 0;
            pointsGames[i] = 0;
        }
        // Se resetea la puntuaci�n total
        totalPoints = 0;

        RestartMenu();

        gameStarted = false;
        // Se elimina al jugador
        Destroy(GameObject.FindGameObjectWithTag("Player"));
    }

    public void RestartMenu()
    {
        // Se resetean los botones
        StartingManager.instance.UnlockButtons();
        // Se reinicia el nobmre
        SelectionController.instance.ModifyName("");
        // Se reactiva el men�
        UI_Controller.instance.Menu.SetActive(true);
        // Se reactiva el nombre
        UI_Controller.instance.Nombre.SetActive(true);
        UI_Controller.instance.Nombre.GetComponent<InputField>().text = string.Empty;
        CharacterSelectionController.instance.skinName.text = "LAGAR BOY";
    }


}
