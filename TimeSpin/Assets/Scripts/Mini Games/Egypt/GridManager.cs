using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    // Variable que controla el flujo del juego
    public bool runningGame = false;

    // Dimensiones del tablero
    private const int COLUMNS = 13;
    private const int ROWS = 9;
    // Casillas del tablero, organizadas en una matriz bidimensional
    private Tile[,] _gridTiles = new Tile[COLUMNS, ROWS];
    private float tileSize = 1f;
    // Lista de casillas no caminables de todo el mapa
    [SerializeField] private List<Vector2Int> _nonWalkableTiles;

    // Temporizador del juego
    [SerializeField] private TMP_Text _timerText;
    private float _remainingTime = 120f; // El tiempo de juego son 2 minutos (120 segundos)
    private float _survivedTime = 0f;

    // Gestión de la aparición aleatoria de pinchos
    // Lista de todas las casillas con pinchos
    [SerializeField] private List<GameObject> _spikesList;
    // Lista para generar la aparición aleatoria de los pinchos
    private List<int> _randomSpikesSpawn = new List<int>();
    // Número total de casillas que tendrán pinchos
    private const int NUM_SPIKES_TILES = 15;
    // Tiempo que tiene que transcurrir para que aparezcan los siguientes pinchos
    private float _spikesTime = 8f; // Se generan pinchos cada 8 segundos
    // Número de casillas con pinchos
    private int _numSpikes = 0;

    // Gestión de la aparición de momias
    // Prefab de la momia
    [SerializeField] private GameObject _mummy;
    // Posición de spawn de las momias
    [SerializeField] private Vector3 _mummySpawnPosition;
    // Tiempo que tiene que transcurrir para que se genere una nueva momia
    private float _mummyTime = 0f;

    /////////////////////////LOGROS///////////////////////////////////

    //Lista para saber la casilla
    public HashSet<Tile> _visitedTiles = new HashSet<Tile>();
    //Cola que almacena las puntuaciones junto con el tiempo en que fueron obtenidas
    private Queue<(float time, int points)> _scoreHistory = new Queue<(float time, int points)>();
    // Puntuación del jugador
    private int _playerScore = 0;
    // Indica si el jugador fue atrapado
    public bool PlayerCaught = false;
    // Referencias de UI o externas
    public Tile[,] _gridTile; // Mapa del juego
    //Centro 
    public bool centroPisado;

    // OPCIONES Y CONFIGURACIÓN

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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Se genera el tablero
        GenerateGrid();
        // Se prepara la gestión aleatoria de los pinchos 
        PrepareSpikesSpawn();
        // Se preapara el menú de opciones
        ShowCorrectButton();
    }

    private void Update()
    {
        if (!runningGame) return;

        // GESTIÓN DEL TIEMPO RESTANTE
        if (_remainingTime > 0f)
        {
            // Aumentar el tiempo sobrevivido
            _survivedTime+= Time.deltaTime;
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
            // Se finaliza el minijuego
            GameOver();
            return;
        }

        // Verifica logros
        CheckAchievements();

        // APARICIÓN DE PINCHOS
        _spikesTime -= Time.deltaTime;
        if (_spikesTime < 0f)
        {
            _spikesTime = 8f; // Entre apariciones se deja un tiempo de 8 segundos
            StartCoroutine(SpawnSpikesCoroutine());
        }

        // GENERACIÓN DE MOMIAS
        _mummyTime -= Time.deltaTime;
        if (_mummyTime < 0f)
        {
            // Instancia el objeto
            Instantiate(_mummy, _mummySpawnPosition, Quaternion.Euler(-90f, 0f, 0f));
            _mummyTime = 90f;
        }

    }

    private void CheckAchievements()
    {
        // 1. AstrologiaDePiramides: Sobrevive 30 segundos sin ser golpeado
        if (_survivedTime >= 30f && !PlayerCaught)
        {
            AchievementManager.UnlockAchievement("Egipto_AstrologiaDePiramides");
            Debug.Log("Desbloqueado: AstrologiaDePiramides");
        }

        // 2. CalendarioEgipcio: Sobrevive 1.5 minutos sin ser atrapado
        if (_survivedTime >= 90f && !PlayerCaught)
        {
            AchievementManager.UnlockAchievement("Egipto_CalendarioEgipcio");
            Debug.Log("Desbloqueado: CalendarioEgipcio");
        }

        // 3. DiversidadEnFaraones: Termina con al menos dos trampas activadas
        if (_remainingTime <= 0f && _numSpikes >= 2)
        {
            AchievementManager.UnlockAchievement("Egipto_DiversidadEnFaraones");
            Debug.Log("Desbloqueado: DiversidadEnFaraones");
        }

        // 4. ElMaquillajeDeLosOjos: Activa al menos 5 trampas sin morir
        if (_numSpikes >= 5 && !PlayerCaught)
        {
            AchievementManager.UnlockAchievement("Egipto_ElMaquillajeDeLosOjos");
            Debug.Log("Desbloqueado: ElMaquillajeDeLosOjos");
        }

        // 5. EscrituraJeroglifica: Utiliza todo el mapa para escapar
        if (_visitedTiles.Count == (COLUMNS * ROWS - _nonWalkableTiles.Count))
        {
            AchievementManager.UnlockAchievement("Egipto_EscrituraJeroglifica");
            Debug.Log("Desbloqueado: EscrituraJeroglifica");
        }

        // 6. GatosSagrados: Llega al centro del mapa antes de que se cierre el primer espacio
        Vector2Int centerTile = new Vector2Int(COLUMNS/2, ROWS/2);//Centro
        Tile centerTileObj = GetTile(centerTile.x, centerTile.y);

        if (_remainingTime >= 100f && GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>()._currentTile == centerTileObj)
        {
            AchievementManager.UnlockAchievement("Egipto_GatosSagrados");
            Debug.Log("Desbloqueado: GatosSagrados");
        }

        // 7. MujeresConDerechos: Obtén 50 puntos en una partida
        if (_playerScore >= 50)
        {
            AchievementManager.UnlockAchievement("Egipto_MujeresConDerechos");
            Debug.Log("Desbloqueado: MujeresConDerechos");
        }

        // 8. ProcesoDeMomificacion: Obtén 30 puntos 
        if (_playerScore >= 30)
        {
            AchievementManager.UnlockAchievement("Egipto_ProcesoDeMomificacion");
            Debug.Log("Desbloqueado: ProcesoDeMomificacion");
        }
    }

    #region Grid

    private void GenerateGrid()
    {
        for (int x = 0; x < COLUMNS; x++)
        {
            for (int z = 0; z < ROWS; z++)
            {
                // Primero se define el centro de la casilla en la que se encuentra
                Vector3 tileCenter = new Vector3((x * tileSize) + tileSize / 2, 0, -((z * tileSize) + tileSize / 2));

                // Se crea la casilla y se añade a la matriz
                _gridTiles[x, z] = new Tile(x, z, tileCenter);
            }
        }

        GenerateNonWalkableTiles();
    }

    private void GenerateNonWalkableTiles()
    {
        // Se recorren todas las coordenadas de las casillas no caminables, para marcarlas en el tablero
        foreach (Vector2Int tile in _nonWalkableTiles)
        {
            _gridTiles[tile.x, tile.y].MakeNonWalkable();
        }
    }

    public Tile GetTile(int x, int z)
    {
        // Comprobar si las coordenadas están dentro de los límites del tablero
        if (x < 0 || x >= COLUMNS || z < 0 || z >= ROWS)
        {
            // Si están fuera de los límites, devolver null
            return null;
        }

        // Si están dentro de los límites, devolver la casilla correspondiente, se añade a la lista de casillas caminadas        
        _visitedTiles.Add(_gridTiles[x, z]);
        return _gridTiles[x, z];

    }

    public Tile[] GetWalkableNeighbours(Tile currentTile)
    {
        List<Tile> walkableNeighbours = new List<Tile>();

        // Obtener las coordenadas de la casilla actual
        Vector2Int currentPos = new Vector2Int(currentTile.xTile, currentTile.zTile);

        // Definir los posibles desplazamientos (arriba, abajo, izquierda, derecha)
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, -1),  // Arriba
        new Vector2Int(0, 1), // Abajo
        new Vector2Int(-1, 0), // Izquierda
        new Vector2Int(1, 0)   // Derecha
        };

        // Iterar sobre cada dirección y obtener las casillas vecinas
        foreach (Vector2Int dir in directions)
        {
            // Obtener las coordenadas de la casilla vecina
            Vector2Int neighbourPos = currentPos + dir;

            // Obtener la casilla vecina del GridManager (o tu gestor de tablero)
            Tile neighbourTile = GridManager.Instance.GetTile(neighbourPos.x, neighbourPos.y);

            // Si la casilla vecina existe y es caminable, añadirla a la lista
            if (neighbourTile != null && neighbourTile.isWalkable)
            {
                walkableNeighbours.Add(neighbourTile);
            }
        }

        // Devolver la lista de vecinos caminables como un array
        return walkableNeighbours.ToArray();
    }
    #endregion

    #region Timer

    private void UpdateTimer()
    {
        // Calcular minutos y segundos
        int displayMinutes = Mathf.FloorToInt(_remainingTime / 60);
        int displaySeconds = Mathf.FloorToInt(_remainingTime % 60);

        // Actualizar el texto del TMP para que muestre el tiempo restante
        _timerText.text = string.Format("{0:00}:{1:00}", displayMinutes, displaySeconds);
    }

    #endregion

    #region Spikes

    private void PrepareSpikesSpawn()
    {
        // Al ser un total de 10 pinchos, se genera una lista de 10 elementos
        for (int i = 0; i < NUM_SPIKES_TILES; i++)
        {
            _randomSpikesSpawn.Add(i);
        }
        // Para garantizar que aparezcan de forma aleatoria, es decir, en cada partida con un orden distinto,
        // se utiliza el algoritmo de Fisher - Yates, con coste O(N)
        int n = _randomSpikesSpawn.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1); // Usamos UnityEngine.Random para generar un número aleatorio
            // Intercambiar los elementos en i y j
            int temp = _randomSpikesSpawn[i];
            _randomSpikesSpawn[i] = _randomSpikesSpawn[j];
            _randomSpikesSpawn[j] = temp;
        }
    }

    private IEnumerator SpawnSpikesCoroutine()
    {
        MusicManager.PonerMusica(_clipAudio, _reproductor, false); // Reproducción del efecto de sonido
        yield return new WaitForSeconds(1.5f);
        SpawnSpikes();
    }
    private void SpawnSpikes()
    {
        int randomSpike = _randomSpikesSpawn[_numSpikes];
        // Se activan los pinchos de la casilla que toque según el orden aleatorio
        _spikesList[randomSpike].SetActive(true);
        // Se obtiene la posición global de dicha casilla
        Vector3 spikesPos = _spikesList[_randomSpikesSpawn[_numSpikes]].transform.position;
        // Se pasa de dichas coordenadas a las locales del escenario
        Vector2 spikesTilePos = new Vector2(spikesPos.x - 0.5f, -(spikesPos.z + 0.5f));
        // Se utilizan dichas coordenadas de la casilla para marcarla como no caminable
        GetTile((int)spikesTilePos.x, (int)spikesTilePos.y).MakeNonWalkable();
        // Se indica que se ha generado una casilla más con pinchos
        _numSpikes++;
        // Una vez hecho, se activan los pinchos
        _spikesList[randomSpike].SetActive(true);
    }

    #endregion
    public void GameOver()
    {
        if (!runningGame) return;
        // Se indica que ha terminado el juego
        runningGame = false;

        // Revisión de logros finales
        if (_numSpikes >= 2)
        {
            AchievementManager.UnlockAchievement("Egipto_DiversidadEnFaraones");
            Debug.Log("Desbloqueado: DiversidadEnFaraones");
        }

        // Se calcula la puntuación del jugador en base a los resultados
        GameSceneManager.instance.GameOverEgyptFuture(_survivedTime, true);
    }


    //Cada vez que se obtienen puntos
    public void AddPoints(int points)
    {
        _playerScore += points; // Sumar directamente los puntos obtenidos
        _scoreHistory.Enqueue((Time.time, points));

        // Limpia la cola eliminando los puntos fuera del rango de 60 segundos
        while (_scoreHistory.Count > 0 && Time.time - _scoreHistory.Peek().time > 60f)
        {
            _scoreHistory.Dequeue();
        }
    }

    //Puntos en los ultimos 60 segundos
    private int PointsInLastMinute()
    {
        int totalPoints = 0;

        // Suma los puntos dentro del rango de 60 segundos
        foreach (var entry in _scoreHistory)
        {
            if (Time.time - entry.time <= 120f)
            {
                totalPoints += entry.points;
            }
        }

        return totalPoints;
    }

    public void ShowOptions()
    {
        _optionsPanel.SetActive(true);
        runningGame = false;
    }
    public void Options()
    {
        _optionsPanel.SetActive(false);
        runningGame = true;
    }

    private void ShowCorrectButton()
    {
        if(GameSceneManager.instance.practiceStarted)
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
        GameSceneManager.instance.GameOverEgyptFuture(_survivedTime, true);
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
