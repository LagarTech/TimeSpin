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
    private const float SPAWN_INTERVAL = 3f; // Tiempo de aparición de espadas
    private const float GAME_TIME = 60f; // Duración del minijuego

    private float _timeLeft; // Tiempo restante
    private float _spawnTimer; // Temporizador para spawnear las espadas

    public bool runningGame = false;

    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _scoreText; // UI de la puntuación del jugador

    public Transform basePlayer;

    private int _score = 0; // Puntuación del juego
    private int _numSwords = 0; // Número de espadas

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
    }

    private void Update()
    {
        if (!runningGame) return;

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
            return new Vector3(x, 0, z);
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
    }

}
