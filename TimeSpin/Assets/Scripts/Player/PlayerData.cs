using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    // Esta clase almacena los datos de un jugador, al registrarse al lobby
    public int CharacterSelected;   // Personaje seleccionado por el jugador
    public string PlayerName;      // Nombre del jugador
    public int ClientId;          // ID del cliente

    public PlayerData(int characterSelected, string playerName, int clientId)
    {
        CharacterSelected = characterSelected;
        PlayerName = playerName;
        ClientId = clientId;
    }
}
