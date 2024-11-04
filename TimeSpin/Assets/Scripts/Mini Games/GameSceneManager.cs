using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameSceneManager : NetworkBehaviour
{
    public static GameSceneManager instance;

    [SerializeField] private GameObject[] _playersList;
    private int _numPlayers;
    // Control de las puntuaciones de cada minijuego
    public List<PlayerMovement> orderedPlayers; // Lista para ordenar a los jugadores en función de los resultados
    public List<PlayerMovement> defeatedPlayers; // Lista para colocar los jugadores eliminados (EGIPTO Y FUTURO)
    public List<PlayerMovement> finishedPlayers; // Lista para colocar los jugadores que terminen la carrera (MAYA)
    // Puntuaciones fijas para los minijuegos de carrera o supervivencia
    public List<int> fixedPointsGames;
    private const int MAX_PLAYERS = 4;

    public bool gameStarted = false;
    public bool practiceStarted = false;

    private const int NUM_GAMES = 5;
    private bool[] _playedGames; // Lista en la que se va a marcar los minijuegos que se van jugando, para que no se puedan repetir
    private int _numPlayedGames = 0; // Contador que maneja el número de minijuegos que ya se han jugado
    public bool allGamesPlayed = false; // Variable que gestiona cuándo termina la partida, al jugar los 5 minijuegos

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        // Se hace que el objeto navegue entre escenas y no se destruya
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        if (Application.platform != RuntimePlatform.LinuxServer) return;
        _playedGames = new bool[NUM_GAMES]; // Se inicializa la lista para los 5 minijuegos
        for(int i = 0; i < NUM_GAMES; i++)
        {
            _playedGames[i] = false; // Se indica que ningún minijuego se ha jugado por el momento
        }
    }

    public void InitializePlayersList()
    {
        _playersList = GameObject.FindGameObjectsWithTag("Player");
        _numPlayers = _playersList.Length;
    }

    public void ActivePlayersList()
    {
        // Se reactivan los jugadores de la lista
        foreach(var player in _playersList)
        {
            PlayerMovement pMovement = player.GetComponent<PlayerMovement>();
            if (pMovement != null)
            {
                pMovement.ShowPlayer();
            }
        }
    }

    public void RegisterGameSelection(int gameID)
    {
        _playedGames[gameID] = true; // Se marca el juego como ya escogido
        _numPlayedGames++; // Se incrementa el número de minijuegos jugados
        if(_numPlayedGames == NUM_GAMES)
        {
            allGamesPlayed = true; // Si se han jugado todos, se indica para poder terminar el juego
        }
    }

    public bool[] GetPlayedGames() { return _playedGames; }
    
    // GESTIÓN DE LAS PUNTUACIONES DE LOS MINIJUEGOS
    public void GameOverEgiptFuture()
    {
        // Primero se indica que aquellos jugadores que no han sido eliminados han quedado en el primer puesto
        int numPlayersNonDefeated = 0;
        foreach(var player in _playersList)
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                if(!playerMovement.isDefeated)
                {
                    playerMovement.currentPosition = 1; // La posición de aquellos que no han sido eliminados será 1º
                    numPlayersNonDefeated++; // Se indica que hay un jugador más que no ha sido derrotado, para empezar a contar las posiciones correctamente de aquellos que sí
                    orderedPlayers.Add(playerMovement);
                }
            }
        }
        // Se establecen las posiciones de los jugadores eliminados (desde el final de la lista)
        for(int i = defeatedPlayers.Count - 1; i >= 0; i--)
        {
            defeatedPlayers[i].currentPosition = numPlayersNonDefeated + 1;
            numPlayersNonDefeated++;
            orderedPlayers.Add(defeatedPlayers[i]);
        }
        // Se asignan las puntuaciones en función del puesto y del número de jugadores que hubiera
        foreach(var player in orderedPlayers)
        {
            player.pointsToAdd = MAX_PLAYERS - _numPlayers + player.currentPosition - 1;
        }

        // Toda esta información está procesada en el servidor, al cliente es necesario pasarle:
        // - El orden de los OwnerClientId en base a los orderedPlayers
        // - La posición de cada uno de ellos
        // - Los puntos a sumar de cada uno de ellos
        int[] orderedPlayersId = new int[orderedPlayers.Count];
        int[] positionPlayers = new int[orderedPlayers.Count];
        int[] pointsPlayers = new int[orderedPlayers.Count];

        for(int i = 0; i<orderedPlayers.Count; i++)
        {
            orderedPlayersId[i] = orderedPlayers[i].ownerClient;
            positionPlayers[i] = orderedPlayers[i].currentPosition;
            pointsPlayers[i] = orderedPlayers[i].pointsToAdd;
        }

        UpdateGameOverStatsClientRpc(orderedPlayersId, positionPlayers, pointsPlayers);
    }

    [ClientRpc]
    public void UpdateGameOverStatsClientRpc(int[] orderedPlayersId, int[] positionPlayers, int[] pointsPlayers)
    {
        Debug.Log("Calculando puntuaciones...");
        // Con esta función se tendrán a los jugadores ordenados de la misma forma que en el servidor, para mostrarlos por pantalla en el panel de puntuaciones
        // Se guardan además los puntos para añadir y la posición concreta en la que han quedado
        GameObject[] _playersList = GameObject.FindGameObjectsWithTag("Player");
        for(int i = 0; i< orderedPlayersId.Length; i++)
        {
            for(int j = 0; j< _playersList.Length; j++)
            {
                if (_playersList[j].GetComponent<PlayerMovement>().ownerClient == orderedPlayersId[i])
                {
                    orderedPlayers.Add(_playersList[j].GetComponent<PlayerMovement>());
                    _playersList[j].GetComponent<PlayerMovement>().currentPosition = positionPlayers[i];
                    _playersList[j].GetComponent<PlayerMovement>().pointsToAdd = positionPlayers[i];
                }
            }
        }


    }
}
