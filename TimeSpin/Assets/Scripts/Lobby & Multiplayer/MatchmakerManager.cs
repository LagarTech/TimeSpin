using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
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
        if(NetworkManager.Singleton.IsServer && Application.platform == RuntimePlatform.LinuxServer)
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
        // Verifica si el NetworkManager es el servidor y está ejecutándose en Linux
        if (NetworkManager.Singleton.IsServer && Application.platform == RuntimePlatform.LinuxServer)
        {
            // Verifica si hay clientes conectados
            if (NetworkManager.Singleton.ConnectedClients.Count > 0)
            {
                // Desconecta a todos los clientes
                foreach (var client in NetworkManager.Singleton.ConnectedClients)
                {
                    NetworkManager.Singleton.DisconnectClient(client.Key);
                }
            }
            // Finaliza el NetworkManager
            NetworkManager.Singleton.Shutdown();
        }
    }

    // Función para realizar el emparejamiento del creador de la sala con el servidor
    // Se hace como una corrutina para evitar la instrucción Task.Delay, incompatible con WebGL
    public IEnumerator FirstServerJoinCoroutine(System.Action<bool> onComplete)
    {
        // Este proceso se ejecuta hasta que se encuentra un servidor. En caso de timeout, se crea un ticket nuevo, hasta que se encuentra el server
        while (true)
        {
            // Se configura la petición del ticket
            CreateTicketOptions createTicketOptions = new CreateTicketOptions("BetaQueue");
            List<Player> players = new List<Player> { new Player(AuthenticationService.Instance.PlayerId) };

            var createTicketTask = MatchmakerService.Instance.CreateTicketAsync(players, createTicketOptions);

            yield return new WaitUntil(() => createTicketTask.IsCompleted);

            if (createTicketTask.Exception != null)
            {
                Debug.LogError("Error creating ticket: " + createTicketTask.Exception);
                yield return new WaitForSeconds(1f); // Espera antes de reintentar
                continue; // Reintenta la creación del ticket
            }

            _currentTicket = createTicketTask.Result.Id;
            Debug.Log("Ticket created: " + _currentTicket);

            // Después, se comprueba el estado del ticket
            bool serverFound = false;

            while (!serverFound)
            {
                var getTicketTask = MatchmakerService.Instance.GetTicketAsync(_currentTicket);
                yield return new WaitUntil(() => getTicketTask.IsCompleted);

                if (getTicketTask.Exception != null)
                {
                    Debug.LogError("Error checking ticket: " + getTicketTask.Exception);
                    onComplete(false);
                    yield break; // Detener la ejecución en caso de error grave
                }

                TicketStatusResponse ticketStatusResponse = getTicketTask.Result;

                if (ticketStatusResponse.Type == typeof(MultiplayAssignment))
                {
                    MultiplayAssignment multiplayAssignment = (MultiplayAssignment)ticketStatusResponse.Value;

                    if (multiplayAssignment.Status == MultiplayAssignment.StatusOptions.Found)
                    {
                        // UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                        _serverIP = multiplayAssignment.Ip;
                        _serverPort = ushort.Parse(multiplayAssignment.Port.ToString());
                        // transport.SetClientSecrets(SecureParameters.ServerCommonName, SecureParameters.MyGameClientCA); // Credenciales de seguridad de la red
                        // transport.SetConnectionData(_serverIP, _serverPort);
                        // NetworkManager.Singleton.StartClient();
                        Debug.Log("Server found");
                        onComplete(true);
                        yield break; // Servidor encontrado, salir de la función
                    }
                    else if (multiplayAssignment.Status == MultiplayAssignment.StatusOptions.Timeout)
                    {
                        Debug.Log("Match timeout, retrying...");
                        break; // Salir del bucle interno y recrear el ticket
                    }
                    else if (multiplayAssignment.Status == MultiplayAssignment.StatusOptions.Failed)
                    {
                        Debug.Log("Match failed: " + multiplayAssignment.Status + "  " + multiplayAssignment.Message);
                        onComplete(false);
                        yield break; // En caso de fallo, se sale de la corrutina
                    }
                    else if (multiplayAssignment.Status == MultiplayAssignment.StatusOptions.InProgress)
                    {
                        Debug.Log("Match in progress");
                    }
                }

                yield return new WaitForSeconds(1f);
            }

            // Espera antes de reintentar la creación del ticket
            yield return new WaitForSeconds(1f);
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
