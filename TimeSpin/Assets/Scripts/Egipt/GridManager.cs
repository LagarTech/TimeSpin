using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

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


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GenerateGrid();
        PrepareSpikesSpawn();
    }

    private void Update()
    {
        // GESTIÓN DEL TIEMPO RESTANTE
        if(_remainingTime > 0f)
        {
            // Disminuir el tiempo restante
            _remainingTime -= Time.deltaTime;
            // Se actualiza el temporizador
            UpdateTimer();
        }
        else
        {
            _remainingTime = 0f;
            // GameOver
            return;
        }
        // APARICIÓN DE PINCHOS
        _spikesTime -= Time.deltaTime;
        if(_spikesTime < 0f)
        {
            _spikesTime = 10f; // Entre apariciones se deja un tiempo de 10 segundos
            SpawnSpikes();
        }
    }

    private void GenerateGrid()
    {
        for(int x = 0; x < COLUMNS; x++)
        {
            for(int z = 0; z < ROWS; z++)
            {
                // Primero se define el centro de la casilla en la que se encuentra
                Vector3 tileCenter = new Vector3((x * tileSize) + tileSize / 2, 0, - ((z * tileSize) + tileSize / 2));

                // Se crea la casilla y se añade a la matriz
                _gridTiles[x,z] = new Tile(x, z, tileCenter);
            }
        }

        GenerateNonWalkableTiles();
    }

    private void GenerateNonWalkableTiles()
    {
        // Se recorren todas las coordenadas de las casillas no caminables, para marcarlas en el tablero
        foreach(Vector2Int tile in _nonWalkableTiles)
        {
            _gridTiles[tile.x, tile.y].MakeNonWalkable();
        }
    }

    public Tile GetTile(int x, int z)
    {
        return _gridTiles[x, z];
    }

    private void UpdateTimer()
    {
        // Calcular minutos y segundos
        int displayMinutes = Mathf.FloorToInt(_remainingTime / 60);
        int displaySeconds = Mathf.FloorToInt(_remainingTime % 60);

        // Actualizar el texto del TMP para que muestre el tiempo restante
        _timerText.text = string.Format("{0:00}:{1:00}", displayMinutes, displaySeconds);
    }

    private void PrepareSpikesSpawn()
    {
        // Al ser un total de 10 pinchos, se genera una lista de 10 elementos
        for(int i = 0; i < NUM_SPIKES_TILES; i++)
        {
            _randomSpikesSpawn.Add(i);
        }
        // Para garantizar que aparezcan de forma aleatoria, es decir, en cada partida con un orden distinto,
        // se utiliza el algoritmo de Fisher - Yates, con coste O(N)
        int n = _randomSpikesSpawn.Count;
        for(int i = n - 1; i > 0; i--)
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
        // Se activan los pinchos de la casilla que toque según el orden aleatorio
        _spikesList[_randomSpikesSpawn[_numSpikes]].SetActive(true);
        // Se obtiene la posición global de dicha casilla
        Vector3 spikesPos = _spikesList[_randomSpikesSpawn[_numSpikes]].transform.position;
        // Se pasa de dichas coordenadas a las locales del escenario
        Vector2 spikesTilePos = new Vector2(spikesPos.x - 0.5f, -(spikesPos.z + 0.5f));
        // Se utilizan dichas coordenadas de la casilla para marcarla como no caminable
        GetTile((int)spikesTilePos.x, (int)spikesTilePos.y).MakeNonWalkable();
        // Se indica que se ha generado una casilla más con pinchos
        _numSpikes++;
    }
}
