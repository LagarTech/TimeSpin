using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerRegister : NetworkBehaviour
{
    public static PlayerRegister instance;

    private void Awake ()
    {
       if(instance == null)
        {
            instance = this;
        }
       else
        {
            Destroy(gameObject);
        }
    }

    // REGISTRO DE JUGADORES
    // Lista que almacena la información de los jugadores que se unen al Lobby
    [SerializeField] private List<PlayerData> _joinedPlayers = new List<PlayerData>();
    // Indice de los jugadores
    private int _playerID = 0;

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc(int character, string name)
    {
        Debug.Log("Jugador registrado");
        // Se crea un campo de datos con la información del jugador y se añade al registro
        PlayerData newPlayer = new PlayerData(character, name, _playerID);
        _joinedPlayers.Add(newPlayer);
        _playerID++;
        // Se muestran dichos datos en las partidas de los clientes
        SetupPlayerClientRpc(newPlayer.ClientId, newPlayer.PlayerName, newPlayer.CharacterSelected);
    }

    [ClientRpc]
    private void SetupPlayerClientRpc(int clientID, string name, int character)
    {
        // Se buscan todos los objetos con la etiqueta Player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        // Se busca el player con el ID correcto
        foreach (GameObject player in players)
        {
            PlayerSetup playerSetup = player.GetComponent<PlayerSetup>();
            if (playerSetup.playerID == clientID)
            {
                // Se establecen sus datos de partida
                playerSetup.NamePlayer(name);
                playerSetup.CharacterPlayer(character);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void GetPlayerServerRpc(int clientID)
    {
        foreach(var player in _joinedPlayers)
        {
            if(player.ClientId == clientID)
            {
                SetupPlayerClientRpc(player.ClientId, player.PlayerName, player.CharacterSelected);
            }
        }
    }

}
