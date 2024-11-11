using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager instance;

    private string _lobbyCode;
    public bool inLobby = false;
    // Se almacena el n�mero m�ximo de jugadores de la sala
    private const int MAX_PLAYERS = 4;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        // Se hace que el objeto navegue entre escenas y no se destruya
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        // Este c�digo solo se ejecuta en el cliente
        if (Application.platform == RuntimePlatform.LinuxServer) return;
        // Si el juego ya ha comenzado, no se vuelve a hacer un registro
        if (!GameSceneManager.instance.gameStarted)
        {
            // Se inicializan los servicios de Unity
            await UnityServices.InitializeAsync();
            // Se hace un registro an�nimo
            // Se crea un evento para comprobar que se realiza dicho registro
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    // Se utilizan corrutinas para realizar las esperas necesarias para que otras funcionen terminen de ejecutarse
    // Funci�n as�ncrona para crear la sala privada con un m�ximo de jugadores

    public IEnumerator CreatePrivateGameCoroutine(System.Action<bool> onComplete)
    {
        // Se crea un lugar al que se pueden conectar los jugadores. Se reserva espacio para uno menos, ya que el host no cuenta
        string nearestAllocation = "europe-west4";
        var createAllocationTask = RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS - 1, nearestAllocation);
        yield return new WaitUntil(() => createAllocationTask.IsCompleted);

        // Verificaci�n de errores en la creaci�n de la asignaci�n
        if (createAllocationTask.IsFaulted)
        {
            Debug.LogError("Error creating Relay allocation: " + createAllocationTask.Exception?.Message);
            onComplete(false);
            yield break;
        }

        Allocation allocation = createAllocationTask.Result;
        Debug.Log(allocation.Region);
        // Se debe obtener el c�digo de relay para que se puedan unir los dem�s jugadores
        var getCodeTask = RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        yield return new WaitUntil(() => getCodeTask.IsCompleted);

        // Verificaci�n de errores al obtener el c�digo de uni�n
        if (getCodeTask.IsFaulted)
        {
            Debug.LogError("Error getting Relay join code: " + getCodeTask.Exception?.Message);
            onComplete(false);
            yield break;
        }

        _lobbyCode = getCodeTask.Result;

        // Se crea informaci�n para el servidor, con el protocolo seguro de WebSockets
        RelayServerData relayServerData = new RelayServerData(allocation, "wss");

        // Se establece dicha informaci�n en el protocolo de transporte
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        // Se inicia como Host
        NetworkManager.Singleton.StartHost();

        // Se muestra el c�digo del Relay en la interfaz
        UI_Lobby.instance.EnterLobbyCode(_lobbyCode);

        inLobby = true; // Se indica que se est� conectado
        onComplete(true); // Indicar �xito
    }

    // Funci�n para unirse mediante el c�digo
    public IEnumerator JoinGameByCodeCoroutine(string lobbyCode, System.Action<bool> onComplete)
    {
        // Se obtiene una referencia de la ubicaci�n reservada mediante el c�digo introducido
        var joinAllocationTask = RelayService.Instance.JoinAllocationAsync(lobbyCode);
        yield return new WaitUntil(() => joinAllocationTask.IsCompleted);

        // Verificaci�n de errores en la asignaci�n de uni�n
        if (joinAllocationTask.IsFaulted)
        {
            Debug.LogError("Error joining Relay allocation: " + joinAllocationTask.Exception?.Message);
            onComplete(false);
            yield break;
        }

        JoinAllocation joinAllocation = joinAllocationTask.Result;

        // Se obtiene la informaci�n del servidor, utilizando la localizaci�n reservada
        RelayServerData relayServerData = new RelayServerData(joinAllocation, "wss");

        // Se establece dicha informaci�n en el protocolo de transporte
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        // Se comienza el juego como cliente
        NetworkManager.Singleton.StartClient();

        // Se indica que se ha realizado la funci�n con �xito
        onComplete(true);
    }

    // Se obtiene el c�digo del lobby
    public string GetLobbyCode()
    {
        return _lobbyCode;
    }

}
