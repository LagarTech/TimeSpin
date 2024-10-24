using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private List<GameObject> _characterList; // Lista que almacena todos los personajes seleccionables
    [SerializeField] private TMP_Text _playerName; // Nombre del jugador
    [SerializeField] private List<Vector3> _spawnPositionList; // Lista de posiciones donde comienza el jugador

    private Dictionary<string, List<string>> _playersData;
    private int _playerID;


    private void Start()
    {
        StartCoroutine(OnNetworkSpawnCoroutine());
    }

    // Corrutina que maneja la lógica de OnNetworkSpawn
    private IEnumerator OnNetworkSpawnCoroutine()
    {
        // Se obtiene el identificador del jugador, para poder establecer sus datos personalizados
        _playerID = (int)OwnerClientId - 1;

        // Este código solo se ejecuta en el servidor
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            // Se coloca al jugador en la parte correcta de la escena
            PositionPlayer();
            yield break; // Sale de la corrutina si está en el servidor
        }

        // Primero se obtienen los datos de los jugadores en el lobby
        // Se actualiza la referencia del lobby
        yield return StartCoroutine(LobbyManager.instance.GetLobbyCoroutine());

        _playersData = LobbyManager.instance.GetPlayersInLobby();

        // Se establece el nombre del jugador
        NamePlayer();

        // Se muestra su personaje escogido
        CharacterPlayer();
    }

    private void PositionPlayer()
    {
        transform.position = _spawnPositionList[_playerID];
    }

    private void NamePlayer()
    {
        _playersData.TryGetValue("Names", out List<string> playerNames);
        _playerName.text = playerNames[_playerID];
        LobbyManager.instance.PrintPlayers();
    }

    private void CharacterPlayer()
    {
        // Por si acaso, primero se ocultan todos los modelos de la lista
        foreach (GameObject p in _characterList)
        {
            p.SetActive(false);
        }
        // Después, se obtiene el personaje que se debe visibilizar
        _playersData.TryGetValue("Characters", out List<string> playerCharacters);
        int characterSelected = int.Parse(playerCharacters[_playerID]);
        _characterList[characterSelected].SetActive(true);
    }

   
}
