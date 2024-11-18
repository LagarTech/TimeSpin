using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MedievalGameManager : MonoBehaviour
{
    public static MedievalGameManager Instance;

    [SerializeField] private GameObject[] _swordPrefabs; // Prefabs de espadas (bronce, plata, oro)
    [SerializeField] private GameObject _spawnArea; // Donde aparecen las espadas
    private const float SPAWN_INTERVAL = 8f; // Tiempo de aparición de espadas
    private const float GAME_TIME = 60f; // Duración del minijuego

    private float _timeLeft; // Tiempo restante
    private float _spawnTimer = 5f; // Temporizador para spawnear las espadas

    public bool runningGame = false;

    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _scoreText; // UI de la puntuación del jugador

    public Transform[] bases;

    private int _score = 0; // Puntuación del juego
    private int _numSwords = 0; // Número de espadas

    // Gestión de las bases
    public int nextBaseIndex = 0;
    private const int NUM_BASES = 4;

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

    private void Start()
    {
        _timeLeft = GAME_TIME;
        UpdateUI();
        GameSceneManager.instance.practiceStarted = true;
        // Se indica cuál es la primera base a la que se debe llevar una espada
        ChooseRandomBase();
    }

    private void Update()
    {
        if (!runningGame) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _optionsPanel.SetActive(true);
            runningGame = false;
        }

        if (_timeLeft > 0)
        {
            _timeLeft -= Time.deltaTime;
            _spawnTimer += Time.deltaTime;

            // Si ha pasado el tiempo de spawn, se genera una nueva espada
            if (_spawnTimer >= SPAWN_INTERVAL)
            {
                SpawnRandomSword();
                _spawnTimer = 0f;
            }
            // Se actualiza la interfaz
            UpdateUI();
        }
        else
        {
            EndGame();
        }
    }

    public void ChooseRandomBase()
    {
        nextBaseIndex = Random.Range(0, NUM_BASES); // Se escoge una base al azar
    }

    void SpawnRandomSword()
    {
        // Se escoge una espada aleatoria
        int swordIndex = Random.Range(0, _swordPrefabs.Length);
        Instantiate(_swordPrefabs[swordIndex], GetRandomSpawnPosition(), Quaternion.identity);
    }

    Vector3 GetRandomSpawnPosition()
    {
        // Se genera una posición aleatoria
        MeshRenderer meshRenderer = _spawnArea.gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Vector3 size = meshRenderer.bounds.size;
            Vector3 center = meshRenderer.bounds.center;
            float x = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
            float z = Random.Range(center.z - size.z / 2, center.z + size.z / 2);
            return new Vector3(x, 2, z);
        }
        return Vector3.zero;
    }

    public void AddScore(int points)
    {
        _score += points; // Se suman los puntos asociados a la espada
        _numSwords++; // Se incrementa el número de espadas
        UpdateUI(); // Se actualiza la UI
    }

    void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(_timeLeft / 60);
        int seconds = Mathf.FloorToInt(_timeLeft % 60);
        _timeText.text = string.Format("{0:0}:{1:00}", minutes, seconds);

        _scoreText.text = _score.ToString() + " puntos";
    }

    void EndGame()
    {
        runningGame = false;
        GameSceneManager.instance.GameOverPrehistoryMedieval(_score, _numSwords, false); // Se muestra la pantalla de puntuaciones y se pasa a la siguiente pantalla
    }
    public void Options()
    {
        _optionsPanel.SetActive(false);
        runningGame = true;
    }

}
