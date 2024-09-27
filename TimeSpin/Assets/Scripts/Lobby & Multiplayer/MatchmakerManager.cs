using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class MatchmakerManager : NetworkBehaviour
{
    public static MatchmakerManager Instance;

    private string _currentTicket;
    private string _serverIP;
    private ushort _serverPort;

    private bool _isDeallocating = false;
    private bool _deallocatingCancellationToken = false;

    public string GetServerIP() { return _serverIP; }
    public ushort GetServerPort() { return _serverPort; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        // Este código solo se ejecuta en el servidor, ya que es el encargado de desasignarlo
        if(Application.platform == RuntimePlatform.LinuxServer)
        {
            // Si no hay ningún cliente conectado se detiene el servidor
            if(NetworkManager.Singleton.ConnectedClients.Count == 0 && !_isDeallocating)
            {
                _isDeallocating = true;
                _deallocatingCancellationToken = false;
                DeallocateServer();
            }
            if(NetworkManager.Singleton.ConnectedClients.Count != 0)
            {
                _isDeallocating = false;
                _deallocatingCancellationToken = true;
            }

        }
    }

    private void OnApplicationQuit()
    {
        if(Application.platform != RuntimePlatform.LinuxServer)
        {
            if(NetworkManager.Singleton.IsConnectedClient)
            {
                NetworkManager.Singleton.Shutdown(true);
                NetworkManager.Singleton.DisconnectClient(OwnerClientId);
            }
        }
    }

    // Función para realizar el emparejamiento del creador de la sala con el servidor
    public async Task FirstServerJoin()
    {
        // Se configura la petición del ticket
        CreateTicketOptions createTicketOptions = new CreateTicketOptions("JoinServerQueue");
        List<Player> players = new List<Player> { new Player(AuthenticationService.Instance.PlayerId) };
        CreateTicketResponse createTicketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, createTicketOptions);

        _currentTicket = createTicketResponse.Id;
        Debug.Log("Ticket created: " + _currentTicket);

        // Después, se comprueba el estado del ticket
        while (true)
        {
            TicketStatusResponse ticketStatusResponse = await MatchmakerService.Instance.GetTicketAsync(_currentTicket);
            // Se comprueba si su estado ha cambiado 
            if(ticketStatusResponse.Type == typeof(MultiplayAssignment))
            {
                MultiplayAssignment multiplayAssignment = (MultiplayAssignment) ticketStatusResponse.Value;
                // En función del estado que sea se hace una acción u otra
                if (multiplayAssignment.Status == MultiplayAssignment.StatusOptions.Found)
                {
                    UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                    // Se obtienen los datos de la conexión
                    _serverIP = multiplayAssignment.Ip;
                    _serverPort = ushort.Parse(multiplayAssignment.Port.ToString());
                    transport.SetConnectionData(_serverIP, _serverPort);

                    NetworkManager.Singleton.StartClient();
                    Debug.Log("Server found");
                    return;
                }
                else if (multiplayAssignment.Status == MultiplayAssignment.StatusOptions.Timeout)
                {
                    Debug.Log("Match timeout");
                    return;
                }
                else if (multiplayAssignment.Status == MultiplayAssignment.StatusOptions.Failed)
                {
                    Debug.Log("Match failed: " + multiplayAssignment.Status + "  " + multiplayAssignment.Message);
                    return;
                }
                else if (multiplayAssignment.Status == MultiplayAssignment.StatusOptions.InProgress)
                {
                    Debug.Log("Match in progress");
                }
            }

            await Task.Delay(1000);
        }
    }

    // Función para desasignar un servidor
    private async void DeallocateServer()
    {
        await Task.Delay(60 * 1000);

        if(!_deallocatingCancellationToken)
        {
            Application.Quit();
        }
    }

}
