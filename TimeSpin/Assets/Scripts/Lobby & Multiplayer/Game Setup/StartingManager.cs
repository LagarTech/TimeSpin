using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartingManager : NetworkBehaviour
{
    public static StartingManager instance;

    [SerializeField] private TMP_Text _numPlayersText;
    [SerializeField] private GameObject _startGameButton;
    [SerializeField] private GameObject _startedSelectionText;
    [SerializeField] private GameObject _waitingPlayersText;
    [SerializeField] private GameObject _informationText;
    [SerializeField] private GameObject _information2Text;
    [SerializeField] private GameObject _lobbyCodeText;
    [SerializeField] private GameObject _selectionServerButton;
    [SerializeField] private GameObject _selectionMenu;
    [SerializeField] private GameObject _waitingSelectionText;
    [SerializeField] private GameObject _rankingPanel;

    [SerializeField] private List<Button> _gamesButtons;

    private int _numPlayers;
    private bool _isReady = false;
    private const int _requiredPlayers = 2;

    // Esta variable sólo se maneja en el servidor
    private int _numPlayersReady = 0; // Número de jugadores que han indicado que están listos
    // Esta variable controla si todos los jugadores han indicado que están listos
    public bool startedSelection = false;

    // Control del minijuego escogido //
    [SerializeField] private int _selectedGame = 0;
    // Lista con el número de votos por minijuego
    private int[] _selectedGames;
    private int _numSelections; // Número de selecciones recibidas

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer && Application.platform == RuntimePlatform.LinuxServer)
        {
            _selectedGames = new int[5]; // Se inicializa la lista con 5 huecos, una para los votos de cada minijuego
            if(GameSceneManager.instance.gameStarted)
            {
                startedSelection = true; // Como el juego ya ha comenzado, el proceso de selección tambíen
            }
        }
        else
        {
            startedSelection = true;
            // Se prepara la UI para un regreso a la escena
            if (GameSceneManager.instance.gameStarted)
            {
                _startedSelectionText.SetActive(true);
                _numPlayersText.text = "";
                _waitingPlayersText.SetActive(false);
                _informationText.SetActive(false);
                _lobbyCodeText.SetActive(false);
                _rankingPanel.SetActive(true);
                GameSceneManager.instance.UpdateLobbyRanking();
            }
            _startGameButton.SetActive(false);
            _information2Text.SetActive(false);

            // Se bloquean los botones de los minijuegos que ya se han jugado
            // Para ello se hace una llamada al servidor, que es quien almacena la lista de juegos terminados
            BlockPlayedGamesButtonsServerRpc();
        }
    }

    private void Update()
    {
        UpdatePlayersCount();
    }

    [ServerRpc (RequireOwnership = false)]
    private void BlockPlayedGamesButtonsServerRpc()
    {
        bool[] gamesPlayed = GameSceneManager.instance.GetPlayedGames();
        BlockPlayedGamesButtonsClientRpc(gamesPlayed);
    }

    [ClientRpc]
    private void BlockPlayedGamesButtonsClientRpc(bool[] gamesPlayed)
    {
        for(int i = 0; i < gamesPlayed.Length; i++)
        {
            if (gamesPlayed[i])
            {
                BlockButton(_gamesButtons[i]);
            }
        }
    }

    private void BlockButton(Button button)
    {
        button.interactable = false;
        // Cambia el color para indicar que el botón está bloqueado
        ColorBlock colores = button.colors;
        colores.normalColor = Color.gray; // Cambia a un color gris
        button.colors = colores;
    }

    private void UpdatePlayersCount()
    {
        if (Application.platform != RuntimePlatform.LinuxServer) return;
        // En el servidor se calcula el número de jugadores que hay, y se informa al cliente
        if (NetworkManager.Singleton.IsServer)
        {
            _numPlayers = NetworkManager.Singleton.ConnectedClients.Count;
            if (startedSelection) return;
            UpdatePlayerCountClientRpc(_numPlayers);
        }
        // En el servidor, se comprueba si todos los jugadores están listos
        if (_numPlayersReady == _numPlayers && _numPlayers >= _requiredPlayers)
        {
            // Comienza la transición
            // Se indica que todos los jugadores están listos y que comienza el proceso de votación
            // Se inicializa la lista de jugadores para tener una referencia a cada uno entre escenas
            GameSceneManager.instance.InitializePlayersList();
            startedSelection = true;
            StartSelectionClientRpc();
        }
    }


    [ClientRpc]
    private void UpdatePlayerCountClientRpc(int playerCount)
    {
        if (!LobbyManager.instance.inLobby) return;
        _numPlayers = playerCount;

        if (_numPlayers >= _requiredPlayers)
        {
            _startGameButton.SetActive(true);
            _information2Text.SetActive(true);
        }
        else
        {
            _startGameButton.SetActive(false);
            _information2Text.SetActive(false);
        }

        // Actualiza el texto con la cuenta de jugadores
        _numPlayersText.text = _numPlayers.ToString() + "/4";
    }

    public void PlayerReady()
    {
        if (_isReady) return;
        // Llamada RPC para incrementar los jugadores listos en el servidor
        IncrementPlayersReadyServerRpc();
        _isReady = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncrementPlayersReadyServerRpc()
    {
        _numPlayersReady++;
    }

    [ClientRpc]
    private void StartSelectionClientRpc()
    {
        startedSelection = true;
        // Se indica que ya ha comenzado la votación y se oculta el botón de listo, junto con el número de jugadores y otros elementos de la UI
        _numPlayersText.text = "";
        _startedSelectionText.SetActive(true);
        _startGameButton.SetActive(false);
        _waitingPlayersText.SetActive(false);
        _informationText.SetActive(false);
        _information2Text.SetActive(false);
        _lobbyCodeText.SetActive(false);
        _rankingPanel.SetActive(true);
        GameSceneManager.instance.UpdateLobbyRanking();
    }

    public void ChooseGame(int game)
    {
        _selectedGame = game;
        _selectionServerButton.SetActive(true);
    }

    public void SendSelectionServer()
    {
        GameSelectionServerRpc(_selectedGame);
        _selectionMenu.SetActive(false);
        _waitingSelectionText.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GameSelectionServerRpc(int idGame)
    {
        _selectedGames[idGame]++; // Se aumenta en uno el número de votos del minijuego escogido
        _numSelections++; // Se incrementa el número de selecciones recibidas
        // Si ya se han recibido tantas selecciones como jugadores, se procede a determinar qué minijuego se va a jugar
        if(_numSelections == _numPlayers)
        {
            int bestGame = 0;
            // Se busca la posición con más elegidos, con un algoritmo de búsqueda del mejor candidato sencillo
            for (int i = 0; i < 4; i++)
            {
                if (_selectedGames[i] > _selectedGames[bestGame])
                {
                    bestGame = i;
                }
            }
            // En función del resultado, se transiciona a una escena u otra
            string nextGameScene = "";
            switch(bestGame)
            {
                case 0:
                    nextGameScene = "Prehistory";
                    break;
                case 1:
                    nextGameScene = "Egipt";
                    break;
                case 2:
                    nextGameScene = "Medieval";
                    break;
                case 3:
                    nextGameScene = "Maya";
                    break;
                case 4:
                    nextGameScene = "Future";
                    break;
            }
            // Se informa al gestor entre escenas el minijuego seleccionado, para que no se pueda volver a él
            GameSceneManager.instance.RegisterGameSelection(bestGame);
            StartCoroutine(LoadingScreenManager.instance.ServerSceneTransition(nextGameScene));
        }
    }
}
