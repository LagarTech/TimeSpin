using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class StartingManager : MonoBehaviour
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
        // En el cliente, se actualiza el texto con el número de jugadores conectados
        if (NetworkManager.Singleton.IsClient && Application.platform != RuntimePlatform.LinuxServer)
        {
            if (!LobbyManager.instance.inLobby) return;
            _numPlayers = LobbyManager.instance.NUM_PLAYERS_IN_LOBBY;
            if(_numPlayers >= _requiredPlayers)
            {
                _startGameButton.SetActive(true);
            }
            else
            {
                _startGameButton.SetActive(false);
            }
             _numPlayersText.text = _numPlayers.ToString() + "/4";
        }
        else
        {
            if(NetworkManager.Singleton.IsServer)
            {
                _numPlayers = NetworkManager.Singleton.ConnectedClients.Count;
            }
            // En el servidor, se comprueba si todos los jugadores están listos
            if (_numPlayersReady == _numPlayers)
            {
                // Se avisa a los clientes para empezar el primer minijuego
                StartGameClientRpc();
            }
        }
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
    }

}
