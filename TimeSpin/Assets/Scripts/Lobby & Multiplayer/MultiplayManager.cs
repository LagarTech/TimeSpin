using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Authentication;
// El servicio de Multiplay no es compatible con WebGL y s�lo es necesario para el c�digo del servidor
#if UNITY_SERVER
using Unity.Services.Multiplay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Networking.Transport.Relay;
#endif

public class MultiplayManager : MonoBehaviour
{
    public static MultiplayManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Si se est� en el servidor, se hace que este objeto pase entre escenas, para que no se reinicie al volver al lobby
#if UNITY_SERVER
        DontDestroyOnLoad(this);
#endif
    }

    #if UNITY_SERVER
    private IServerQueryHandler _serverQueryHandler; // Variable para el manejador del servidor
    private Lobby _lobbySession; // Referencia al lobby al que se va a conectar el servidor
    // Temporizador para el heartbeat
    private float _heartBeatLobbyTimer = 0f;
    private const float MAX_HEARTBEAT_TIMER = 10f; // Intervalo de tiempo en segundos para el heartbeat

    private async void Start()
    {
        // Se verifica que se est� en el servidor
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            // Se inicializan los servicios de Unity
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            // Se ajusta el frame rate para no crashear el servidor
            Application.targetFrameRate = 60;
            await UnityServices.InitializeAsync(); // Se inicializan los servicios de Unity
            // Se obtiene la informaci�n del servidor
            ServerConfig serverConfig = MultiplayService.Instance.ServerConfig;
            // Se configuran los datos del servidor
            _serverQueryHandler = await MultiplayService.Instance.StartServerQueryHandlerAsync(4, "TimeSpinServer", "HistoricGame", "0", "TestMap");
            // Se comprueba si el servidor est� activo
            if (serverConfig.AllocationId != string.Empty)
            {
                // PASO 1 - Se crea una asignaci�n de Relay
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(5);  // N�mero de jugadores permitidos, m�s el servidor
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                // PASO 2 - Crear un lobby en Unity Services con los detalles del servidor
                string lobbyName = "TimeSpinLobby";
                int maxPlayers = 5; // 4 jugadores m�s el servidor

                // Crear un diccionario de datos personalizados
                var lobbyData = new Dictionary<string, DataObject>
                {
                    {
                        "joinCode", new DataObject(
                            visibility: DataObject.VisibilityOptions.Public,
                            value: joinCode)
                    },
                    {
                        "serverIP", new DataObject(
                            visibility: DataObject.VisibilityOptions.Public,
                            value: serverConfig.IpAddress.ToString(),
                            index: DataObject.IndexOptions.S1)
                    },
                    {
                        "serverPort", new DataObject(
                            visibility: DataObject.VisibilityOptions.Public,
                            value: serverConfig.Port.ToString(),
                            index: DataObject.IndexOptions.S2)
                    }
                };

                // Opciones para crear el lobby
                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    IsPrivate = false,  // El lobby ser� p�blico
                    Data = lobbyData    // Aqu� se pasan los datos personalizados
                };

                try
                {
                    // Crear el lobby de forma as�ncrona y esperar el resultado
                    _lobbySession = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

                    // Si la tarea fue exitosa, mostramos el c�digo del lobby
                    Debug.Log("Created Lobby! Join Code: " + _lobbySession.LobbyCode);
                }
                catch (System.Exception ex)
                {
                    // Manejo de errores en caso de que haya fallado la creaci�n del lobby
                    Debug.LogError("Error creating lobby: " + ex.Message);
                }

                // PASO 3 - Configurar la conexi�n con UnityTransport usando Relay
                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(new RelayServerData(allocation, "wss"));

                NetworkManager.Singleton.StartServer();

                // Se indica que el servidor est� listo para que los jugadores se puedan unir
                await MultiplayService.Instance.ReadyServerForPlayersAsync();
            }
        }
    }

    private async void Update()
    {
        if (NetworkManager.Singleton.IsServer && Application.platform == RuntimePlatform.LinuxServer)
        {
            if (_serverQueryHandler != null)
            {
                // Se actualiza el n�mero de jugadores del servidor
                _serverQueryHandler.CurrentPlayers = (ushort)NetworkManager.Singleton.ConnectedClients.Count;
                _serverQueryHandler.UpdateServerCheck();
                // Se retrasa la ejecuci�n para no saturar las comprobaciones
                await Task.Delay(100);
            }

            // Temporizador para enviar latidos al lobby y que no desaparezca
            _heartBeatLobbyTimer += Time.deltaTime;

            // Si se supera el tiempo umbral, se env�a el heartbeat
            if (_heartBeatLobbyTimer > MAX_HEARTBEAT_TIMER)
            {
                _heartBeatLobbyTimer = 0f;  // Reiniciamos el temporizador
                SendPingHeartbeat();  // Llamamos al m�todo para enviar el ping
            }
        }
    }

    private async void SendPingHeartbeat()
    {
        try
        {
            // Enviar el ping al lobby para mantenerlo activo
            await LobbyService.Instance.SendHeartbeatPingAsync(_lobbySession.Id);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error while sending heartbeat: " + ex.Message);
        }
    }
#endif

    // Esta funci�n se utiliza para conectar a los clientes con el servidor
    public void JoinToServer(string IP, string port)
    {
        // Se establece la conexi�n
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(IP, ushort.Parse(port));
        // Se inicia el cliente
        NetworkManager.Singleton.StartClient();
    }
}
