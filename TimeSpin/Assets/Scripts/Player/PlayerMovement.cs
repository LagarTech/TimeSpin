using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    // Modelo del jugador
    public GameObject characterNamePlayer;
    // Dirección y velocidad de movimiento
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

    // Variables encargadas de la gestión de los minijuegos
    // EGIPTO
    private Tile _currentTile; // Casilla en la que se encuentra el personaje
    // MEDIEVAL
    public GameObject carriedSword;
    // MAYA
    private Rigidbody _rb;
    private bool _isGrounded = true; // Se indica que se encuentra en el suelo
    private float _jumpForce = 5f; // Fuerza con la que salta
    // FUTURO
    [SerializeField] private bool _startedRotation = false; // Variable que controla el giro del personaje
    private Quaternion _targetRotation; // Rotación destino

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        // Se establece la escena actual
        // Se obtiene el nombre de la escena
        string sceneName = SceneManager.GetActiveScene().name;
        // Gestión de las acciones necesarias para pasar de una escena a otra
        switch (sceneName)
        {
            case "LobbyMenu": _currentScene = Scene.Lobby; break;
            case "Prehistory": _currentScene = Scene.Prehistory; break;
            case "Egipt": _currentScene = Scene.Egipt; break;
            case "Medieval": _currentScene = Scene.Medieval; break;
            case "Maya": _currentScene = Scene.Maya; break;
            case "Future": _currentScene = Scene.Future; break;
        }
    }

    private void Update()
    {
        _movementDirection = Vector3.zero;
        // En función del escenario en el que se encuentre el jugador, se realizan una serie de acciones y se procesa una lógica diferente
        switch (_currentScene)
        {
            case Scene.Lobby:
                // Gestión de los controles
                if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
                if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
                if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
                if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;
                break;

            case Scene.Prehistory:
                // Si el minijuego no ha comenzado, no se ejecuta ninguna acción
                if (!PrehistoryManager.Instance.runningGame) return;
                // Gestión de los controles
                if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
                if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
                if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
                if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;
                break;

            case Scene.Egipt:
                // Si el minijuego no ha comenzado, no se ejecuta ninguna acción
                if (!GridManager.Instance.runningGame) return;
                // En el minijuego de Egipto, se necesita conocer la casilla en la que se encuentra el jugador
                // Para que el agente enemigo pueda realizar la búsqueda
                // Para ello, se truncan los valores de su posición en los ejes x y z. A este último se le cambia el signo
                Vector2Int tilePos = new Vector2Int((int)transform.position.x, -(int)transform.position.z);
                _currentTile = GridManager.Instance.GetTile(tilePos.x, tilePos.y);
                // Gestión de los controles
                if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
                if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
                if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
                if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;
                break;

            case Scene.Medieval:
                // Si el minijuego no ha comenzado, no se ejecuta ninguna acción
                if (!MedievalGameManager.Instance.runningGame) return;
                // Gestión de los controles
                if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
                if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
                if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
                if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;
                break;

            case Scene.Maya:
                if (!RaceManager.instance.runningGame) return;
                // Gestión de los controles
                if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
                if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
                if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
                if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;
                // Gestión del control del salto
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Jump();
                }
                break;

            case Scene.Future:
                if (!GravityManager.Instance.runningGame) return;
                // Se comprueba que el jugador no ha salido de los límites establecidos, es decir, que no se ha caído
                if (transform.position.y < -5f || transform.position.y > 15f)
                {
                    // Si se ha caído, se indica al controlador
                    GravityManager.Instance.GameOver();
                    return;
                }
                // Mientras los jugadores estén flotando, no se podrán controlar, es decir, no se aplicarán los controles
                if (GravityManager.Instance.floating && !_startedRotation)
                {
                    // Se indica que ha comenzado la rotación, y se calcula la rotación destino
                    _startedRotation = true;
                    RotatePlayerToGround();
                    return;
                }
                else if (GravityManager.Instance.floating && _startedRotation)
                {
                    // Aplicar rotación suavemente, mientras que los jugadores están flotando
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, Time.deltaTime * (180 / GravityManager.Instance._floatTime));
                    return;
                }
                // Se indica que ya se terminó la rotación
                _startedRotation = false;
                // Gestión de los controles
                if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
                if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
                if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
                if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;
                break;
        }
        transform.position += _movementDirection * _speed * Time.deltaTime;
    }

    // Esta función se utiliza para ocultar el nombre y el personaje del jugador, sin desactivarlo completamente para poder acceder a él de nuevo
    public void HidePlayer()
    {
        characterNamePlayer.SetActive(false);
    }

    // Esta función se usa para volver a mostrar los detalles del jugador
    public void ShowPlayer()
    {
        characterNamePlayer.SetActive(true);
    }

    #region Egipt
    public Tile GetCurrentTile() { return _currentTile; }

    #endregion

    #region Medieval
    public void SetCarriedSword(GameObject sword)
    {
        carriedSword = sword;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si el jugador entra a su base y lleva una espada
        if(carriedSword != null && other.gameObject.tag == "Base")
        {
            // El jugador deja de llevar la espada
            carriedSword = null;
            // La deja en la base
            carriedSword.GetComponent<SwordController>().DeliverSword();
        }
    }

    #endregion
    #region Maya
    private void Jump()
    {
        if (!_isGrounded) return; // Si no está en el suelo, no puede saltar
        // Se aplica una fuerza sobre el rigidbody del jugador
        // Se indica que ahora el jugador ya no está en el suelo
        _isGrounded = false;
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Detectamos cuando el jugador está de vuelta en el suelo
        if (collision.gameObject.CompareTag("Ground") && _currentScene == Scene.Maya)
        {
            _isGrounded = true;
        }
    }

    #endregion
    #region Future
    private void RotatePlayerToGround()
    {
        // Calcular la rotación objetivo (180 grados en el eje X)
        _targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x + 180 % 360, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }
    #endregion
}
