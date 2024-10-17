using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager: MonoBehaviour
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
    // Función asíncrona para crear la sala privada con un nombre y un número máximo de integrantes
    public async Task CreatePrivateLobby()
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

            // Una vez creada la sala, se hace una petición para iniciar un servidor de Multiplay
            await MatchmakerManager.Instance.FirstServerJoin();

            // Una vez activo, se recibe la IP y el puerto del servidor para establecer la conexión
            string serverIP = MatchmakerManager.Instance.GetServerIP();
            ushort serverPort = MatchmakerManager.Instance.GetServerPort();

            // Se guarda dicha información en la sala
            Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
            {
                { "serverIP", new DataObject(DataObject.VisibilityOptions.Member, serverIP) },
                { "serverPort", new DataObject(DataObject.VisibilityOptions.Member, serverPort.ToString()) }
            };

            // Actualizar el lobby con la IP y el puerto
            _joinedLobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
            {
                Data = lobbyData
            });

            Debug.Log("Server information saved to lobby!");

            // Se pasa el código del lobby a la UI para poder mostrarlo
            UI_Lobby.instance.EnterLobbyCode(_joinedLobby.LobbyCode);

            inLobby = true;

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

            // Una vez creada la sala, se hace una petición para iniciar un servidor de Multiplay
            await MatchmakerManager.Instance.FirstServerJoin();

            // Una vez activo, se recibe la IP y el puerto del servidor para establecer la conexión
            string serverIP = MatchmakerManager.Instance.GetServerIP();
            ushort serverPort = MatchmakerManager.Instance.GetServerPort();

            // Se guarda dicha información en la sala
            Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
            {
                { "serverIP", new DataObject(DataObject.VisibilityOptions.Member, serverIP) },
                { "serverPort", new DataObject(DataObject.VisibilityOptions.Member, serverPort.ToString()) }
            };

            // Actualizar el lobby con la IP y el puerto
            _joinedLobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
            {
                Data = lobbyData
            });

            Debug.Log("Server information saved to lobby!");

            inLobby = true;
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
            // Una vez se una al lobby, se trata de unir al servidor, con los datos almacenados
            // Se obtienen la IP y el servidor
            string serverIP = _joinedLobby.Data["serverIP"].Value;
            string serverPort = _joinedLobby.Data["serverPort"].Value;
            // Se une al servidor
            MultiplayManager.Instance.JoinToServer(serverIP, serverPort);

            inLobby = true;

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
            // Una vez se una al lobby, se trata de unir al servidor, con los datos almacenados
            // Se obtienen la IP y el servidor
            string serverIP = _joinedLobby.Data["serverIP"].Value;
            string serverPort = _joinedLobby.Data["serverPort"].Value;
            // Se une al servidor
            MultiplayManager.Instance.JoinToServer(serverIP, serverPort);

            inLobby = true;

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
            inLobby = false;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
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

}
