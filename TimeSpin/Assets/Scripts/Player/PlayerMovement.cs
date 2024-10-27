using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PlayerMovement : NetworkBehaviour
{
    // Modelo y nombre del jugador
    [SerializeField] private GameObject _characterNamePlayer;
    // Due�o del jugador
    public int ownerClient;
    private Vector3 _movementDirection = Vector3.zero;
    private float _speed = 1f;

    // Control de la escena en la que se encuentra el jugador
    private enum Scene
    {
        Lobby,
        Prehistory,
        Egipt,
        Medieval,
        Maya,
        Future
    }
    [SerializeField] private Scene _currentScene = Scene.Lobby; // Se comienza en 
    private string _previousScene = "LobbyMenu"; // Se guarda una referencia a la escena en el instante anterior, para comprobar si se ha llevado a cabo alg�n cambio

    // Variables encargadas de la gesti�n de los minijuegos y el lobby
    // LOBBY
    [SerializeField] private List<Vector3> _startingPositionsLobby;
    // EGIPTO
    private Tile _currentTile; // Casilla en la que se encuentra el personaje
    [SerializeField] private List<Vector3> _startingPositionsEgipt;
    // MAYA
    private Rigidbody _rb;
    private bool _isGrounded = true; // Se indica que se encuentra en el suelo
    private float _jumpForce = 5f; // Fuerza con la que salta
    [SerializeField] private List<Vector3> _startingPositionsMaya;
    // FUTURO
    [SerializeField] private List<Vector3> _startingPositionsFuture;
    [SerializeField] private bool _startedRotation = false; // Variable que controla el giro del personaje
    private Quaternion _targetRotation; // Rotaci�n destino
    private bool _fallenPlayer = false;

    private void Start()
    {
        ownerClient = (int)OwnerClientId - 1;
    }

    private void Update()
    {
        // Se obtiene la informaci�n acerca de la escena en la que se encuentra el jugador
        GetCurrentScene();

        // El servidor procesa el movimiento del jugador, y este se sincroniza en los clientes debido al NetworkTransform
        if (NetworkManager.Singleton.IsServer)
        {
            // El servidor procesa distintos aspectos de la l�gica de los minijuegos
            switch(_currentScene)
            {
                case Scene.Egipt:
                    // En el minijuego de Egipto, se necesita conocer la casilla en la que se encuentra el jugador
                    // Para que el agente enemigo pueda realizar la b�squeda
                    // Para ello, se truncan los valores de su posici�n en los ejes x y z. A este �ltimo se le cambia el signo
                    Vector2Int tilePos = new Vector2Int((int)transform.position.x, -(int)transform.position.z);
                    _currentTile = GridManager.Instance.GetTile(tilePos.x, tilePos.y);
                    break;
                case Scene.Future:                   
                    if (_fallenPlayer) return; // Si el jugador se ha ca�do, no se procesa nada de la l�gica
                    // Se comprueba que el jugador no ha salido de los l�mites establecidos, es decir, que no se ha ca�do
                    if (transform.position.y < -5f || transform.position.y > 15f)
                    {
                        _fallenPlayer = true;
                        // Si se ha ca�do, se indica al controlador
                        GravityManager.Instance.fallenPlayers++;
                        HidePlayer(); // Se oculta al jugador
                        return;
                    }
                    // Mientras los jugadores est�n flotando, no se podr�n controlar, es decir, no se aplicar�n los inputs recibidos del cliente
                    if (GravityManager.Instance.floating && !_startedRotation)
                    {
                        // Se indica que ha comenzado la rotaci�n, y se calcula la rotaci�n destino
                        _startedRotation = true;
                        RotatePlayerToGround();
                        return;
                    }
                    else if (GravityManager.Instance.floating && _startedRotation)
                    {
                        // Aplicar rotaci�n suavemente, mientras que los jugadores est�n flotando
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, Time.deltaTime * (180 / GravityManager.Instance._floatTime));
                        return;
                    }
                    // Se indica que ya se termin� la rotaci�n
                    _startedRotation = false;
                    break;
            }
            // Aplica el movimiento recibido continuamente del cliente
            transform.position += _movementDirection * _speed * Time.deltaTime;
        }

        // Los clientes capturan los inputs y los env�an al servidor, que actualizar� la direcci�n de movimiento en funci�n de ello
        else
        {
            // S�lo se capturan los inputs del propietario del jugador
            if (!IsOwner) return;

            _movementDirection = Vector3.zero;
            // En funci�n del escenario en el que se encuentre el jugador, se realizan una serie de acciones
            switch(_currentScene)
            {
                case Scene.Lobby:
                    // Gesti�n de los controles
                    if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
                    if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
                    if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
                    if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;
                    // Env�o del input al servidor
                    ChangePlayerDirectionServerRpc(_movementDirection, (int)OwnerClientId);
                    break;

                case Scene.Egipt:
                    if (!GridManager.Instance.runningGame) return;
                    // Gesti�n de los controles
                    if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
                    if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
                    if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
                    if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;
                    // Env�o del input al servidor
                    ChangePlayerDirectionServerRpc(_movementDirection, (int)OwnerClientId);
                    break;

                case Scene.Maya:
                    if (!RaceManager.instance.runningGame) return;
                    // Gesti�n de los controles
                    if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
                    if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
                    if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
                    if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;
                    // Env�o del input al servidor
                    ChangePlayerDirectionServerRpc(_movementDirection, (int)OwnerClientId);
                    // Gesti�n del control del salto
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        JumpPlayerServerRpc((int)OwnerClientId);
                    }
                    break;

                case Scene.Future:
                    if (!GravityManager.Instance.runningGame) return;
                    // Se comprueba que el jugador no ha salido de los l�mites establecidos, es decir, que no se ha ca�do
                    if (transform.position.y < -5f || transform.position.y > 15f)
                    {
                        HidePlayer(); // En el cliente solo se oculta al jugador
                        return;
                    }
                    // Gesti�n de los controles
                    if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
                    if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
                    if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
                    if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;
                    // Env�o del input al servidor
                    ChangePlayerDirectionServerRpc(_movementDirection, (int)OwnerClientId);
                    break;
            }
        }
    }

    private void GetCurrentScene()
    {
        // Se obtiene el nombre de la escena
        string sceneName = SceneManager.GetActiveScene().name;
        // Si se sigue en la misma escena que antes, no se actualiza nada
        if (_previousScene == sceneName) return;
        // Una vez se cambia de la primera escena, se indica que ya ha comenzado el juego
        GameSceneManager.instance.gameStarted = true;
        // Gesti�n de las acciones necesarias para pasar de una escena a otra
        switch(sceneName)
        {
            case "LobbyMenu":
                _currentScene = Scene.Lobby;
                // Se coloca al jugador en la posici�n de inicio adecuada
                transform.position = _startingPositionsLobby[(int)OwnerClientId - 1];
                // Se resetea la rotaci�n
                transform.rotation = Quaternion.Euler(0, 0, 0);
                // Se restaura la gravedad
                Physics.gravity = new Vector3(0, -9.81f, 0);
                // Se vuelve a mostrar el c�digo del lobby
                if(Application.platform != RuntimePlatform.LinuxServer)
                {
                    Debug.Log(LobbyManager.instance.GetLobbyCode());
                    UI_Lobby.instance.ShowLobbyCode(LobbyManager.instance.GetLobbyCode());
                }
                // Se muestra a los jugadores
                ShowPlayer();
                break;
            case "Prehistory":
                _currentScene = Scene.Prehistory;
                break;
            case "Egipt":
                _currentScene = Scene.Egipt;
                // Se debe colocar al jugador en la posici�n de inicio adecuada
                transform.position = _startingPositionsEgipt[(int)OwnerClientId - 1];
                break;
            case "Medieval":
                _currentScene = Scene.Medieval;
                break;
            case "Maya":
                _currentScene = Scene.Maya;
                // Se eliminan las momias del minijuego anterior en el servidor
                if(Application.platform == RuntimePlatform.LinuxServer)
                {
                    DespawnMummies();
                }
                // Se obtiene una referencia al rigidbody
                _rb = GetComponent<Rigidbody>();
                // Se debe colocar al jugador en la posici�n de inicio adecuada
                transform.position = _startingPositionsMaya[(int) OwnerClientId - 1];
                // Se hace que si el personaje es del propietario, la c�mara lo siga
                if(IsOwner)
                {
                    GameObject.FindGameObjectWithTag("FollowCamera").GetComponent<CinemachineVirtualCamera>().Follow = transform;
                    GameObject.FindGameObjectWithTag("FollowCamera").GetComponent<CinemachineVirtualCamera>().LookAt = transform;
                }
                break;
            case "Future":
                _currentScene = Scene.Future;
                // Se debe colocar al jugador en la posici�n de inicio adecuada
                transform.position = _startingPositionsFuture[(int)OwnerClientId - 1];
                // Se resetea el estado, por si acaso se necesita restaurarlo
                _fallenPlayer = false;
                _startedRotation = false;
                // Se muestra a los jugadores
                ShowPlayer();
                // Se eliminan los troncos del minijuego anterior en el servidor
                if (Application.platform == RuntimePlatform.LinuxServer)
                {
                    DespawnTrunks();
                }
                break;
        }
        // Se inicia la pantalla de carga
        StartCoroutine(LoadingScreenManager.instance.LoadingScreenCoroutine(sceneName));
        _previousScene = sceneName;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerDirectionServerRpc(Vector3 direction, int playerId)
    {
        // Se comprueba primero si el jugador que se est� comprobando es del que se ha recibido el input
        if (playerId != (int)OwnerClientId) return;
        // Se modifica la direcci�n de movimiento del jugador en el servidor
        _movementDirection = direction;
    }

    // Esta funci�n se utiliza para ocultar el nombre y el personaje del jugador, sin desactivarlo completamente para poder acceder a �l de nuevo
    public void HidePlayer()
    {
        _characterNamePlayer.SetActive(false);
    }

    // Esta funci�n se usa para volver a mostrar los detalles del jugador
    public void ShowPlayer()
    {
        _characterNamePlayer.SetActive(true);
    }

    #region Egipt
    public Tile GetCurrentTile() { return _currentTile; }

    private void DespawnMummies()
    {
        if(IsServer)
        {
            // Encontrar todos los objetos con la etiqueta "Momia"
            GameObject[] mummies = GameObject.FindGameObjectsWithTag("Momia");

            // Recorrer cada objeto y despawnearlo si es un NetworkObject
            foreach (GameObject mummy in mummies)
            {
                NetworkObject networkObject = mummy.GetComponent<NetworkObject>();

                if (networkObject != null && networkObject.IsSpawned) // Verificar si es un NetworkObject y est� spawneado
                {
                    networkObject.Despawn(); // Despawnear el objeto de red
                }
            }
        }
    }
    #endregion
    #region Maya
    [ServerRpc(RequireOwnership = false)]
    private void JumpPlayerServerRpc(int playerId)
    {
        // Se comprueba primero si el jugador que se est� comprobando es del que se ha recibido el input
        if (playerId != (int)OwnerClientId) return;
        if (!_isGrounded) return; // Si no est� en el suelo, no puede saltar
        // Se aplica una fuerza sobre el rigidbody del jugador
        // Se indica que ahora el jugador ya no est� en el suelo
        _isGrounded = false;
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Detectamos cuando el jugador est� de vuelta en el suelo
        if (collision.gameObject.CompareTag("Ground") && _currentScene == Scene.Maya)
        {
            _isGrounded = true;
        }
    }

    private void DespawnTrunks()
    {
        if (IsServer)
        {
            // Encontrar todos los objetos con la etiqueta "Tronco"
            GameObject[] trunks = GameObject.FindGameObjectsWithTag("Tronco");

            // Recorrer cada objeto y despawnearlo si es un NetworkObject
            foreach (GameObject trunk in trunks)
            {
                NetworkObject networkObject = trunk.GetComponent<NetworkObject>();

                if (networkObject != null && networkObject.IsSpawned) // Verificar si es un NetworkObject y est� spawneado
                {
                    networkObject.Despawn(); // Despawnear el objeto de red
                }
            }
        }
    }

    #endregion
    #region Future
    private void RotatePlayerToGround()
    {
        // Calcular la rotaci�n objetivo (180 grados en el eje X)
        _targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x + 180 % 360, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }
    #endregion
}
