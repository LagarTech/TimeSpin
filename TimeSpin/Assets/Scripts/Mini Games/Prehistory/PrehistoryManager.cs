using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PrehistoryManager : MonoBehaviour
{
    public static PrehistoryManager Instance;

    [SerializeField] Transform[] _holes; // Posiciones de los agujeros
    private const float SPAWN_INTERVAL = 2f; // Intervalo de aparición
    private const float GAME_TIME = 60f; // Duración del juego

    private float _timeLeft;
    private float _spawnTimer;

    public bool runningGame = false;

    [SerializeField] private TMP_Text timeText; // UI del tiempo
    [SerializeField] private TMP_Text scoreText; // UI de la puntuación
    private int _score;
    private int _hitDinosaurs = 0;

    private HashSet<int> _occupiedHoles = new HashSet<int>(); // Conjunto que almacena los agujeros ocupados por dinosaurios

    //Logros
    public int _velociraptorHits = 0;
    public bool _tRexDefeated = false;
    public HashSet<int> _holesHit = new HashSet<int>(); // Agujeros golpeados
    public float _lastHitTime = -5f; // Tiempo del último golpe para "Racha rápida"
    public int _consecutiveHits = 0; // Golpes consecutivos en corto tiempo

    private List<string> _recentlyUnlockedAchievements = new List<string>(); //lista de logros desbloqueados recientemente

    [SerializeField]
    private GameObject _optionsPanel;

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

    }

    void Update()
    {
        // Si el juego no ha comenzado, no se ejecuta nada
        if (!runningGame) return;
        /*if (Input.GetKeyDown(KeyCode.Escape))
        {
            _optionsPanel.SetActive(true);
            runningGame = false;
        }*/
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
        // Elegir un agujero aleatorio que no esté ocupado
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
            // Animación del dinosaurio 
            StartCoroutine(dinosaur.GetComponent<DinosaurController>().AppearAnimation());
            dinosaur.GetComponent<DinosaurController>().holeIndex = holeIndex; // Se indica qué agujero ocupa el dinosaurio
            _occupiedHoles.Add(holeIndex);
        }
    }

    // Método para liberar el agujero cuando el dinosaurio desaparece
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
        if (_hitDinosaurs == 1)
        {
            AchievementManager.UnlockAchievement("Prehistory_PrimerosInstrumentosMusicales");
        }

        // Logro: Racha rápida
        if (currentTime - _lastHitTime <= 10f)
        {
            _consecutiveHits++;
            if (_consecutiveHits >= 3)
            {
                AchievementManager.UnlockAchievement("Prehistory_PinturasRupestres");
            }
        }
        else
        {
            _consecutiveHits = 1; // Reinicia la racha si no golpea a tiempo
        }

        _lastHitTime = currentTime;

        // Logro: Cazador experto
        if (_hitDinosaurs >= 10)
        {
            AchievementManager.UnlockAchievement("Prehistory_HerramientasDePiedra");
        }

        // Logro: Puntuación perfecta
        if (_score >= 30)
        {
            AchievementManager.UnlockAchievement("Prehistory_DomesticaciónDeAnimales");
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

        // Logro "El descubrimiento de Ötzi"
        if (_holesHit.Count == _holes.Length)
        {
            AchievementManager.UnlockAchievement("Prehistory_ElDescubrimientoDeÖtzi");
        }

        // Mostrar logros desbloqueados en esta partida
        if (_recentlyUnlockedAchievements.Count > 0)
        {
            string unlockedAchievements = "¡Logros desbloqueados!\n";
            foreach (string achievement in _recentlyUnlockedAchievements)
            {
                unlockedAchievements += "- " + achievement.Replace("Prehistory_", "") + "\n";
            }
            Debug.Log(unlockedAchievements);
        }

        // Limpiar logros recientes
        _recentlyUnlockedAchievements.Clear();

        // Se inicia la transición
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
}