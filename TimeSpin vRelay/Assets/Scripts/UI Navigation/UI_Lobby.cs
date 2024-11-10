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

    [SerializeField] private GameObject _errorMessage;
    [SerializeField] private GameObject _waitingMessage;
    [SerializeField] private GameObject _joiningErrorMessage;

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

    // Función para crear una sala para jugar una partida
    public void CreateGameButton()
    {
        // Inicia la coroutine en lugar de usar async
        StartCoroutine(CreateGameCoroutine());
    }

    private IEnumerator CreateGameCoroutine()
    {
        HideMessages();
        _waitingMessage.SetActive(true); // Se muestra el mensaje de espera
        bool lobbyCreated = false;

        // Primero se espera a que se asigne a un servidor, y establezca la conexión
        yield return StartCoroutine(LobbyManager.instance.CreatePrivateGameCoroutine((success) => lobbyCreated = success));

        _waitingMessage.SetActive(true); // Se oculta el mensaje de espera

        // Si no se ha encontrado servidor, mostrar mensaje de error en la UI
        if (!lobbyCreated)
        {
            Debug.Log("Lobby creation failed, no server found.");
            _errorMessage.SetActive(true); // Se muestra el mensaje de error
            yield break;
        }

        // Después se oculta el menú
        UI_Controller.instance.OcultarMenu();

        // Tras ello se coloca el código del lobby en la interfaz
        _lobbyCodeText.text = "Clave sala: " + LobbyManager.instance.GetLobbyCode(); // Asumiendo que hay un método para obtener el código del lobby
    }

    // Función para unirse a una sala, que accede a la instancia del manejador del LobbyManager
    public void JoinGameButton()
    {
        // Inicia la coroutine en lugar de usar async
        StartCoroutine(JoinGameCoroutine());
    }

    private IEnumerator JoinGameCoroutine()
    {
        HideMessages();
        bool success = false;

        // Primero se espera a unirse al lobby y se almacena el resultado en `success`
        yield return StartCoroutine(LobbyManager.instance.JoinGameByCodeCoroutine(_lobbyCode, (result) => success = result));

        // Si no se pudo unir al lobby, se detiene el proceso
        if (!success)
        {
            Debug.Log("Failed to join the lobby.");
            _joiningErrorMessage.SetActive(true);
            yield break;
        }

        // Después se oculta el menú si el proceso fue exitoso
        UI_Controller.instance.OcultarMenu();

        // Tras ello se coloca el código del lobby en la interfaz
        _lobbyCodeText.text = "Clave sala: " + _lobbyCode;
    }

    public void HideMessages()
    {
        _joiningErrorMessage.SetActive(false);
        _errorMessage.SetActive(false);
    }

    public void ShowLobbyCode(string code)
    {
        _lobbyCodeText.text = "Clave sala: " + code;
    }

}
