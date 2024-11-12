using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using UnityEngine;
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
    private const int NUM_SPIKES_TILES = 10;
    // Tiempo que tiene que transcurrir para que aparezcan los siguientes pinchos
    private float _spikesTime = 20f; // En un principio, se esperan 20 segundos para empezar a generar pinchos
    // Número de casillas con pinchos
    private int _numSpikes = 0;

    // Gestión de la aparición de momias
    // Prefab de la momia
    [SerializeField] private GameObject _mummy;
    // Posición de spawn de las momias
    [SerializeField] private Vector3 _mummySpawnPosition;
    // Tiempo que tiene que transcurrir para que se genere una nueva momia
    private float _mummyTime = 0f;

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

        // APARICIÓN DE PINCHOS
        _spikesTime -= Time.deltaTime;
        if (_spikesTime < 0f)
        {
            _spikesTime = 10f; // Entre apariciones se deja un tiempo de 10 segundos
            SpawnSpikes();
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

        // Si están dentro de los límites, devolver la casilla correspondiente
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
        // Se calcula la puntuación del jugador en base a los resultados
        GameSceneManager.instance.GameOverEgyptFuture(_survivedTime, true);
    }

}
