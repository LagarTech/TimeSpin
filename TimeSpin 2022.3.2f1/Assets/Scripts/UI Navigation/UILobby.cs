using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class UI_Lobby : MonoBehaviour
{
    public static UI_Lobby instance;

    private string _lobbyCode;
    [SerializeField] private TMP_Text _lobbyCodeText;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Función para introducir el código de la sala a la que se quiere unir
    public void EnterLobbyCode(string lobbyCode)
    {
        _lobbyCode = lobbyCode;
    }

    // Función para crear una sala, que accede a la instancia del manejador del LobbyManager
    public async void CreateLobbyButton()
    {
        // Primero se espera a crear el lobby
        await LobbyManager.instance.CreatePrivateLobby();
        // Después se oculta el menú
        UIController.instance.OcultarMenu();
        // Tras ello se coloca el código del lobby en la interfaz
        _lobbyCodeText.text = "Clave sala: " + _lobbyCode;
    }

    // Función para unirse a una sala, que accede a la instancia del manejador del LobbyManager
    public async void JoinLobbyButton()
    {
        // Primero se espera a unirse al lobby
        await LobbyManager.instance.JoinLobbyByCode(_lobbyCode);
        // Después se oculta el menú
        UIController.instance.OcultarMenu();
        // Tras ello se coloca el código del lobby en la interfaz
        _lobbyCodeText.text = "Clave sala: " + _lobbyCode;
    }

}
