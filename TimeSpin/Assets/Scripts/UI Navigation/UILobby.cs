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
    public void CreateLobbyButton()
    {
        // Inicia la coroutine en lugar de usar async
        StartCoroutine(CreateLobbyCoroutine());
    }

    private IEnumerator CreateLobbyCoroutine()
    {
        // Primero se espera a crear el lobby
        yield return StartCoroutine(LobbyManager.instance.CreatePrivateLobbyCoroutine());

        // Después se oculta el menú
        UIController.instance.OcultarMenu();

        // Tras ello se coloca el código del lobby en la interfaz
        _lobbyCodeText.text = "Clave sala: " + LobbyManager.instance.GetLobbyCode(); // Asumiendo que hay un método para obtener el código del lobby
    }

    // Función para unirse a una sala, que accede a la instancia del manejador del LobbyManager
    public void JoinLobbyButton()
    {
        // Inicia la coroutine en lugar de usar async
        StartCoroutine(JoinLobbyCoroutine());
    }

    private IEnumerator JoinLobbyCoroutine()
    {
        // Primero se espera a unirse al lobby
        yield return StartCoroutine(LobbyManager.instance.JoinLobbyByCodeCoroutine(_lobbyCode));

        // Después se oculta el menú
        UIController.instance.OcultarMenu();

        // Tras ello se coloca el código del lobby en la interfaz
        _lobbyCodeText.text = "Clave sala: " + _lobbyCode;
    }

}
