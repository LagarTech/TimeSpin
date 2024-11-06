using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameSceneManager : NetworkBehaviour
{
    public static GameSceneManager instance;

    [SerializeField] private GameObject[] _playersList;
    private int _numPlayers;
    // Control de las puntuaciones de cada minijuego
    public List<PlayerMovement> orderedPlayers; // Lista para ordenar a los jugadores en funci�n de los resultados
    public List<PlayerMovement> defeatedPlayers; // Lista para colocar los jugadores eliminados (EGIPTO Y FUTURO)
    public List<PlayerMovement> finishedPlayers; // Lista para colocar los jugadores que terminen la carrera (MAYA)
    // Puntuaciones fijas para los minijuegos de carrera o supervivencia
    public List<int> fixedPointsGames;
    private const int MAX_PLAYERS = 4;

    public bool gameStarted = false;
    public bool practiceStarted = false;

    private const int NUM_GAMES = 5;
    private bool[] _playedGames; // Lista en la que se va a marcar los minijuegos que se van jugando, para que no se puedan repetir
    private int _numPlayedGames = 0; // Contador que maneja el n�mero de minijuegos que ya se han jugado
    public bool allGamesPlayed = false; // Variable que gestiona cu�ndo termina la partida, al jugar los 5 minijuegos

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
            _playedGames[i] = false; // Se indica que ning�n minijuego se ha jugado por el momento
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
        _numPlayedGames++; // Se incrementa el n�mero de minijuegos jugados
        if(_numPlayedGames == NUM_GAMES)
        {
            allGamesPlayed = true; // Si se han jugado todos, se indica para poder terminar el juego
        }
    }

    public bool[] GetPlayedGames() { return _playedGames; }
    
    // GESTI�N DE LAS PUNTUACIONES DE LOS MINIJUEGOS
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
                    playerMovement.currentPosition = 1; // La posici�n de aquellos que no han sido eliminados ser� 1�
                    numPlayersNonDefeated++; // Se indica que hay un jugador m�s que no ha sido derrotado, para empezar a contar las posiciones correctamente de aquellos que s�
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
        // Se asignan las puntuaciones en funci�n del puesto y del n�mero de jugadores que hubiera
        foreach(var player in orderedPlayers)
        {
            player.pointsToAdd = fixedPointsGames[MAX_PLAYERS - _numPlayers + player.currentPosition - 1];
        }

        // Se prepara la informaci�n para pasarla a los clientes
        SetClientGameOverData();

    }

    public void GameOverMaya()
    {
        // Se establecen las posiciones y puntuaciones de los jugadores en funci�n del orden en el que hayan llegado a la meta
        for(int i = 0; i < finishedPlayers.Count; i++)
        {
            finishedPlayers[i].currentPosition = i + 1;
            finishedPlayers[i].currentPoints = fixedPointsGames[MAX_PLAYERS - _numPlayers + finishedPlayers[i].currentPosition - 1];
            orderedPlayers.Add(finishedPlayers[i]); // Se a�ade a la lista ordenada
        }
        // Se establece la puntuaci�n del jugador �ltimo, el que no ha llegado a la meta
        foreach(var player in _playersList)
        {
            if(!player.GetComponent<PlayerMovement>().goalReached)
            {
                player.GetComponent<PlayerMovement>().currentPosition = finishedPlayers.Count + 1; // �ltima posici�n
                player.GetComponent<PlayerMovement>().currentPoints = fixedPointsGames[3]; // Peor puntuaci�n
                orderedPlayers.Add(player.GetComponent<PlayerMovement>());
            }
        }

        // Se preparan los datos para pasarlos a los clientes
        SetClientGameOverData();

    }

    private void SetClientGameOverData()
    {
        // Toda esta informaci�n est� procesada en el servidor, al cliente es necesario pasarle:
        // - El orden de los OwnerClientId en base a los orderedPlayers
        // - La posici�n de cada uno de ellos
        // - Los puntos a sumar de cada uno de ellos
        int[] orderedPlayersId = new int[orderedPlayers.Count];
        int[] positionPlayers = new int[orderedPlayers.Count];
        int[] pointsPlayers = new int[orderedPlayers.Count];

        for (int i = 0; i < orderedPlayers.Count; i++)
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
        // Con esta funci�n se tendr�n a los jugadores ordenados de la misma forma que en el servidor, para mostrarlos por pantalla en el panel de puntuaciones
        // Se guardan adem�s los puntos para a�adir y la posici�n concreta en la que han quedado
        GameObject[] _playersList = GameObject.FindGameObjectsWithTag("Player");
        for(int i = 0; i< orderedPlayersId.Length; i++)
        {
            for(int j = 0; j< _playersList.Length; j++)
            {
                if (_playersList[j].GetComponent<PlayerMovement>().ownerClient == orderedPlayersId[i])
                {
                    orderedPlayers.Add(_playersList[j].GetComponent<PlayerMovement>());
                    _playersList[j].GetComponent<PlayerMovement>().currentPosition = positionPlayers[i];
                    _playersList[j].GetComponent<PlayerMovement>().pointsToAdd = pointsPlayers[i];
                }
            }
        }
    }

    // Estructura y comparador para mostrar el ranking en el lobby
    struct PlayerScore
    {
        public string playerName;
        public int playerScore;
    }

    class PlayerScoreComparer : IComparer<PlayerScore>
    {
        public int Compare(PlayerScore x, PlayerScore y)
        {
            // Ordena de mayor a menor por playerScore
            return y.playerScore.CompareTo(x.playerScore);
        }
    }

    public void UpdateLobbyRanking()
    {
        _playersList = GameObject.FindGameObjectsWithTag("Player");
        List<PlayerScore> ranking = new List<PlayerScore>(_playersList.Length);
        foreach(var player in _playersList)
        {
            // Se guardan los puntos y el nombre de cada jugador, se almacena en la lista
            PlayerMovement playerData = player.GetComponent<PlayerMovement>();
            PlayerScore playerRanking = new PlayerScore();
            playerRanking.playerName = playerData.characterNamePlayer.GetComponentInChildren<TMP_Text>().text;
            playerRanking.playerScore = playerData.currentPoints;
            ranking.Add(playerRanking);
        }
        // Se ordena la lista
        ranking.Sort(new PlayerScoreComparer());

        // Se buscan los objetos de texto para almacenar el ranking
        GameObject[] rankingGO = GameObject.FindGameObjectsWithTag("RankingLobby");
        // Se almacenan los componentes de text
        List<TMP_Text> rankingTexts= new List<TMP_Text>();
        foreach(var text in rankingGO)
        {
            rankingTexts.Add(text.GetComponent<TMP_Text>());
        }
        // Se muestran las puntuaciones ordenadas
        for(int i = 0; i < ranking.Count; i++)
        {
            rankingTexts[i].text = (i+1).ToString() + "� - " + ranking[i].playerName + " : " + ranking[i].playerScore.ToString() + " pts.";
        }
    }
}
