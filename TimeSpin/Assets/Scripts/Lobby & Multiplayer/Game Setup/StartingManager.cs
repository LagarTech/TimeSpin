using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartingManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text _numPlayersText;
    [SerializeField] private GameObject _startGameButton;
    private int _numPlayers;
    private bool _isReady = false;
    private const int _requiredPlayers = 2;
    // Esta variable sólo se maneja en el servidor
    private int _numPlayersReady = 0;


    private void Start()
    {
        // Este código sólo se ejecuta en el cliente
        if (NetworkManager.Singleton.IsServer && Application.platform == RuntimePlatform.LinuxServer) return;
        _startGameButton.SetActive(false);
    }

    private void Update()
    {
        UpdatePlayersCount();
    }

    private void UpdatePlayersCount()
    {

        if (Application.platform != RuntimePlatform.LinuxServer) return;
        // En el servidor se calcula el número de jugadores que hay, y se informa al cliente
        if (NetworkManager.Singleton.IsServer)
        {
            _numPlayers = NetworkManager.Singleton.ConnectedClients.Count;
            UpdatePlayerCountClientRpc(_numPlayers);
        }
        // En el servidor, se comprueba si todos los jugadores están listos
        if (_numPlayersReady == _numPlayers && _numPlayers >= _requiredPlayers)
        {
            // Se avisa a los clientes para empezar el primer minijuego
            StartGameClientRpc();
            // Carga la escena del minijuego de Egipto y sincroniza con todos los clientes
            NetworkManager.Singleton.SceneManager.LoadScene("Egipt", LoadSceneMode.Single);
            // Se inicializa la lista de jugadores
            GameSceneManager.instance.InitializePlayersList();
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
        }
        else
        {
            _startGameButton.SetActive(false);
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
    private void StartGameClientRpc()
    {
        Debug.Log("Comenzando minijuego...");
        // Se inicializa la lista de jugadores
        GameSceneManager.instance.InitializePlayersList();
    }

}
