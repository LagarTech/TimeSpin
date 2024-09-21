using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private Lobby hostLobby;

    private async void Start()
    {
        // Se inicializan los servicios de Unity
        await UnityServices.InitializeAsync();
        // Se hace un registro anónimo
        // Se crea un evento para comprobar que se realiza dicho registro
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        CreateLobby();
    }


    // Función asíncrona para crear la sala con un nombre y un número máximo de integrantes
    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "TestLobby";
            int maxPlayers = 8;
            hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
