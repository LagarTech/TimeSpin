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

    // Funci�n para introducir el c�digo de la sala a la que se quiere unir
    public void EnterLobbyCode(string lobbyCode)
    {
        _lobbyCode = lobbyCode;
    }

    // Funci�n para crear una sala, que accede a la instancia del manejador del LobbyManager
    public async void CreateLobbyButton()
    {
        // Primero se espera a crear el lobby
        await LobbyManager.instance.CreatePrivateLobby();
        // Despu�s se oculta el men�
        UIController.instance.OcultarMenu();
        // Tras ello se coloca el c�digo del lobby en la interfaz
        _lobbyCodeText.text = "Clave sala: " + _lobbyCode;
    }

    // Funci�n para unirse a una sala, que accede a la instancia del manejador del LobbyManager
    public async void JoinLobbyButton()
    {
        // Primero se espera a unirse al lobby
        await LobbyManager.instance.JoinLobbyByCode(_lobbyCode);
        // Despu�s se oculta el men�
        UIController.instance.OcultarMenu();
        // Tras ello se coloca el c�digo del lobby en la interfaz
        _lobbyCodeText.text = "Clave sala: " + _lobbyCode;
    }

}
