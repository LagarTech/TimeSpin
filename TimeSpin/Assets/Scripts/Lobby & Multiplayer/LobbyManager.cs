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

    private Lobby _hostLobby; // Referencia a la sala creada (difiere de null s�lo en el caso del host)
    private Lobby _joinedLobby; // Referencia a la sala a la que se ha unido
    private string _lobbyCode;
    public bool inLobby = false;
    // Nombre para la sala
    private string _lobbyName = "TimeSpin";
    // Se almacena el n�mero m�ximo de jugadores de la sala
    private const int MAX_PLAYERS = 4;
    // Se almacena el n�mero actual de jugadores en la sala
    public int NUM_PLAYERS_IN_LOBBY;
    // Variables encargadas de hacer una pulsaci�n cada cierto tiempo, para que la sala no se destruya por inactividad
    private float _heartBeatLobbyTimer = 0;
    private const int MAX_HEARTBEAT_TIMER = 15;

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
    // Funci�n as�ncrona para crear la sala privada con un nombre y un n�mero m�ximo de integrantes

    public IEnumerator CreatePrivateGameCoroutine(System.Action<bool> onComplete)
    {
        // Llama a FirstServerJoin y espera a que termine
        bool serverFound = false;
        yield return StartCoroutine(MatchmakerManager.Instance.FirstServerJoinCoroutine((found) => serverFound = found));

        if (!serverFound)
        {
            Debug.Log("Server not found.");
            onComplete(false); // Indicar fallo si no se encuentra servidor
            yield break;
        }

        // Se recibe la IP y el puerto del servidor para establecer la conexi�n
        string serverIP = MatchmakerManager.Instance.GetServerIP();
        string serverPort = MatchmakerManager.Instance.GetServerPort().ToString();

        // Se utilizan la IP y el puerto para buscar el Lobby creado por el servidor y obtener el c�digo Relay
        // Se obtiene la lista de lobbies activas y se filtran
        QueryLobbiesOptions queryLobbyOptions = new QueryLobbiesOptions
        {
            Filters = new List<QueryFilter>
            {
                new QueryFilter(field: QueryFilter.FieldOptions.S1,
                                op: QueryFilter.OpOptions.EQ,
                                value: serverIP),
                new QueryFilter(field: QueryFilter.FieldOptions.S2,
                                op: QueryFilter.OpOptions.EQ,
                                value: serverPort)
            }
        };

        var listLobbiesTask = Lobbies.Instance.QueryLobbiesAsync(queryLobbyOptions);
        yield return new WaitUntil(() => listLobbiesTask.IsCompleted);
        QueryResponse queryResponse = listLobbiesTask.Result;

        // Desde que se crea la Lobby en el servidor, se retrasa una cantidad peque�a de tiempo hasta que la b�squeda la detecta
        // Por lo tanto, se repite el proceso cada segundo hasta que se encuentra

        while (queryResponse.Results.Count == 0)
        {
            yield return new WaitForSeconds(1f);
            listLobbiesTask = Lobbies.Instance.QueryLobbiesAsync(queryLobbyOptions);
            yield return new WaitUntil(() => listLobbiesTask.IsCompleted);
            queryResponse = listLobbiesTask.Result;
        }

        // S�lo se obtendr� un Lobby con las caracter�sticas dadas
        Lobby lobbyServer = queryResponse.Results[0];
        // Se obtiene el c�digo para unirse al servidor Relay
        lobbyServer.Data.TryGetValue("joinCode", out DataObject code);
        _lobbyCode = code.Value;

        // Finalmente se establece la conexi�n con el servidor Relay utilizando el c�digo obtenido
        // Se obtiene una referencia de la ubicaci�n reservada
        var joinAllocationTask = RelayService.Instance.JoinAllocationAsync(_lobbyCode);
        yield return new WaitUntil(() => joinAllocationTask.IsCompleted);
        JoinAllocation joinAllocation = joinAllocationTask.Result;
        // Se obtiene la informaci�n del servidor, utilizando la localizaci�n reservada
        RelayServerData relayServerData = new RelayServerData(joinAllocation, "wss");
        // Se establece dicha informaci�n en el protocolo de transporte
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        // Se comienza el juego como cliente
        NetworkManager.Singleton.StartClient();

        // Se muestra el c�digo en la interfaz
        UI_Lobby.instance.EnterLobbyCode(_lobbyCode);

        inLobby = true;
        onComplete(true); // Indicar �xito
    }

    public IEnumerator JoinGameByCodeCoroutine(string lobbyCode, System.Action<bool> onComplete)
    {
        // Se obtiene una referencia de la ubicaci�n reservada
        var joinAllocationTask = RelayService.Instance.JoinAllocationAsync(lobbyCode);
        yield return new WaitUntil(() => joinAllocationTask.IsCompleted);
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
