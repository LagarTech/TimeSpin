using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// Este script se encarga de las inversiones en la gravedad, que se gestionar�n por el servidor. La �nica funcionalidad que realizar� el cliente es actualizar el temporizador
public class GravityManager : MonoBehaviour
{
    public static GravityManager Instance;

    // Variable que controla el flujo del juego
    public bool runningGame = false;

    // Tiempo que se espera entre cambios de gravedad
    private const float _gravitySwitchTime = 9.5f;
    private float _gravityTimer; // Temporizador
    public bool isGravityInverted = false; // Controla si se ha invertido la gravedad o no

    // Gesti�n de que los jugadores floten
    private const float _gravity = 9.81f;
    public bool floating;
    private float _floatTimer; // Temporizador
    public float _floatTime; // Tiempo que tardan los jugadores en ir de una plataforma a otra
    // Referencias a las plataformas, para calcular el tiempo que se tarda en ir de una a otra
    [SerializeField] private Transform _topPlatform;
    [SerializeField] private Transform _bottomPlatform;

    // Temporizador del juego
    [SerializeField] private TMP_Text _timerText;
    private float _remainingTime = 120f; // El tiempo de juego son 2 minutos (120 segundos)
    private float _survivedTime = 0f;

    private int consecutiveGravityChanges = 0;//Cambio gravitatorios seguidos

    [SerializeField]
    private AudioSource _reproductor;
    [SerializeField]
    private AudioClip _clipAudio;
    [SerializeField]
    private AudioMixer mezclador;
    [SerializeField]
    private GameObject _optionsPanel;

    [SerializeField] private GameObject _exitButton;
    [SerializeField] private GameObject _leaveButton;
    [SerializeField] private GameObject _leaveAdvise;

    private bool logr01;
    private bool logr02;
    private bool logr03;
    private bool logr04;
    private bool logr05;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        // Se calcula el tiempo de flotaci�n utilizando la ecuaci�n del MRUA
        // d = d0 + v0*t + 1/2*a*t^2 -> Se parte del reposo -> d = 1/2*a*t^2
        // Despejando, se obtiene que t = sqrt(2*d/g), siendo d la distancia entre las plataformas y g la gravedad
        float distance = Vector3.Distance(_topPlatform.position, _bottomPlatform.position);
        _floatTime = Mathf.Sqrt(2 * distance / _gravity);

        ShowCorrectButton();
    }

    private void Update()
    {
        if (!runningGame) return;

        // GESTI�N DEL TIEMPO RESTANTE
        if (_remainingTime > 0f)
        {
            // Aumentar el tiempo de supervivencia
            _survivedTime += Time.deltaTime;
            // Disminuir el tiempo restante
            _remainingTime -= Time.deltaTime;
            // Se actualiza el temporizador
            UpdateTimer();
            
        }
        else
        {
            _remainingTime = 0f;
            // Se actualiza el temporizador
            UpdateTimer();
            // Se inicia el final del juego
            GameOver();     
            return;
        }

        // GESTI�N DE LA INVERSI�N DE LA GRAVEDAD
        _gravityTimer += Time.deltaTime;

        if (_gravityTimer >= _gravitySwitchTime)
        {
            // Se inicia el proceso de flotaci�n
            StartCoroutine(WarningGravityChange());
            _gravityTimer = 0; // Tambi�n se reinicia el temporizador
            MusicManager.PonerMusica(_clipAudio, _reproductor, false);
        }

        if(floating)
        {
            // Se comienza el contador
            _floatTimer += Time.deltaTime;
            if(_floatTimer >= _floatTime)
            {
                // Terminar la flotaci�n cuando se acabe el tiempo calculado
                StopFloating();
            }
        }

        if (_survivedTime >= 60f && !logr01)
        {
            AchievementManager.UnlockAchievement("Future_Ciudades");
            logr01 = true;
        }

    }

    private IEnumerator WarningGravityChange()
    {
        yield return GravityWarning.Instance.FlashEffect();
        StartFloating();
    }

    private void StartFloating()
    {
        // Los jugadores comienzan a flotar
        floating = true;
        _floatTimer = 0f;

        // Se invierte la gravedad
        if (isGravityInverted)
        {
            Physics.gravity = new Vector3(0, -_gravity, 0);  // Gravedad normal
        }
        else
        {
            Physics.gravity = new Vector3(0, _gravity, 0);   // Gravedad invertida
        }
        isGravityInverted = !isGravityInverted;

        // Logro: Alien�genas
        if(!logr02)
        {
            AchievementManager.UnlockAchievement("Future_Alien�genas");
            logr02 = true;
        }
       

        // Logro: Inteligencia Artificial
        consecutiveGravityChanges++;

        if (consecutiveGravityChanges >= 5 && !logr03)
        {
            AchievementManager.UnlockAchievement("Future_InteligenciaArtificialYRobots");
            logr03 = true;
        }
    }

    private void StopFloating()
    {
        floating = false;
        _floatTimer = 0f;
    }

    private void UpdateTimer()
    {
        // Calcular minutos y segundos
        int displayMinutes = Mathf.FloorToInt(_remainingTime / 60);
        int displaySeconds = Mathf.FloorToInt(_remainingTime % 60);

        // Actualizar el texto del TMP para que muestre el tiempo restante
        _timerText.text = string.Format("{0:00}:{1:00}", displayMinutes, displaySeconds);
    }

    public void GameOver()
    {
        // Se indica que el juego ha finalizado
        runningGame = false;
        // Se restaura la gravedad
        Physics.gravity = new Vector3(0, -9.81f, 0);

        //Logro: Tecnologia de Comunicacion
        if (_remainingTime <= 0 && !logr04)
        {
            AchievementManager.UnlockAchievement("Future_Tecnolog�aDeComunicaci�n");
            logr04 = true;
        }

        float maxScore = 120f; // Cambia esto seg�n tus reglas de puntuaci�n
        if (_survivedTime >= maxScore && !logr05)
        {
            AchievementManager.UnlockAchievement("Future_VidaEnOtrosPlanetas");
            logr05 = true;
        }

        // Se calcula la puntuaci�n del jugador en base al resultado
        GameSceneManager.instance.GameOverEgyptFuture(_survivedTime, false);
    }
    public void Options()
    {
        _optionsPanel.SetActive(false);
        runningGame = true;
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
            // Se muestra el bot�n de salir
            _exitButton.SetActive(true);
        }
        else
        {
            // Se muestra el bot�n de abandonar la partida junto con la advertencia
            _leaveAdvise.SetActive(true);
            _leaveButton.SetActive(true);
        }
    }

    public void ExitPracticeMode()
    {
        // Se indica que ha terminado el juego
        runningGame = false;
        // Se calcula la puntuaci�n del jugador en base a los resultados
        GameSceneManager.instance.GameOverEgyptFuture(_survivedTime, true);
    }

    public void ExitGame()
    {
        // Se indica que se ha terminado el juego
        runningGame = false;
        // Se resetea el estado inicial para comenzar una nueva partida
        GameSceneManager.instance.ResetState();
        // Se comienza la transici�n para volver al Lobby
        StartCoroutine(LoadingScreenManager.instance.FinalFade("LobbyMenu"));
    }
}
