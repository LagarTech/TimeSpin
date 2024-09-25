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
    // Nombre para la sala
    private string _lobbyName = "TimeSpin";
    // Se almacena el número máximo de jugadores de la sala
    private const int MAX_PLAYERS = 4;
    // Se almacena el número actual de jugadores en la sala
    [SerializeField] private int NUM_PLAYERS_IN_LOBBY;
    // Variables encargadas de hacer una pulsación cada cierto tiempo, para que la sala no se destruya por inactividad
    private float _heartBeatLobbyTimer = 0;
    private const int MAX_HEARTBEAT_TIMER = 15;
    // Variables que almacenan la clave del relay creado por el host
    private string _relayCode;
    private string KEY_START_GAME = "relayCode"; // Key para el diccionario

    private void Awake()
    {
        instance = this;
    }

    private async void Start()
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
        // Mediante esta corrutina, se actualiza cada cierto tiempo el estado del lobby, para tener siempre la información correcta (por si alguien se conecta o desconecta)
        StartCoroutine(UpdateLobby());
    }

    private void Update()
    {
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
            // Cada segundo, se actualiza el estado del lobby
            yield return new WaitForSeconds(1f);

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
    private async Task GetLobby()
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
    // Función asíncrona para crear la sala privada con un nombre y un número máximo de integrantes
    public async void CreatePrivateLobby()
    {
        try
        {
            // Se definen las propiedades de la sala
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                // La sala es privada, es decir, solo se puede unir mediante código
                IsPrivate = true,
                Player = SelectionController.instance.GetPlayer()
            };

            _hostLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, MAX_PLAYERS, createLobbyOptions);
            _joinedLobby = _hostLobby;
            Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);
            // Una vez creada la sala, se inicia el relay para el host, para que pueda visualizar a su personaje en la sala
            StartHostGame();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    // Función asíncrona para crear una sala públic
    private async void CreatePublicLobby()
    {
        try
        {
            // Se definen las propiedades de la sala
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                // La sala es privada, es decir, solo se puede unir mediante código
                IsPrivate = false,
                Player = SelectionController.instance.GetPlayer()
            };

            _hostLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, MAX_PLAYERS, createLobbyOptions);
            _joinedLobby = _hostLobby;
            Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);
            // Una vez creada la sala, se inicia el relay para el host, para que pueda visualizar a su personaje en la sala
            StartHostGame();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    #endregion
    #region Join
    // Función asíncrona para unirse a una sala privada mediante código
    public async Task JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            // Se crea un jugador con las características correspondientes
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = SelectionController.instance.GetPlayer()
            };
            _joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            Debug.Log("Joined Lobby with code: " + lobbyCode);
            // Una vez se ha unido a la sala, se une al cliente al relay creado por el host, para que pueda visualizar a su personaje en la sala
            StartClientGame();
        }
        catch (LobbyServiceException e)
        {
            // Se gestionan las excepciones que pueden ocurrir, para mostrarlas como texto en la UI
            // En caso de que el código sea erróneo
            if (e.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                Debug.Log("No se ha encontrado ninguna sala con dicho código");
            }
            // En caso de que la sala ya tenga su máximo de 8 participantes
            else if (e.Reason == LobbyExceptionReason.LobbyFull)
            {
                Debug.Log("La sala a la que se intenta acceder está llena");
            }
        }
    }

    // Función asíncrona para unirse a una sala pública cualquiera
    public async Task JoinPublicLobby()
    {
        try
        {
            // Se crea un jugador con las características correspondientes
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
            {
                Player = SelectionController.instance.GetPlayer()
            };
            // Se intenta unir a una sala disponible
            _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            // Una vez se ha unido a la sala, se une al cliente al relay creado por el host, para que pueda visualizar a su personaje en la sala
            StartClientGame();
        }
        catch (LobbyServiceException e)
        {
            // Si no lo consigue, creará una sala pública nueva
            Debug.Log("No hay ninguna sala activa, se creará una nueva");
            CreatePublicLobby();
        }
    }
    #endregion
    #region Leave
    // Con esta función, el jugador abandona la sala
    public async Task LeaveLobby()
    {
        try
        {
            // Se limpian todas las referencias del código
            await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            Debug.Log("Jugador ha abandonado la sala");
            _joinedLobby = null;
            _hostLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    #endregion
    #region Debug
    // Se obtienen los jugadores del lobby y se muestran por pantalla
    private void PrintPlayers(Lobby l)
    {
        Debug.Log("Players in lobby ------ ");
        foreach (Player p in l.Players)
        {
            Debug.Log(p.Id + " " + p.Data["Name"].Value + " " + p.Data["Character"].Value);
        }
    }

    // Función asíncrona para listar todas las lobbies existentes
    private async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    #endregion
    #region Start

    private async void StartHostGame()
    {
        /*
        // Tras crear la sala, aparecerá se creará el punto de conexión para los demás jugadores y aparecerán sus personajes
        // Se espera a que se cree para continuar
        await RelayManager.instance.CreateRelay(MAX_PLAYERS);
        // Una vez creado se obtiene la clave
        _relayCode = RelayManager.instance.GetJoinCode();
        // Se actualiza el lobby con dicha clave, para que al intentar iniciar, los jugadores no necesiten introducir este código
        Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(_joinedLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject> {
                    {
                        KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, _relayCode)
                    } }
        });
        // Se actualiza la referencia del lobby actual
        _joinedLobby = lobby;
        // Una vez hecho, se inicia el Host
        NetworkManager.Singleton.StartHost();
        */
    }

    private void StartClientGame()
    {
        /*
        // Comienza el juego como un cliente
        // Primero debe de unirse al Relay que haya creado el Host
        // Para hacerlo, primero debe conseguir la clave del Relay
        _relayCode = _joinedLobby.Data[KEY_START_GAME].Value;
        // Después, se utiliza para unirse al Relay. Una vez hecho, se comienza el juego como cliente
        RelayManager.instance.JoinRelay(_relayCode);
        */
    }
    #endregion

}
