using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridManager : NetworkBehaviour
{
    public static GridManager Instance;

    // Variable que controla el flujo del juego
    public bool runningGame = true;
    // Variable que gestiona el número de jugadores
    private int _numPlayers;

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
        // Se genera el tablero, la lógica sólo se almacena en el servidor
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            GenerateGrid();
            // Se prepara la gestión aleatoria de los pinchos en el servidor
            PrepareSpikesSpawn();
        }
    }

    private void Update()
    {
        if (!runningGame) return;

        // GESTIÓN DEL TIEMPO RESTANTE
        if (_remainingTime > 0f)
        {
            // Disminuir el tiempo restante
            _remainingTime -= Time.deltaTime;
            // Se actualiza el temporizador sólo en el cliente
            if (Application.platform != RuntimePlatform.LinuxServer)
            {
                UpdateTimer();
            }
        }
        else
        {
            _remainingTime = 0f;
            // Se actualiza el temporizador sólo en el cliente
            if (Application.platform != RuntimePlatform.LinuxServer)
            {
                UpdateTimer();
            }
            // Se indica que el juego ha finalizado
            runningGame = false;
            GameOver();
            return;
        }

        // APARICIÓN DE PINCHOS
        // Esto ser realizará en el servidor, que es el que almacena la lógica, y en el cliente, para hacerlo visible
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            _spikesTime -= Time.deltaTime;
            if (_spikesTime < 0f)
            {
                _spikesTime = 10f; // Entre apariciones se deja un tiempo de 10 segundos
                SpawnSpikes();
            }
        }

        // GENERACIÓN DE MOMIAS
        // Únicamente en el servidor, se spawnearán directamente en el cliente
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            _mummyTime -= Time.deltaTime;
            if (_mummyTime < 0f)
            {
                if (IsServer)  // Solo el servidor puede spawnear objetos
                {
                    // Instancia el objeto en el servidor
                    GameObject _mummyObj = Instantiate(_mummy, _mummySpawnPosition, Quaternion.identity);

                    // Lo registramos en la red para sincronizarlo con los clientes
                    _mummyObj.GetComponent<NetworkObject>().Spawn();
                    _mummyTime = 30f;
                }
            }
        }

        // CONTROL DEL NÚMERO DE JUGADORES
        // Únicamente en el servidor
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            _numPlayers = GameObject.FindGameObjectsWithTag("Player").Length;
            // Si sólo queda un jugador, se termina el juego
            if (_numPlayers == 1)
            {
                runningGame = false;
                GameOver();
            }
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
        // Primero se realiza la lógica en el servidor
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
        // Una vez hecho, se activan los pinchos en los clientes
        SpawnSpikesClientRpc(randomSpike);
    }

    [ClientRpc]
    private void SpawnSpikesClientRpc(int idSpike)
    {
        // Se hace visible en los clientes la trampa que toque, en base a lo realizado en el servidor
        _spikesList[idSpike].SetActive(true);
    }

    #endregion
    private void GameOver()
    {
        // Cuando termina el juego, por el momento, se carga el siguiente minijuego
        NetworkManager.Singleton.SceneManager.LoadScene("Maya", LoadSceneMode.Single);
        // Se reactiva la lista de jugadores
        GameSceneManager.instance.ActivePlayersList();
    }

    [ClientRpc]
    private void GameOverClientRpc()
    {
        // Se reactiva la lista de jugadores
        GameSceneManager.instance.ActivePlayersList();
    }
}
