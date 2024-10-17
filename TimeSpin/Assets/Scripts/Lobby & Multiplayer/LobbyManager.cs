using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;

    private Lobby _hostLobby; // Referencia a la sala creada (difiere de null sólo en el caso del host)
    private Lobby _joinedLobby; // Referencia a la sala a la que se ha unido
    public bool inLobby = false;
    // Nombre para la sala
    private string _lobbyName = "TimeSpin";
    // Se almacena el número máximo de jugadores de la sala
    private const int MAX_PLAYERS = 4;
    // Se almacena el número actual de jugadores en la sala
    public int NUM_PLAYERS_IN_LOBBY;
    // Variables encargadas de hacer una pulsación cada cierto tiempo, para que la sala no se destruya por inactividad
    private float _heartBeatLobbyTimer = 0;
    private const int MAX_HEARTBEAT_TIMER = 15;

    private void Awake()
    {
        instance = this;
    }

    private async void Start()
    {
        // Este código solo se ejecuta en el cliente
        if (Application.platform == RuntimePlatform.LinuxServer) return;

        // Se inicializan los servicios de Unity
        await UnityServices.InitializeAsync();
        // Se hace un registro anónimo
        // Se crea un evento para comprobar que se realiza dicho registro
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        // Mediante esta corrutina, se actualiza cada cierto tiempo el estado del lobby, para tener siempre la información correcta (por si alguien se conecta o desconecta)
        StartCoroutine(UpdateLobby());
    }

    private void Update()
    {
        // Este código solo se ejecuta en el cliente
        if (Application.platform == RuntimePlatform.LinuxServer) return;
        // Si estamos unidos a un lobby, cada cierto tiempo se la mandan pulsaciones
        if (_hostLobby != null)
        {
            HandleLobbyHeartbeat();
        }
    }

    #region Updates
    // Esta función se utiliza para enviar un mensaje al lobby cada 15 segundos, para evitar que la sala desaparezca por inactividad
    private async void HandleLobbyHeartbeat()
    {
        if (_hostLobby != null)
        {
            _heartBeatLobbyTimer += Time.deltaTime;
            // Si se supera el tiempo umbral sin enviar un mensaje, este se manda
            if (_heartBeatLobbyTimer > MAX_HEARTBEAT_TIMER)
            {
                _heartBeatLobbyTimer = 0;
                await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
            }
        }
    }

    // Función para actualizar continuamente el lobby
    private IEnumerator UpdateLobby()
    {
        while (true)
        {
            // Cada dos segundos, se actualiza el estado del lobby
            yield return new WaitForSeconds(2f);

            if (_joinedLobby != null)
            {
                // Se crea la tarea encargada de obtener el Lobby
                var tarea = GetLobby();
                // Mediante un delegado, se espera a que la tarea finalice para continuar
                yield return new WaitUntil(() => tarea.IsCompleted);
                // Se actualiza el número de jugadores en la sala
                if (_joinedLobby != null)
                {
                    NUM_PLAYERS_IN_LOBBY = _joinedLobby.Players.Count;
                }
            }
        }
    }
    public async Task GetLobby()
    {
        try
        {
            // Mediante el ID de la sala, se obtiene una referencia actualizada
            Lobby lobbyActualizado = await Lobbies.Instance.GetLobbyAsync(_joinedLobby.Id);
            _joinedLobby = lobbyActualizado;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    #endregion
    #region Create
    // Se utilizan corrutinas para realizar las esperas necesarias para que otras funcionen terminen de ejecutarse
    // Función asíncrona para crear la sala privada con un nombre y un número máximo de integrantes
    public void CreatePrivateLobby()
    {
        StartCoroutine(CreatePrivateLobbyCoroutine());
    }

    public IEnumerator CreatePrivateLobbyCoroutine()
    {
        // Se definen las propiedades de la sala
        CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
        {
            IsPrivate = true,
            Player = SelectionController.instance.GetPlayer()
        };
        // Se obtiene la tarea y se espera hasta que finaliza
        var createLobbyTask = LobbyService.Instance.CreateLobbyAsync(_lobbyName, MAX_PLAYERS, createLobbyOptions);
        yield return new WaitUntil(() => createLobbyTask.IsCompleted);

        if (createLobbyTask.Exception != null)
        {
            Debug.LogError("Error creating lobby: " + createLobbyTask.Exception);
            yield break;
        }

        _hostLobby = createLobbyTask.Result;
        _joinedLobby = _hostLobby;
        Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);

        // Llama a FirstServerJoin y espera a que termine
        bool serverFound = false;
        yield return StartCoroutine(MatchmakerManager.Instance.FirstServerJoinCoroutine((found) => serverFound = found));

        if (!serverFound)
        {
            Debug.Log("Server not found.");
            yield break;
        }

        // Se recibe la IP y el puerto del servidor para establecer la conexión
        string serverIP = MatchmakerManager.Instance.GetServerIP();
        ushort serverPort = MatchmakerManager.Instance.GetServerPort();

        // Se guarda dicha información en la sala
        Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
        {
            { "serverIP", new DataObject(DataObject.VisibilityOptions.Member, serverIP) },
            { "serverPort", new DataObject(DataObject.VisibilityOptions.Member, serverPort.ToString()) }
        };

        var updateLobbyTask = LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions { Data = lobbyData });
        yield return new WaitUntil(() => updateLobbyTask.IsCompleted);

        if (updateLobbyTask.Exception != null)
        {
            Debug.LogError("Error updating lobby: " + updateLobbyTask.Exception);
            yield break;
        }

        _joinedLobby = updateLobbyTask.Result;

        Debug.Log("Server information saved to lobby!");
        UI_Lobby.instance.EnterLobbyCode(_joinedLobby.LobbyCode);

        inLobby = true;
    }

    // Función asíncrona para crear una sala públic
    private void CreatePublicLobby()
    {
        // Se inicia la coroutine en lugar de async
        StartCoroutine(CreatePublicLobbyCoroutine());
    }

    public IEnumerator CreatePublicLobbyCoroutine()
    {
        // Se definen las propiedades de la sala
        CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
        {
            // La sala es pública
            IsPrivate = false,
            Player = SelectionController.instance.GetPlayer()
        };

        // Se espera la creación del lobby
        var createLobbyTask = LobbyService.Instance.CreateLobbyAsync(_lobbyName, MAX_PLAYERS, createLobbyOptions);
        yield return new WaitUntil(() => createLobbyTask.IsCompleted);

        _hostLobby = createLobbyTask.Result;
        _joinedLobby = _hostLobby;
        Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);

        // Llama a FirstServerJoin y espera a que termine
        bool serverFound = false;
        yield return StartCoroutine(MatchmakerManager.Instance.FirstServerJoinCoroutine((found) => serverFound = found));

        if (!serverFound)
        {
            Debug.Log("Server not found.");
            yield break;
        }

        // Una vez activo, se recibe la IP y el puerto del servidor para establecer la conexión
        string serverIP = MatchmakerManager.Instance.GetServerIP();
        ushort serverPort = MatchmakerManager.Instance.GetServerPort();

        // Se guarda dicha información en la sala
        Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
    {
        { "serverIP", new DataObject(DataObject.VisibilityOptions.Member, serverIP) },
        { "serverPort", new DataObject(DataObject.VisibilityOptions.Member, serverPort.ToString()) }
    };

        // Se actualiza el lobby con la IP y el puerto
        var updateLobbyTask = LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions { Data = lobbyData });
        yield return new WaitUntil(() => updateLobbyTask.IsCompleted);

        _joinedLobby = updateLobbyTask.Result;
        Debug.Log("Server information saved to lobby!");

        inLobby = true;
    }
    #endregion
    #region Join
    public IEnumerator JoinLobbyByCodeCoroutine(string lobbyCode)
    {
        // Se crea un jugador con las características correspondientes
        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
        {
            Player = SelectionController.instance.GetPlayer()
        };

        // Se espera a que la operación de unirse al lobby se complete
        var joinLobbyTask = Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
        yield return new WaitUntil(() => joinLobbyTask.IsCompleted);

        // Si ocurrió un error
        if (joinLobbyTask.IsFaulted)
        {
            // Recorremos las excepciones agregadas en la tarea
            foreach (var exception in joinLobbyTask.Exception.InnerExceptions)
            {
                // Verificamos si la excepción es del tipo LobbyServiceException
                if (exception is LobbyServiceException lobbyException)
                {
                    // Gestión de excepciones para mostrar en la UI
                    if (lobbyException.Reason == LobbyExceptionReason.LobbyNotFound)
                    {
                        Debug.Log("No se ha encontrado ninguna sala con dicho código");
                    }
                    else if (lobbyException.Reason == LobbyExceptionReason.LobbyFull)
                    {
                        Debug.Log("La sala a la que se intenta acceder está llena");
                    }
                }
            }
            yield break; // Salir de la coroutine si hay un error
        }

        // Si el lobby fue encontrado
        _joinedLobby = joinLobbyTask.Result;
        Debug.Log("Joined Lobby with code: " + lobbyCode);

        // Una vez se une al lobby, se obtienen la IP y el servidor
        string serverIP = _joinedLobby.Data["serverIP"].Value;
        string serverPort = _joinedLobby.Data["serverPort"].Value;

        // Se une al servidor
        MultiplayManager.Instance.JoinToServer(serverIP, serverPort);

        inLobby = true;
    }

    public IEnumerator JoinPublicLobbyCoroutine()
    {
        // Se crea un jugador con las características correspondientes
        QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
        {
            Player = SelectionController.instance.GetPlayer()
        };

        // Se intenta unir a una sala disponible
        var joinLobbyTask = LobbyService.Instance.QuickJoinLobbyAsync(options);
        yield return new WaitUntil(() => joinLobbyTask.IsCompleted);

        // Si ocurrió un error
        if (joinLobbyTask.IsFaulted)
        {
            // Recorremos las excepciones agregadas en la tarea
            foreach (var exception in joinLobbyTask.Exception.InnerExceptions)
            {
                // Verificamos si la excepción es del tipo LobbyServiceException
                if (exception is LobbyServiceException)
                {
                    Debug.Log("No hay ninguna sala activa, se creará una nueva");
                    CreatePublicLobby(); // Se crea una sala pública nueva
                }
            }
            yield break; // Salir de la coroutine si hay un error
        }

        // Si el lobby fue encontrado
        _joinedLobby = joinLobbyTask.Result;
        Debug.Log("Joined public lobby!");

        // Una vez se une al lobby, se obtienen la IP y el servidor
        string serverIP = _joinedLobby.Data["serverIP"].Value;
        string serverPort = _joinedLobby.Data["serverPort"].Value;

        // Se une al servidor
        MultiplayManager.Instance.JoinToServer(serverIP, serverPort);

        inLobby = true;
    }
    #endregion
    #region Leave
    // Con esta función, el jugador abandona la sala
    public void LeaveLobby()
    {
        // Inicia la coroutine en lugar de usar async
        StartCoroutine(LeaveLobbyCoroutine());
    }

    private IEnumerator LeaveLobbyCoroutine()
    {
        // Se realiza la petición para remover al jugador del lobby
        var leaveLobbyTask = LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        yield return new WaitUntil(() => leaveLobbyTask.IsCompleted);

        // Si ocurrió un error
        if (leaveLobbyTask.IsFaulted)
        {
            // Recorremos las excepciones agregadas en la tarea
            foreach (var exception in leaveLobbyTask.Exception.InnerExceptions)
            {
                // Verificamos si la excepción es del tipo LobbyServiceException
                if (exception is LobbyServiceException lobbyException)
                {
                    Debug.Log(lobbyException);
                }
            }
            yield break; // Salir de la coroutine si hay un error
        }

        // Si la tarea fue exitosa
        Debug.Log("Jugador ha abandonado la sala");

        // Se limpian todas las referencias del código
        _joinedLobby = null;
        _hostLobby = null;
        inLobby = false;
    }
    #endregion
    #region Data

    // Función para obtener los datos de los jugadores en el lobby y poder mostrarlos
    public Dictionary<string, List<string>> GetPlayersInLobby()
    {
        // Se crea un diccionario de listas de strings, para almacenar los nombres y los personajes escogidos por los jugadores
        Dictionary<string, List<string>> dataPlayers = new Dictionary<string, List<string>>();
        // Se crea la lista de personajes y se van añadiendo los personajes elegidos de cada uno de los jugadores
        List<string> personajes = new List<string>();
        foreach (Player p in _joinedLobby.Players)
        {
            personajes.Add(p.Data["Character"].Value);
        }
        // Se añade la lista al diccionario
        dataPlayers.Add("Characters", personajes);
        // Se crea la lista de nombres y se van añadiendo los nombres de cada uno de los jugadores
        List<string> nombres = new List<string>();
        foreach (Player p in _joinedLobby.Players)
        {
            nombres.Add(p.Data["Name"].Value);
        }
        // Se añade la lista al diccionario
        dataPlayers.Add("Names", nombres);

        return dataPlayers;
    }

    // Se obtienen los jugadores del lobby y se muestran por pantalla
    public void PrintPlayers()
    {
        Debug.Log("Players in lobby ------ ");
        foreach (Player p in _joinedLobby.Players)
        {
            Debug.Log(p.Id + " " + p.Data["Name"].Value + " " + p.Data["Character"].Value);
        }
    }

    // Se obtiene el código del lobby
    public string GetLobbyCode()
    {
        return _joinedLobby.LobbyCode;
    }
    #endregion

}
