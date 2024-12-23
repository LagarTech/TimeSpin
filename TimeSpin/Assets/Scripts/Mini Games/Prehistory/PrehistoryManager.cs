using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PrehistoryManager : MonoBehaviour
{
    public static PrehistoryManager Instance;

    [SerializeField] Transform[] _holes; // Posiciones de los agujeros
    private const float SPAWN_INTERVAL = 3f; // Intervalo de aparici�n
    private const float GAME_TIME = 60f; // Duraci�n del juego

    private float _timeLeft;
    private float _spawnTimer;

    public bool runningGame = false;

    [SerializeField] private TMP_Text timeText; // UI del tiempo
    [SerializeField] private TMP_Text scoreText; // UI de la puntuaci�n
    private int _score;
    private int _hitDinosaurs = 0;

    private HashSet<int> _occupiedHoles = new HashSet<int>(); // Conjunto que almacena los agujeros ocupados por dinosaurios

    //Logros
    public int _velociraptorHits = 0;
    public bool _tRexDefeated = false;
    public HashSet<int> _holesHit = new HashSet<int>(); // Agujeros golpeados
    public float _lastHitTime = -5f; // Tiempo del �ltimo golpe para "Racha r�pida"
    public int _consecutiveHits = 0; // Golpes consecutivos en corto tiempo

    private List<string> _recentlyUnlockedAchievements = new List<string>(); //lista de logros desbloqueados recientemente

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
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _timeLeft = GAME_TIME;
        UpdateUI();
        ShowCorrectButton();
    }

    void Update()
    {
        // Si el juego no ha comenzado, no se ejecuta nada
        if (!runningGame) return;

        // Temporizador
        if (_timeLeft > 0)
        {
            _timeLeft -= Time.deltaTime;
            _spawnTimer += Time.deltaTime;

            if (_spawnTimer >= SPAWN_INTERVAL)
            {
                SpawnRandomDinosaur();
                _spawnTimer = 0f;
            }
            UpdateUI();
        }
        else
        {
            _timeLeft = 0f;
            UpdateUI();
            // Se finaaliza el juego
            EndGame();
        }
    }

    void SpawnRandomDinosaur()
    {
        int holeIndex;
        // Elegir un agujero aleatorio que no est� ocupado
        do
        {
            holeIndex = Random.Range(0, _holes.Length);
        } while (_occupiedHoles.Contains(holeIndex));

        // Obtener un dinosaurio del pool
        GameObject dinosaur = DinosaurPool.Instance.GetDinosaur();

        if (dinosaur != null) // Asegurarse de que hay dinosaurios disponibles en el pool
        {
            // Colocar el dinosaurio en el agujero y marcarlo como ocupado
            dinosaur.transform.position = _holes[holeIndex].position - new Vector3(0, 0.5f, 0);
            // Animaci�n del dinosaurio 
            StartCoroutine(dinosaur.GetComponent<DinosaurController>().AppearAnimation());
            dinosaur.GetComponent<DinosaurController>().holeIndex = holeIndex; // Se indica qu� agujero ocupa el dinosaurio
            _occupiedHoles.Add(holeIndex);
        }
    }

    // M�todo para liberar el agujero cuando el dinosaurio desaparece
    public void ReleaseHole(int holeIndex)
    {
        _occupiedHoles.Remove(holeIndex);
    }

    public void AddScore(int points)
    {
        _score += points;
        _hitDinosaurs++;

        float currentTime = Time.time;

        // Logro: Primer golpe
        if (_hitDinosaurs == 1 && !logr01)
        {
            AchievementManager.UnlockAchievement("Prehistory_PrimerosInstrumentosMusicales");
            logr01 = true;
        }

        // Logro: Racha r�pida
        if (currentTime - _lastHitTime <= 10f && !logr02)
        {
            _consecutiveHits++;
            if (_consecutiveHits >= 3)
            {
                AchievementManager.UnlockAchievement("Prehistory_PinturasRupestres");
                logr02 = true;
            }
        }
        else
        {
            _consecutiveHits = 1; // Reinicia la racha si no golpea a tiempo
        }

        _lastHitTime = currentTime;

        // Logro: Cazador experto
        if (_hitDinosaurs >= 10 && !logr03)
        {
            AchievementManager.UnlockAchievement("Prehistory_HerramientasDePiedra");
            logr03 = true;

        }

        // Logro: Puntuaci�n perfecta
        if (_score >= 30 && !logr04)
        {
            AchievementManager.UnlockAchievement("Prehistory_Domesticaci�nDeAnimales");
            logr04 = true;
        }

        UpdateUI();
    }


    void UpdateUI()
    {
        // Tiempo restante en minutos y segundos
        int minutes = Mathf.FloorToInt(_timeLeft / 60);
        int seconds = Mathf.FloorToInt(_timeLeft % 60);

        // Formateo el texto 
        timeText.text = string.Format("{0:0}:{1:00}", minutes, seconds);

        // Puntuacion
        scoreText.text = "Puntuacion: " + _score;
    }


    void UnlockAchievement(string achievementKey)
    {
        if (!AchievementManager.IsAchievementUnlocked(achievementKey))
        {
            AchievementManager.UnlockAchievement(achievementKey);
            _recentlyUnlockedAchievements.Add(achievementKey); // Guardar en lista temporal
        }
    }

    // Detengo el juego cuando se acabe el tiempo
    void EndGame()
    {
        runningGame = false;

        // Logro "El descubrimiento de �tzi"
        if (_holesHit.Count == _holes.Length && !logr05)
        {
            AchievementManager.UnlockAchievement("Prehistory_ElDescubrimientoDe�tzi");
            logr05 = true;
        }

        // Mostrar logros desbloqueados en esta partida
        if (_recentlyUnlockedAchievements.Count > 0)
        {
            string unlockedAchievements = "�Logros desbloqueados!\n";
            foreach (string achievement in _recentlyUnlockedAchievements)
            {
                unlockedAchievements += "- " + achievement.Replace("Prehistory_", "") + "\n";
            }
            Debug.Log(unlockedAchievements);
        }

        // Limpiar logros recientes
        _recentlyUnlockedAchievements.Clear();

        // Se inicia la transici�n
        GameSceneManager.instance.GameOverPrehistoryMedieval(_score, _hitDinosaurs, true);
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
        GameSceneManager.instance.GameOverPrehistoryMedieval(_score, _hitDinosaurs, false);
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