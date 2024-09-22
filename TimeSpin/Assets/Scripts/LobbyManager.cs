using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private Lobby _hostLobby; // Referencia a la sala creada (difiere de null s�lo en el caso del host)
    private Lobby _joinedLobby; // Referencia a la sala a la que se ha unido
    // Nombre para la sala
    private string _lobbyName = "TimeSpin";
    // Se almacena el n�mero m�ximo de jugadores de la sala
    private const int MAX_PLAYERS = 8;
    // Se almacena el n�mero actual de jugadores en la sala
    [SerializeField] private int NUM_PLAYERS_IN_LOBBY;
    // Variables encargadas de hacer una pulsaci�n cada cierto tiempo, para que la sala no se destruya por inactividad
    private float _heartBeatLobbyTimer = 0;
    private const int MAX_HEARTBEAT_TIMER = 15;

    private async void Start()
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
        // Mediante esta corrutina, se actualiza cada cierto tiempo el estado del lobby, para tener siempre la informaci�n correcta (por si alguien se conecta o desconecta)
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
    // Esta funci�n se utiliza para enviar un mensaje al lobby cada 15 segundos, para evitar que la sala desaparezca por inactividad
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

    // Funci�n para actualizar continuamente el lobby
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
                // Se actualiza el n�mero de jugadores en la sala
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
    // Funci�n as�ncrona para crear la sala privada con un nombre y un n�mero m�ximo de integrantes
    private async void CreatePrivateLobby()
    {
        try
        {
            // Se definen las propiedades de la sala
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                // La sala es privada, es decir, solo se puede unir mediante c�digo
                IsPrivate = true,
                Player = SelectionController.instance.GetPlayer()
            };

            _hostLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, MAX_PLAYERS, createLobbyOptions);
            _joinedLobby = _hostLobby;
            Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);
            // PrintPlayers(_hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    // Funci�n as�ncrona para crear una sala p�blic
    private async void CreatePublicLobby()
    {
        try
        {
            // Se definen las propiedades de la sala
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                // La sala es privada, es decir, solo se puede unir mediante c�digo
                IsPrivate = false,
                Player = SelectionController.instance.GetPlayer()
            };

            _hostLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, MAX_PLAYERS, createLobbyOptions);
            _joinedLobby = _hostLobby;
            Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);
            // PrintPlayers(_hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    #endregion
    #region Join
    // Funci�n as�ncrona para unirse a una sala privada mediante c�digo
    private async Task JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            // Se crea un jugador con las caracter�sticas correspondientes
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = SelectionController.instance.GetPlayer()
            };
            _joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            Debug.Log("Joined Lobby with code: " + lobbyCode);
        }
        catch (LobbyServiceException e)
        {
            // Se gestionan las excepciones que pueden ocurrir, para mostrarlas como texto en la UI
            // En caso de que el c�digo sea err�neo
            if (e.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                Debug.Log("No se ha encontrado ninguna sala con dicho c�digo");
            }
            // En caso de que la sala ya tenga su m�ximo de 8 participantes
            else if (e.Reason == LobbyExceptionReason.LobbyFull)
            {
                Debug.Log("La sala a la que se intenta acceder est� llena");
            }
        }
    }

    // Funci�n as�ncrona para unirse a una sala p�blica cualquiera
    private async Task JoinPublicLobby()
    {
        try
        {
            // Se crea un jugador con las caracter�sticas correspondientes
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
            {
                Player = SelectionController.instance.GetPlayer()
            };
            // Se intenta unir a una sala disponible
            _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
        }
        catch (LobbyServiceException e)
        {
            // Si no lo consigue, crear� una sala p�blica nueva
            Debug.Log("No hay ninguna sala activa, se crear� una nueva");
            CreatePublicLobby();
        }
    }
    #endregion
    #region Leave
    // Con esta funci�n, el jugador abandona la sala
    public async Task LeaveLobby()
    {
        try
        {
            // Se limpian todas las referencias del c�digo
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

    // Funci�n as�ncrona para listar todas las lobbies existentes
    private async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach(Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    #endregion

}
