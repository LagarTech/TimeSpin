using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Lobby : MonoBehaviour
{
    private string _lobbyCode;

    // Funci�n para introducir el c�digo de la sala a la que se quiere unir
    public void EnterLobbyCode(string lobbyCode)
    {
        _lobbyCode = lobbyCode;
    }

    // Funci�n para crear una sala, que accede a la instancia del manejador del LobbyManager
    public void CreateLobbyButton()
    {
        LobbyManager.instance.CreatePrivateLobby();
    }

    // Funci�n para unirse a una sala, que accede a la instancia del manejador del LobbyManager
    public async void JoinLobbyButton()
    {
        await LobbyManager.instance.JoinLobbyByCode(_lobbyCode);
    }

}
