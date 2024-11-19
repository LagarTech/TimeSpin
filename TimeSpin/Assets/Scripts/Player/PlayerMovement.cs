using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    // Modelo del jugador
    public GameObject characterNamePlayer;
    // Direcci�n y velocidad de movimiento
    private Vector3 _movementDirection = Vector3.zero;
    private float _speed = 4f;

    // Control de la escena en la que se encuentra el jugador
    private enum Scene
    {
        Lobby,
        Prehistory,
        Egypt,
        Medieval,
        Maya,
        Future
    }
    [SerializeField] private Scene _currentScene = Scene.Lobby; // Se comienza en 

    // Variables encargadas de la gesti�n de los minijuegos
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
    private Quaternion _targetRotation; // Rotaci�n destino

    // CONTROL DE ANIMACIONES
    private Animator _animatorController;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _animatorController = GetComponentInChildren<Animator>();
        // Se establece la escena actual
        // Se obtiene el nombre de la escena
        string sceneName = SceneManager.GetActiveScene().name;
        // Gesti�n de las acciones necesarias para pasar de una escena a otra
        switch (sceneName)
        {
            case "LobbyMenu": _currentScene = Scene.Lobby; break;
            case "Prehistory": _currentScene = Scene.Prehistory; break;
            case "Egypt": _currentScene = Scene.Egypt; break;
            case "Medieval": _currentScene = Scene.Medieval; break;
            case "Maya": _currentScene = Scene.Maya; break;
            case "Future": _currentScene = Scene.Future; break;
        }
    }

    private void Update()
    {
        _movementDirection = Vector3.zero;
        // En funci�n del escenario en el que se encuentre el jugador, se realizan una serie de acciones y se procesa una l�gica diferente
        switch (_currentScene)
        {
            case Scene.Lobby:
                // Si se est� en el men�, no te puedes mover
                if (!SelectionTable.Instance.runningGame) return;
                // Gesti�n de los controles
                ProcessMovementInput();
                break;

            case Scene.Prehistory:
                // Si el minijuego no ha comenzado, no se ejecuta ninguna acci�n
                if (!PrehistoryManager.Instance.runningGame) return;
                // Gesti�n de los controles
                ProcessMovementInput();
                break;

            case Scene.Egypt:
                // Si el minijuego no ha comenzado, no se ejecuta ninguna acci�n
                if (!GridManager.Instance.runningGame) return;
                // En el minijuego de Egipto, se necesita conocer la casilla en la que se encuentra el jugador
                // Para que el agente enemigo pueda realizar la b�squeda
                // Para ello, se truncan los valores de su posici�n en los ejes x y z. A este �ltimo se le cambia el signo
                Vector2Int tilePos = new Vector2Int((int)transform.position.x, -(int)transform.position.z);
                _currentTile = GridManager.Instance.GetTile(tilePos.x, tilePos.y);
                // Gesti�n de los controles
                ProcessMovementInput();
                break;

            case Scene.Medieval:
                // Si el minijuego no ha comenzado, no se ejecuta ninguna acci�n
                if (!MedievalGameManager.Instance.runningGame) return;
                // Gesti�n de los controles
                ProcessMovementInput();
                break;

            case Scene.Maya:
                if (!RaceManager.instance.runningGame) return;
                // Gesti�n de los controles
                ProcessMovementInput();
                // Gesti�n del control del salto
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Jump();
                }
                break;

            case Scene.Future:
                if (!GravityManager.Instance.runningGame) return;
                // Se comprueba que el jugador no ha salido de los l�mites establecidos, es decir, que no se ha ca�do
                if (transform.position.y < -5f || transform.position.y > 15f)
                {
                    // Si se ha ca�do, se indica al controlador
                    GravityManager.Instance.GameOver();
                    return;
                }
                // Mientras los jugadores est�n flotando, no se podr�n controlar, es decir, no se aplicar�n los controles
                if (GravityManager.Instance.floating && !_startedRotation)
                {
                    // Se indica que ha comenzado la rotaci�n, y se calcula la rotaci�n destino
                    _startedRotation = true;
                    RotatePlayerToGround();
                    // Actualiza la velocidad del Rigidbody manteniendo la gravedad
                    Vector3 cVelocity = _rb.velocity;
                    Vector3 hVelocity = Vector3.zero;
                    _rb.velocity = new Vector3(hVelocity.x, cVelocity.y, hVelocity.z);
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
                // Gesti�n de los controles
                ProcessMovementInput();
                break;
        }

        // Actualiza la velocidad del Rigidbody manteniendo la gravedad
        Vector3 currentVelocity = _rb.velocity;
        Vector3 horizontalVelocity = _movementDirection * _speed;
        _rb.velocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);

        // Calcula la velocidad del Animator usando solo las componentes x y z
        float currentSpeed = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z).magnitude;
        _animatorController.SetFloat("Speed", currentSpeed);
    }

    private void ProcessMovementInput()
    {
        if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
        if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
        if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
        if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;
    }


    // Esta funci�n se utiliza para ocultar el nombre y el personaje del jugador, sin desactivarlo completamente para poder acceder a �l de nuevo
    public void HidePlayer()
    {
        characterNamePlayer.SetActive(false);
    }

    // Esta funci�n se usa para volver a mostrar los detalles del jugador
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

    private void OnCollisionEnter(Collision collision)
    {
        // Detectamos cuando el jugador est� de vuelta en el suelo en el juego Maya
        if (collision.gameObject.CompareTag("Ground") && _currentScene == Scene.Maya)
        {
            _isGrounded = true;
            _animatorController.SetBool("Jump", false);
        }
        // Si el jugador entra a una base y lleva una espada en el juego Medieval
        if (carriedSword != null && collision.gameObject.tag == "Base" && _currentScene == Scene.Medieval)
        {
            // Si se deja la espada en la base correcta
            if (collision.gameObject.GetComponent<Base>().baseIndex == MedievalGameManager.Instance.nextBaseIndex)
            {
                // El jugador deja la espada en la base
                carriedSword.GetComponent<SwordController>().DeliverSword();
                // El jugador deja de llevar la espada
                carriedSword = null;
                // Se genera la siguiente base
                MedievalGameManager.Instance.ChooseRandomBase();
            }
        }
    }

    #endregion
    #region Maya
    private void Jump()
    {
        if (!_isGrounded) return; // Si no est� en el suelo, no puede saltar
        // Se aplica una fuerza sobre el rigidbody del jugador
        // Se indica que ahora el jugador ya no est� en el suelo
        _isGrounded = false;
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        _animatorController.SetBool("Jump", true);
    }

    #endregion
    #region Future
    private void RotatePlayerToGround()
    {
        // Calcular la rotaci�n objetivo (180 grados en el eje X)
        _targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x + 180 % 360, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }
    #endregion

    public IEnumerator InteractPlayer()
    {
        _animatorController.SetBool("Interacting", true);
        yield return new WaitForSeconds(2f);
        _animatorController.SetBool("Interacting", false);
    } 
}
