using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager instance;

    private Lobby _hostLobby; // Referencia a la sala creada (difiere de null sólo en el caso del host)
    private Lobby _joinedLobby; // Referencia a la sala a la que se ha unido
    private string _lobbyCode;
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

    private void Update()
    {
        // Este código solo se ejecuta en el cliente
        if (Application.platform == RuntimePlatform.LinuxServer) return;
        // Si estamos unidos a un lobby, cada cierto tiempo se la mandan pulsaciones
        if (_hostLobby != null)
        {
            StartCoroutine(HandleLobbyHeartbeatCoroutine());
        }
    }

    #region Updates
    // Esta función se utiliza para enviar un mensaje al lobby cada 15 segundos, para evitar que la sala desaparezca por inactividad
    private IEnumerator HandleLobbyHeartbeatCoroutine()
    {
        if (_hostLobby != null)
        {
            _heartBeatLobbyTimer += Time.deltaTime;

            // Si se supera el tiempo umbral, se envía el heartbeat
            if (_heartBeatLobbyTimer > MAX_HEARTBEAT_TIMER)
            {
                _heartBeatLobbyTimer = 0;
                var sendHeartbeatTask = LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);

                // Espera hasta que la tarea esté completa
                yield return new WaitUntil(() => sendHeartbeatTask.IsCompleted);

                if (sendHeartbeatTask.Exception != null)
                {
                    Debug.LogError("Error sending heartbeat: " + sendHeartbeatTask.Exception);
                }
            }
        }
    }

    public IEnumerator GetLobbyCoroutine()
    {
        // Se obtiene la tarea para obtener la sala actualizada
        var getLobbyTask = Lobbies.Instance.GetLobbyAsync(_joinedLobby.Id);

        // Se espera hasta que la tarea esté completa
        yield return new WaitUntil(() => getLobbyTask.IsCompleted);

        // Se gestiona la excepción en caso de que ocurra
        if (getLobbyTask.Exception != null)
        {
            Debug.LogError("Error fetching lobby: " + getLobbyTask.Exception);
            yield break; // Salimos de la corrutina en caso de error
        }

        // Si todo va bien, se actualiza la referencia al lobby
        _joinedLobby = getLobbyTask.Result;
    }

    #endregion
    #region Create
    // Se utilizan corrutinas para realizar las esperas necesarias para que otras funcionen terminen de ejecutarse
    // Función asíncrona para crear la sala privada con un nombre y un número máximo de integrantes

    public IEnumerator CreatePrivateLobbyCoroutine(System.Action<bool> onComplete)
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
            onComplete(false); // Indicar fallo en la creación del lobby
            yield break;
        }

        _hostLobby = createLobbyTask.Result;
        _joinedLobby = _hostLobby;
        Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);
        _lobbyCode = _joinedLobby.LobbyCode;


        // Llama a FirstServerJoin y espera a que termine
        bool serverFound = false;
        yield return StartCoroutine(MatchmakerManager.Instance.FirstServerJoinCoroutine((found) => serverFound = found));


        if (!serverFound)
        {
            Debug.Log("Server not found.");
            onComplete(false); // Indicar fallo si no se encuentra servidor
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
            onComplete(false); // Indicar fallo si no se puede actualizar el lobby
            yield break;
        }

        _joinedLobby = updateLobbyTask.Result;

        Debug.Log("Server information saved to lobby!");
        UI_Lobby.instance.EnterLobbyCode(_joinedLobby.LobbyCode);

        inLobby = true;
        onComplete(true); // Indicar éxito
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
        _lobbyCode = _joinedLobby.LobbyCode;
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
    public IEnumerator JoinLobbyByCodeCoroutine(string lobbyCode, System.Action<bool> onComplete)
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
            onComplete(false); // Indicar fallo si ocurre un error
            yield break; // Salir de la coroutine si hay un error
        }

        // Si el lobby fue encontrado
        _joinedLobby = joinLobbyTask.Result;
        _lobbyCode = _joinedLobby.LobbyCode;
        Debug.Log("Joined Lobby with code: " + lobbyCode);

        // Una vez se une al lobby, se obtienen la IP y el puerto del servidor
        string serverIP = _joinedLobby.Data["serverIP"].Value;
        string serverPort = _joinedLobby.Data["serverPort"].Value;

        // Se une al servidor
        MultiplayManager.Instance.JoinToServer(serverIP, serverPort);

        inLobby = true;
        onComplete(true); // Indicar éxito

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
                    StartCoroutine(CreatePublicLobbyCoroutine()); // Se crea una sala pública nueva
                }
            }
            yield break; // Salir de la coroutine si hay un error
        }

        // Si el lobby fue encontrado
        _joinedLobby = joinLobbyTask.Result;
        _lobbyCode = _joinedLobby.LobbyCode;
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

    // Se obtiene el código del lobby
    public string GetLobbyCode()
    {
        return _lobbyCode;
    }
    #endregion

}
