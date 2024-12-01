using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
{
    public static RaceManager instance;

    public bool runningGame = false;
    private float _timer = 0f;
    [SerializeField]
    private GameObject _optionsPanel;

    [SerializeField] private GameObject _exitButton;
    [SerializeField] private GameObject _leaveButton;
    [SerializeField] private GameObject _leaveAdvise;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        ShowCorrectButton();
    }

    private void Update()
    {
        if (runningGame)
        {
            // Se contabiliza el tiempo que se tarda en terminar la carrera
            _timer += Time.deltaTime;
        }
    }

    // Función para terminar el minijuego
    private void EndRace()
    {
        if (!runningGame) return;
        runningGame = false;

        // Completar la carrera y verificar logros
        AchievementsManager.instance.CompleteRace(_timer);

        // Se inicia la transición
        GameSceneManager.instance.GameOverMaya(_timer);
    }

    public void Options()
    {
        _optionsPanel.SetActive(false);
        runningGame = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            EndRace();
        }
    }

    public void ShowOptions()
    {
        _optionsPanel.SetActive(true);
        runningGame = false;
    }

    private void ShowCorrectButton()
    {
        if (GameSceneManager.instance.practiceStarted)
        {
            // Se muestra el botón de salir
            _exitButton.SetActive(true);
        }
        else
        {
            // Se muestra el botón de abandonar la partida junto con la advertencia
            _leaveAdvise.SetActive(true);
            _leaveButton.SetActive(true);
        }
    }

    public void ExitPracticeMode()
    {
        // Se indica que ha terminado el juego
        runningGame = false;
        // Se calcula la puntuación del jugador en base a los resultados
        GameSceneManager.instance.GameOverMaya(_timer);
    }

    public void ExitGame()
    {
        // Se indica que se ha terminado el juego
        runningGame = false;
        // Se resetea el estado inicial para comenzar una nueva partida
        GameSceneManager.instance.ResetState();
        // Se comienza la transición para volver al Lobby
        StartCoroutine(LoadingScreenManager.instance.FinalFade("LobbyMenu"));
    }
}
