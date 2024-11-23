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
    private const float SPAWN_INTERVAL = 4f; // Tiempo de aparición de espadas
    private const float GAME_TIME = 60f; // Duración del minijuego

    private float _timeLeft; // Tiempo restante
    private float _spawnTimer = 4f; // Temporizador para spawnear las espadas

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

    [SerializeField]
    private AudioSource _reproductor;
    [SerializeField]
    private AudioClip _clipAudio;

    /////////////////LOGROS////////////////////
    private HashSet<string> _swordsDelivered = new HashSet<string>(); // Tipos de espadas entregadas ("Bronze", "Silver", "Gold")
    private int _goldSwordsDelivered = 0; // Espadas de oro entregadas en una partida
    private int _goldSwordsDeliveredStreak = 0; // Racha de espadas de oro consecutivas
    private int _totalObjectsDelivered = 0; // Total de objetos entregados
    private HashSet<int> _usedBases = new HashSet<int>(); // Índices de las bases utilizadas
    private bool _collidedWithObstacle = false; // Si el jugador chocó contra un obstáculo

    // Logro de puntuación
    private const int POINTS_FOR_MEDIEVAL = 20;

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
                MusicManager.PonerMusica(_clipAudio, _reproductor, false);
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

    private void CheckAchievements()
    {
        // 1. CaballerosYTorneos: Recoge una espada y llévala a su baúl
        if (_totalObjectsDelivered >= 1)
        {
            AchievementManager.UnlockAchievement("CaballerosYTorneos");
            Debug.Log("Desbloqueado: CaballerosYTorneos");
        }

        // 2. ElSistemaFeudal: Lleva al menos una espada de cada tipo al baúl
        if (_swordsDelivered.Contains("Bronze") && _swordsDelivered.Contains("Silver") && _swordsDelivered.Contains("Gold"))
        {
            AchievementManager.UnlockAchievement("ElSistemaFeudal");
            Debug.Log("Desbloqueado: CaballerosYTorneos");
        }

        // 3. LaPesteNegra: Lleva al menos 3 espadas de oro en una partida al baúl
        if (_goldSwordsDelivered >= 3)
        {
            AchievementManager.UnlockAchievement("Medieval_LaPesteNegra");
            Debug.Log("Desbloqueado: CaballerosYTorneos");
        }

        // 4. LasCiudadesAmuralladas: Lleva 3 espadas de oro seguidas al baúl
        if (_goldSwordsDeliveredStreak >= 3)
        {
            AchievementManager.UnlockAchievement("Medieval_LasCiudadesAmuralladas");
            Debug.Log("Desbloqueado: LasCiudadesAmuralladas");
        }

        // 5. LasCruzadas: Lleva al menos 6 objetos a los baúles
        if (_totalObjectsDelivered >= 6)
        {
            AchievementManager.UnlockAchievement("Medieval_LasCruzadas");
            Debug.Log("Desbloqueado: LasCruzadas");
        }

        // 6. LosCastillos: No chocar contra ningún obstáculo en una partida
        if (!_collidedWithObstacle && _timeLeft <= 0)
        {
            AchievementManager.UnlockAchievement("Medieval_LosCastillos");
            Debug.Log("Desbloqueado: LosCastillos");
        }

        // 7. LosGremios: Utiliza todos los baúles al menos una vez
        if (_usedBases.Count == NUM_BASES)
        {
            AchievementManager.UnlockAchievement("Medieval_LosGremios");
            Debug.Log("Desbloqueado: LosGremios");
        }

        // 8. MujeresEnElMedievo: Consigue 20 puntos o más
        if (_score >= POINTS_FOR_MEDIEVAL)
        {
            AchievementManager.UnlockAchievement("Medieval_MujeresEnElMedievo");
            Debug.Log("Desbloqueado: MujeresEnElMedievo");
        }
    }
    public void DeliverSword(string swordType)
    {
        _totalObjectsDelivered++; // Incrementa el total de objetos entregados
        _swordsDelivered.Add(swordType); // Registra el tipo de espada entregada
        _usedBases.Add(nextBaseIndex); // Marca la base como utilizada

        if (swordType == "Gold")
        {
            _goldSwordsDelivered++;
            _goldSwordsDeliveredStreak++;
        }
        else
        {
            _goldSwordsDeliveredStreak = 0; // Reinicia la racha si no es de oro
        }

        CheckAchievements(); // Verifica los logros después de entregar
    }

    //Se marca que el jugador chocó con un obstáculo 
    public void PlayerHitObstacle()
    {
        _collidedWithObstacle = true;
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
        CheckAchievements();
        GameSceneManager.instance.GameOverPrehistoryMedieval(_score, _numSwords, false); // Se muestra la pantalla de puntuaciones y se pasa a la siguiente pantalla
    }
    public void Options()
    {
        _optionsPanel.SetActive(false);
        runningGame = true;
    }

}
