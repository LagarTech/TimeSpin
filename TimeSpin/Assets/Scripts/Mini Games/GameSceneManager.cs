using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager instance;

    [SerializeField] private GameObject[] _playersList;

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
}
