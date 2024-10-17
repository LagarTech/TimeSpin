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
    public void CreateLobbyButton()
    {
        // Inicia la coroutine en lugar de usar async
        StartCoroutine(CreateLobbyCoroutine());
    }

    private IEnumerator CreateLobbyCoroutine()
    {
        // Primero se espera a crear el lobby
        yield return StartCoroutine(LobbyManager.instance.CreatePrivateLobbyCoroutine());

        // Despu�s se oculta el men�
        UIController.instance.OcultarMenu();

        // Tras ello se coloca el c�digo del lobby en la interfaz
        _lobbyCodeText.text = "Clave sala: " + LobbyManager.instance.GetLobbyCode(); // Asumiendo que hay un m�todo para obtener el c�digo del lobby
    }

    // Funci�n para unirse a una sala, que accede a la instancia del manejador del LobbyManager
    public void JoinLobbyButton()
    {
        // Inicia la coroutine en lugar de usar async
        StartCoroutine(JoinLobbyCoroutine());
    }

    private IEnumerator JoinLobbyCoroutine()
    {
        // Primero se espera a unirse al lobby
        yield return StartCoroutine(LobbyManager.instance.JoinLobbyByCodeCoroutine(_lobbyCode));

        // Despu�s se oculta el men�
        UIController.instance.OcultarMenu();

        // Tras ello se coloca el c�digo del lobby en la interfaz
        _lobbyCodeText.text = "Clave sala: " + _lobbyCode;
    }

}
