using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : NetworkBehaviour
{
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

    // Variables encargadas de la gesti�n de los minijuegos
    // EGIPTO
    [SerializeField] private Tile _currentTile;
    [SerializeField] private List<Vector3> _startingPositions;

    private void Update()
    {
        // Se obtiene la informaci�n acerca de la escena en la que se encuentra el jugador
        GetCurrentScene();

        // El servidor procesa el movimiento del jugador, y este se sincroniza en los clientes debido al NetworkTransform
        if (NetworkManager.Singleton.IsServer)
        {
            transform.position += _movementDirection * _speed * Time.deltaTime;

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
            }
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
        switch(sceneName)
        {
            case "LobbyMenu":
                _currentScene = Scene.Lobby;
                break;
            case "Prehistory":
                _currentScene = Scene.Prehistory;
                break;
            case "Egipt":
                _currentScene = Scene.Egipt;
                // Se debe colocar al jugador en la posici�n de inicio adecuada
                transform.position = _startingPositions[(int)OwnerClientId - 1];
                break;
            case "Medieval":
                _currentScene = Scene.Medieval;
                break;
            case "Maya":
                _currentScene = Scene.Maya;
                break;
            case "Future":
                _currentScene = Scene.Future;
                break;
        }
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

    #region Egipt
    public Tile GetCurrentTile() { return _currentTile; }
    #endregion
}
