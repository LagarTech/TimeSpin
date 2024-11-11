using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PrehistoryManager : MonoBehaviour
{
    public static PrehistoryManager Instance;

    [SerializeField] Transform[] _holes; // Posiciones de los agujeros
    private const float SPAWN_INTERVAL = 1.25f; // Intervalo de aparici�n
    private const float GAME_TIME = 60f; // Duraci�n del juego

    private float _timeLeft;
    private float _spawnTimer;

    public bool runningGame = false;

    [SerializeField] private TMP_Text timeText; // UI del tiempo
    [SerializeField] private TMP_Text scoreText; // UI de la puntuaci�n
    private int _score;
    private int _hitDinosaurs = 0;

    private HashSet<int> _occupiedHoles = new HashSet<int>(); // Conjunto que almacena los agujeros ocupados por dinosaurios

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
        GameSceneManager.instance.practiceStarted = true;

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
        _hitDinosaurs++; // Se incrementa el n�mero de dinosaurios derrotados
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
        scoreText.text = "Puntuaci�n: " + _score;
    }

    // Detengo el juego cuando se acabe el tiempo
    void EndGame()
    {
        runningGame = false;
        // Se inicia la transici�n
        GameSceneManager.instance.GameOverPrehistoryMedieval(_score, _hitDinosaurs, true);
    }

}