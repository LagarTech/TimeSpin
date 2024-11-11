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
    // Se almacena el número máximo de jugadores de la sala
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
        // Este código solo se ejecuta en el cliente
        if (Application.platform == RuntimePlatform.LinuxServer) return;
        // Si el juego ya ha comenzado, no se vuelve a hacer un registro
        if (!GameSceneManager.instance.gameStarted)
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
        }
    }

    // Se utilizan corrutinas para realizar las esperas necesarias para que otras funcionen terminen de ejecutarse
    // Función asíncrona para crear la sala privada con un máximo de jugadores

    public IEnumerator CreatePrivateGameCoroutine(System.Action<bool> onComplete)
    {
        // Se crea un lugar al que se pueden conectar los jugadores. Se reserva espacio para uno menos, ya que el host no cuenta
        string nearestAllocation = "europe-west4";
        var createAllocationTask = RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS - 1, nearestAllocation);
        yield return new WaitUntil(() => createAllocationTask.IsCompleted);

        // Verificación de errores en la creación de la asignación
        if (createAllocationTask.IsFaulted)
        {
            Debug.LogError("Error creating Relay allocation: " + createAllocationTask.Exception?.Message);
            onComplete(false);
            yield break;
        }

        Allocation allocation = createAllocationTask.Result;
        Debug.Log(allocation.Region);
        // Se debe obtener el código de relay para que se puedan unir los demás jugadores
        var getCodeTask = RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        yield return new WaitUntil(() => getCodeTask.IsCompleted);

        // Verificación de errores al obtener el código de unión
        if (getCodeTask.IsFaulted)
        {
            Debug.LogError("Error getting Relay join code: " + getCodeTask.Exception?.Message);
            onComplete(false);
            yield break;
        }

        _lobbyCode = getCodeTask.Result;

        // Se crea información para el servidor, con el protocolo seguro de WebSockets
        RelayServerData relayServerData = new RelayServerData(allocation, "wss");

        // Se establece dicha información en el protocolo de transporte
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        // Se inicia como Host
        NetworkManager.Singleton.StartHost();

        // Se muestra el código del Relay en la interfaz
        UI_Lobby.instance.EnterLobbyCode(_lobbyCode);

        inLobby = true; // Se indica que se está conectado
        onComplete(true); // Indicar éxito
    }

    // Función para unirse mediante el código
    public IEnumerator JoinGameByCodeCoroutine(string lobbyCode, System.Action<bool> onComplete)
    {
        // Se obtiene una referencia de la ubicación reservada mediante el código introducido
        var joinAllocationTask = RelayService.Instance.JoinAllocationAsync(lobbyCode);
        yield return new WaitUntil(() => joinAllocationTask.IsCompleted);

        // Verificación de errores en la asignación de unión
        if (joinAllocationTask.IsFaulted)
        {
            Debug.LogError("Error joining Relay allocation: " + joinAllocationTask.Exception?.Message);
            onComplete(false);
            yield break;
        }

        JoinAllocation joinAllocation = joinAllocationTask.Result;

        // Se obtiene la información del servidor, utilizando la localización reservada
        RelayServerData relayServerData = new RelayServerData(joinAllocation, "wss");

        // Se establece dicha información en el protocolo de transporte
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        // Se comienza el juego como cliente
        NetworkManager.Singleton.StartClient();

        // Se indica que se ha realizado la función con éxito
        onComplete(true);
    }

    // Se obtiene el código del lobby
    public string GetLobbyCode()
    {
        return _lobbyCode;
    }

}
