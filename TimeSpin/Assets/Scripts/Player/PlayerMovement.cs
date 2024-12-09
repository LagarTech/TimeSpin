using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using System.Runtime.InteropServices;
using UnityEngine.UI; // Para manejar el botón interactivo
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    // Modelo del jugador
    public GameObject characterNamePlayer;
    // Dirección y velocidad de movimiento
    private Vector3 _movementDirection = Vector3.zero;
    public float _speed = 4f;

    private Vector2 _entradaMovimiento;   // Para almacenar la entrada del sistema de entrada.

    public InputAction accionMover; // Referencia a la acción "Mover".

    public InputAction interactuarSaltar;

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

    // Variables encargadas de la gestión de los minijuegos
    // PREHISTORIA
    private bool _isHitting = false;
    // EGIPTO
    [SerializeField] public Tile _currentTile; // Casilla en la que se encuentra el personaje
    // MEDIEVAL
    public GameObject carriedSword;
    // MAYA
    private Rigidbody _rb;
    private bool _isGrounded = true; // Se indica que se encuentra en el suelo
    private float _jumpForce = 5f; // Fuerza con la que salta
    public bool isConfused = false;
    private float _confusedTime = 0f;

    // FUTURO
    [SerializeField] private bool _startedRotation = false; // Variable que controla el giro del personaje
    private Quaternion _targetRotation; // Rotación destino

    // CONTROL DE ANIMACIONES
    private Animator _animatorController;

    [SerializeField] private AudioSource _reproductor;
    [SerializeField] private AudioClip _clipAudio;

    private void Start()
    {

        _rb = GetComponent<Rigidbody>();

        // Activa la acción de entrada.
        accionMover.Enable();
        interactuarSaltar.Enable();

        // Configuración para el botón móvil
        if (MobileController.instance.isMobile())
        {
            MobileController.instance.interactuar.onClick.AddListener(HandleMobileInteraction);
        }

        // Se establece la escena actual
        // Se obtiene el nombre de la escena
        string sceneName = SceneManager.GetActiveScene().name;
        // Gestión de las acciones necesarias para pasar de una escena a otra
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


        // En función del escenario en el que se encuentre el jugador, se realizan una serie de acciones y se procesa una lógica diferente
        switch (_currentScene)
        {
            case Scene.Lobby:
                // Si se está en el menú, no te puedes mover
                if (!SelectionTable.Instance.runningGame) return;
                // Gestión de los controles
                if (MobileController.instance.isMobile())
                {
                    ProcessMobileInput();
                }
                else
                {
                    ProcessMovementInput(); // Controles de PC
                    if (interactuarSaltar.triggered)
                    {
                        Interact();
                    }   
                }
                break;

            case Scene.Prehistory:
                // Si el minijuego no ha comenzado, no se ejecuta ninguna acción
                if (!PrehistoryManager.Instance.runningGame) return;
                // Gestión de los controles
                if (MobileController.instance.isMobile())
                {
                    ProcessMobileInput();
                }
                else
                {
                    ProcessMovementInput(); // Controles de PC
                    if (interactuarSaltar.triggered)
                    {
                       Interact();
                    }
                }
                // Gestión del movimiento con la animación de golpeo
                AnimatorStateInfo stateInfo = _animatorController.GetCurrentAnimatorStateInfo(0);
                if(stateInfo.IsName("Standing Melee Attack Downward"))
                {
                    _isHitting = true;
                }
                else
                {
                    _isHitting = false;
                }
                break;

            case Scene.Egypt:
                // Si el minijuego no ha comenzado, no se ejecuta ninguna acción
                if (!GridManager.Instance.runningGame) return;
                // En el minijuego de Egipto, se necesita conocer la casilla en la que se encuentra el jugador
                // Para que el agente enemigo pueda realizar la búsqueda
                // Para ello, se truncan los valores de su posición en los ejes x y z. A este último se le cambia el signo
                Vector2Int tilePos = new Vector2Int((int)transform.position.x, -(int)transform.position.z);
                _currentTile = GridManager.Instance.GetTile(tilePos.x, tilePos.y);
                // Gestión de los controles
                if (MobileController.instance.isMobile())
                {
                    ProcessMobileInput();
                }
                else
                {
                    ProcessMovementInput(); // Controles de PC
                }
                break;

            case Scene.Medieval:
                // Si el minijuego no ha comenzado, no se ejecuta ninguna acción
                if (!MedievalGameManager.Instance.runningGame) return;
                // Gestión de los controles
                if (MobileController.instance.isMobile())
                {
                    ProcessMobileInput();
                }
                else
                {
                    ProcessMovementInput(); // Controles de PC
                }
                break;

            case Scene.Maya:
                if (!RaceManager.instance.runningGame) return;
                // Gestión de los controles
                if (MobileController.instance.isMobile())
                {
                    ProcessMobileInput();
                }
                else
                {
                    ProcessMovementInput();  
                    
                    // Controles de PC
                    if (interactuarSaltar.triggered)
                    {
                       Jump();
                    } 
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
                    // Actualiza la velocidad del Rigidbody manteniendo la gravedad
                    Vector3 cVelocity = _rb.velocity;
                    Vector3 hVelocity = Vector3.zero;
                    _rb.velocity = new Vector3(hVelocity.x, cVelocity.y, hVelocity.z);
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
                if (MobileController.instance.isMobile())
                {
                    ProcessMobileInput();
                }
                else
                {
                    ProcessMovementInput(); // Controles de PC
                }
                break;
        }

        AnimatorStateInfo state = _animatorController.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("Pick Fruit"))
        {
            _rb.velocity = Vector3.zero;
        }

        // Actualiza la velocidad del Rigidbody manteniendo la gravedad
        Vector3 currentVelocity = _rb.velocity;
        Vector3 horizontalVelocity = _movementDirection * _speed;
        // Si está confuso, se invierten sus controles
        if(isConfused)
        {
            horizontalVelocity = new Vector3 (-horizontalVelocity.x, horizontalVelocity.y, horizontalVelocity.z);
        }

        if(_isHitting)
        {
            _rb.velocity = Vector3.zero;
        }
        else
        {
            _rb.velocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);
        }

        // Calcula la velocidad del Animator usando solo las componentes x y z
        float currentSpeed = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z).magnitude;

        // Actualizar animaciones
        if (_animatorController != null)
        {
            _animatorController.SetFloat("Speed", currentSpeed);

            // Se escoge la animación de correr en función de la escena
            if (_currentScene == Scene.Egypt)
            {
                _animatorController.SetBool("Egypt", true);
            }
            else
            {
                _animatorController.SetBool("Egypt", false);
            }
        }

        // Rotar el personaje según la dirección de movimiento
        if (_isHitting) return;
        RotateCharacter();

    }

    private void ProcessMovementInput()
    {
        // Lee el valor de entrada del sistema de entrada.
        _entradaMovimiento = accionMover.ReadValue<Vector2>();

        // Mapear el movimiento a un Vector3 (si es necesario para la lógica de tu juego).
        _movementDirection.x = _entradaMovimiento.x; // Izquierda/Derecha (A/D o Joystick eje X).
        _movementDirection.z = _entradaMovimiento.y; // Adelante/Atrás (W/S o Joystick eje Y).

    }

    private void ProcessMobileInput()
    {
        //Controles móviles con joystick
       _movementDirection.x = MobileController.instance.Joystick.Horizontal;
        _movementDirection.z = MobileController.instance.Joystick.Vertical;
    }

    private void RotateCharacter()
    {
        // Rotación del personaje
        if (_movementDirection.sqrMagnitude > 0.01f) // Comprueba si hay movimiento
        {
            // Calcula el ángulo de rotación en torno al eje Y basado en la dirección de movimiento
            float targetYRotation = Mathf.Atan2(_movementDirection.x, _movementDirection.z) * Mathf.Rad2Deg;

            // Obtén la rotación actual del jugador como ángulos de Euler
            Vector3 currentRotation = transform.rotation.eulerAngles;

            // Crea una nueva rotación que conserva los valores actuales en X y Z, pero ajusta la Y
            Quaternion targetRotation = Quaternion.Euler(currentRotation.x, targetYRotation, currentRotation.z);

            // Aplica una rotación suave hacia el objetivo
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 360f);
        }
    }
    private void HandleMobileInteraction()
    {
        // Evento vinculado al botón móvil
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        // Lógica común para salto e interacción
        if (_isGrounded && _currentScene == Scene.Maya)
        {
            Jump();
        }
        if(_currentScene == Scene.Lobby || _currentScene == Scene.Prehistory) 
        {
            Interact(); 
        }
    }

    private void Interact()
    {
        GameObject[] interactObject = GameObject.FindGameObjectsWithTag("Interactuable");

        for (int i = 0; i < interactObject.Length; i++)
        {
            if (interactObject[i].GetComponent <SelectionTable>() != null) 
            {
                interactObject[i].GetComponent<SelectionTable>().InteractTable();
            }
            if (interactObject[i].GetComponent<LobbyChest>() != null)
            {
                interactObject[i].GetComponent<LobbyChest>().InteractChest();
            }
            if (interactObject[i].GetComponent<DinosaurController>() != null)
            {
                interactObject[i].GetComponent<DinosaurController>().InteractDino();
            }
        }
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

    private void OnCollisionEnter(Collision collision)
    {
        if (_currentScene == Scene.Maya && collision.gameObject.CompareTag("Obstaculos"))
        {
            AchievementsManager.instance.RegisterObstacleHit();

            if(collision.gameObject.GetComponent<TrunkMovement>() != null)
            {
                _confusedTime = 0f;
                if(!isConfused)
                {
                    StartCoroutine(ConfusedState());
                }
                MusicManager.PonerMusica(_clipAudio, _reproductor, false);
            }
        }

        // Detectamos cuando el jugador está de vuelta en el suelo en el juego Maya
        if (collision.gameObject.CompareTag("Ground") && _currentScene == Scene.Maya)
        {
            _isGrounded = true;
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

    private IEnumerator ConfusedState()
    {
        isConfused = true;

        // Bucle que se ejecuta mientras no haya pasado el tiempo total
        while (_confusedTime < 5f)
        {
            _confusedTime += Time.deltaTime; // Aumentar el tiempo transcurrido
            yield return null; // Esperar hasta el siguiente frame
        }

        isConfused = false;
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
        _animatorController.SetTrigger("Jump");

        //Salto en el sistema de logros
        AchievementsManager.instance.RegisterJump();
    }

    #endregion
    #region Future
    private void RotatePlayerToGround()
    {
        // Calcular la rotación objetivo (180 grados en el eje X)
        _targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x + 180 % 360, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }
    #endregion

    public void SetAnimator()
    {
        _animatorController = GetComponentInChildren<Animator>();
    }

    public void InteractPlayer()
    {
        _animatorController.SetTrigger("Interacting");
    }

    public void HitDinosaur(Vector3 dinosaurPosition)
    {
        // Se calcula la rotación del personaje mirando al dinosaurio
        Vector3 direction = (dinosaurPosition - transform.position).normalized;
        direction.y = 0;
        Quaternion desiredRotation = Quaternion.LookRotation(direction);
        StartCoroutine(RotateToDinosaur(desiredRotation));
        _animatorController.SetTrigger("Hit");

    }


    private IEnumerator RotateToDinosaur(Quaternion rotacionObjetivo, float duracion = 0.25f)
    {
        // Rotación inicial
        Quaternion rotacionInicial = transform.rotation;

        // Tiempo transcurrido
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            // Incrementar el tiempo
            tiempo += Time.deltaTime;

            // Interpolar entre la rotación inicial y la rotación objetivo
            transform.rotation = Quaternion.Lerp(rotacionInicial, rotacionObjetivo, tiempo / duracion);

            // Esperar al siguiente frame
            yield return null;
        }

        // Asegurarse de que termine exactamente en la rotación objetivo
        transform.rotation = rotacionObjetivo;
    }

}
