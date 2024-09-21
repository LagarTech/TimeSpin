using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private Lobby _hostLobby; // Referencia a la sala creada
    // Nombre para la sala
    private string _lobbyName = "TimeSpin";
    // Se almacena el número máximo de jugadores de la sala
    private const int MAX_PLAYERS = 8;
    // Variables encargadas de hacer una pulsación cada cierto tiempo, para que la sala no se destruya por inactividad
    private float _heartBeatLobbyTimer = 0;
    private const int MAX_HEARTBEAT_TIMER = 15;

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
        // CreatePrivateLobby();
        // await JoinPublicLobby();
    }

    private void Update()
    {
        // Si estamos unidos a un lobby, cada cierto tiempo se la mandan pulsaciones
        if (_hostLobby != null)
        {
            HandleLobbyHeartbeat();
        }
    }

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


    // Función asíncrona para crear la sala privada con un nombre y un número máximo de integrantes
    private async void CreatePrivateLobby()
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
            Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);
            // PrintPlayers(_hostLobby);
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
            Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);
            // PrintPlayers(_hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    // Función asíncrona para unirse a una sala privada mediante código
    private async Task JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            // Se crea un jugador con las características correspondientes
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = SelectionController.instance.GetPlayer()
            };
            await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            Debug.Log("Joined Lobby with code: " + lobbyCode);
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
    private async Task JoinPublicLobby()
    {
        try
        {
            // Se crea un jugador con las características correspondientes
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
            {
                Player = SelectionController.instance.GetPlayer()
            };
            // Se intenta unir a una sala disponible
            await LobbyService.Instance.QuickJoinLobbyAsync(options);
        }
        catch (LobbyServiceException e)
        {
            // Si no lo consigue, creará una sala pública nueva
            Debug.Log("No hay ninguna sala activa, se creará una nueva");
            CreatePublicLobby();
        }
    }

    // Se obtienen los jugadores del lobby y se muestran por pantalla
    /*
    private void PrintPlayers(Lobby l)
    {
        Debug.Log("Players in lobby ------ ");
        foreach (Player p in l.Players)
        {
            Debug.Log(p.Id + " " + p.Data["Name"].Value + " " + p.Data["Character"].Value);
        }
    }
    */


    // Función asíncrona para listar todas las lobbies existentes
    /*
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
    */
}
