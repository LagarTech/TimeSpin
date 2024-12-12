using System.Collections;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UI_Controller : MonoBehaviour
{
    public static UI_Controller instance;

    public Image Fundido;

    public GameObject Desarrollador;  // Imagen desarrollador
    public GameObject Cinematica;     // Cinematica
    public GameObject Menu;           // Menu principal
    public GameObject Creditos;
    public GameObject Configuracion;

    public GameObject CreditosPanel;
    public GameObject ConfiguracionPanel;
    public GameObject PracticaPanel;

    public GameObject Practica;
    public GameObject Nombre;
    public GameObject Imagen;
    public GameObject Der;
    public GameObject Izq;
    public GameObject Jugar;
    public GameObject Volver;

    public Button avanzarButton;      // Bot?n para avanzar en la cinem?tica
    public Button jugar;              // Bot?n para jugar
    public Button creditos;           // Bot?n para pantalla de cr?ditos
    public Button configuracion;      // Bot?n para pantalla de configuraci?n
    public Button practica;
    public InputField nombreInputField; // InputField para el nombre del jugador

    public GameObject playerPrefab;

    public GameObject AbandonarPanel;
    public GameObject AbandonarBoton;

    public GameObject TextoNombre;

    [SerializeField]
    private AudioSource _reproductor;
    [SerializeField]
    private AudioClip _clipAudio;

    private Quaternion rotacionCamara;
    private Vector3 posicionCamara;

    public VideoPlayer videoPlayer;
    public AudioPlayer audioPlayer;

    public GameObject PanelInfo;
    public GameObject PanelRanking;

    public GameObject FondoCinematica;

    public GameObject joystick;

    public bool videoPrepared = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        rotacionCamara = Camera.main.transform.rotation;
        posicionCamara = Camera.main.transform.position;

        if (GameSceneManager.instance.gameStarted && !GameSceneManager.instance.practiceStarted)
        {
            // Si ya ha comenzado el juego, es decir, se vuelve de nuevo a la escena tras ya haber estado, no se necesitan mostrar todas las pantallas previas
            // Por lo tanto, s?lo se necesita la interfaz de la propia lobby
            Desarrollador.SetActive(false);
            Cinematica.SetActive(false);
            Menu.SetActive(false);
            PracticaPanel.SetActive(false);
            ConfiguracionPanel.SetActive(false);
            CreditosPanel.SetActive(false);
            Volver.SetActive(false);
            Nombre.SetActive(false);
            AbandonarBoton.SetActive(true);
            joystick.SetActive(true);

            // Se agregan los eventos a los botones asociados
            avanzarButton.onClick.AddListener(OnAvanzarButtonClicked);
            jugar.onClick.AddListener(OnJugarButtonClicked);
            creditos.onClick.AddListener(OnCreditosButtonClicked);
            configuracion.onClick.AddListener(OnConfiguracionButtonClicked);
            practica.onClick.AddListener(OnPracticaButtonClicked);

            // Se coloca la cámara
            CharacterSelectionController.instance.SetMuseumCamera();

            // Se reasigna el campo del nombre
            SelectionController.instance.ReasignNameField(Nombre);


        }
        else if (GameSceneManager.instance.practiceStarted)
        {
            // Si ya ha comenzado el juego, es decir, se vuelve de nuevo a la escena tras ya haber estado, no se necesitan mostrar todas las pantallas previas
            // Por lo tanto, solo se necesita la interfaz de la propia lobby
            Desarrollador.SetActive(false);
            Cinematica.SetActive(false);
            Menu.SetActive(false);
            PracticaPanel.SetActive(true);
            ConfiguracionPanel.SetActive(false);
            CreditosPanel.SetActive(false);
            Volver.SetActive(false);
            Nombre.SetActive(false);

            // Se agregan los eventos a los botones asociados
            avanzarButton.onClick.AddListener(OnAvanzarButtonClicked);
            jugar.onClick.AddListener(OnJugarButtonClicked);
            creditos.onClick.AddListener(OnCreditosButtonClicked);
            configuracion.onClick.AddListener(OnConfiguracionButtonClicked);
            practica.onClick.AddListener(OnPracticaButtonClicked);

            GameSceneManager.instance.gameStarted = false;

            // Se reasigna el campo del nombre
            SelectionController.instance.ReasignNameField(Nombre);

            // Se muestra el personaje correcto
            CharacterSelectionController.instance.SetCorrectPreview();
        }
        else if (GameSceneManager.instance.gameFinished)
        {
            // Si la partida ha terminado, se volverá al menú 
            Desarrollador.SetActive(false);
            Cinematica.SetActive(false);
            Menu.SetActive(true);
            PracticaPanel.SetActive(false);
            ConfiguracionPanel.SetActive(false);
            CreditosPanel.SetActive(false);
            Volver.SetActive(false);
            Nombre.SetActive(true);

            // Se agregan los eventos a los botones asociados
            avanzarButton.onClick.AddListener(OnAvanzarButtonClicked);
            jugar.onClick.AddListener(OnJugarButtonClicked);
            creditos.onClick.AddListener(OnCreditosButtonClicked);
            configuracion.onClick.AddListener(OnConfiguracionButtonClicked);
            practica.onClick.AddListener(OnPracticaButtonClicked);

            GameSceneManager.instance.gameFinished = false;
            GameSceneManager.instance.gameStarted = false;

            // Se reasigna el campo del nombre
            SelectionController.instance.ReasignNameField(Nombre);

            // Se muestra el personaje correcto
            CharacterSelectionController.instance.SetCorrectPreview();

        }
        else
        {
            // Se oculta el alfa del fundido
            Color color = Fundido.color;
            color.a = 0f;
            Fundido.color = color;
            // Al inicio, mostramos solo la imagen del desarrollador y ocultamos lo dem?s
            Desarrollador.SetActive(true);

            Cinematica.SetActive(false);
            Menu.SetActive(false);
            PracticaPanel.SetActive(false);
            ConfiguracionPanel.SetActive(false);
            CreditosPanel.SetActive(false);
            Volver.SetActive(false);
            Nombre.SetActive(false);

            // Corrutina que espera 4 segundos antes de cambiar la visibilidad
            StartCoroutine(ShowCinematicAfterDelay(4.0f));

            // Agregar el listener para el bot?n avanzar
            avanzarButton.onClick.AddListener(OnAvanzarButtonClicked);

            // Agregar el listener para el bot?n jugar
            jugar.onClick.AddListener(OnJugarButtonClicked);

            // Agregar el listener para el bot?n de cr?ditos
            creditos.onClick.AddListener(OnCreditosButtonClicked);

            // Agregar el listener para el bot?n de configuraci?n
            configuracion.onClick.AddListener(OnConfiguracionButtonClicked);

            // Agregar el listener para el bot?n de pr?ctica
            practica.onClick.AddListener(OnPracticaButtonClicked);

            // Prepara el video
            if (!MobileController.instance.isMobile())
            {
                videoPlayer.Prepare();
                videoPlayer.prepareCompleted += OnVideoPrepared;
                // Se suscribe al evento del vídeo
                videoPlayer.loopPointReached += OnVideoEnd;
            }

            // Si no se han inicializado ya, se inicializan los servicios de Unity
            if (GameSceneManager.instance.initializedServices) return;
            StartCoroutine(InitializeUnityServicesCoroutine());
        }
    }

    private IEnumerator InitializeUnityServicesCoroutine()
    {
        var initializationTask = UnityServices.InitializeAsync();

        // Esperar a que termine la inicialización
        yield return new WaitUntil(() => initializationTask.IsCompleted);

        if (initializationTask.IsFaulted)
        {
            Debug.LogError("Error al inicializar Unity Services: " + initializationTask.Exception);
            yield break;
        }

        // Autenticación del usuario
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            var signInTask = AuthenticationService.Instance.SignInAnonymouslyAsync();
            yield return new WaitUntil(() => signInTask.IsCompleted);

            if (signInTask.IsFaulted)
            {
                Debug.LogError("Error al autenticar al usuario: " + signInTask.Exception);
            }
            else
            {
                Debug.Log("Usuario autenticado exitosamente.");
                GameSceneManager.instance.initializedServices = true;
            }
        }
    }

    IEnumerator ShowCinematicAfterDelay(float delay)
    {
        // Espera los 4 segundos
        yield return new WaitForSeconds(delay);

        while (!videoPrepared && !MobileController.instance.isMobile())
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Oculta la imagen de desarrollador
        Desarrollador.SetActive(false);

        // Muestra la cinem?tica
        Cinematica.SetActive(true);

        if(!MobileController.instance.isMobile()) { 
            FondoCinematica.SetActive(true);
        }
    }

    // M?todo que se ejecutar? cuando se pulse el bot?n de avanzar
    void OnAvanzarButtonClicked()
    {
        // Ocultar la cinemática y el UILobby
        Cinematica.SetActive(false);
    }

    public void StartVideo()
    {
        if (!MobileController.instance.isMobile())
        {
            // Comenzar el vídeo
            videoPlayer.Play();
            StartCoroutine(HidePanel());
        }
    }

    private IEnumerator HidePanel()
    {
        yield return new WaitForSeconds(0.25f);
        FondoCinematica.SetActive(false);
    }

    void OnJugarButtonClicked()
    {
        // Función para ocultar el menú y pasar al lobby, mejor poner una pantalla de carga
        // Primero se comprueba si el jugador ha introducido un nombre
        if (SelectionController.instance.GetName() == "")
        {
            TextoNombre.SetActive(true);
            MusicManager.PonerMusica(_clipAudio, _reproductor, false);
            return;
        }

        TextoNombre.SetActive(false);

        // Se coloca la cámara
        CharacterSelectionController.instance.SetMuseumCamera();
        // Se muestra el joystick
        joystick.SetActive(true);
        // Se activa el panel del fundido
        Image background = GameObject.FindGameObjectWithTag("Fundido").GetComponent<Image>();
        Color color = background.color;
        color.a = 1;
        background.color = color;
        // Se mostrará la pantalla de carga unos segundos y después, se mostrará la escena del lobby del museo
        StartCoroutine(LoadingScreenManager.instance.LoadingScreenCoroutine(""));
        // Se instancia al jugador
        Instantiate(playerPrefab, GameSceneManager.instance.startingPositionLobby, Quaternion.identity);
        // Se oculta el menú
        Menu.SetActive(false);
        // Se muestra el botón de salir
        AbandonarBoton.SetActive(true);

        SelectionTable.Instance.runningGame = true;
    }

    void OnPracticaButtonClicked()
    {
        // Ocultar la cinem?tica y el menu
        Menu.SetActive(false);
        Nombre.SetActive(false);

        // Mostrar el lobby
        PracticaPanel.SetActive(true);
    }

    void OnCreditosButtonClicked()
    {
        // Ocultar la cinem?tica y el menu
        Menu.SetActive(false);
        Nombre.SetActive(false);

        // Mostrar el lobby
        CreditosPanel.SetActive(true);
    }

    void OnConfiguracionButtonClicked()
    {
        // Ocultar la cinem?tica y el menu
        Menu.SetActive(false);
        Nombre.SetActive(false);

        // Mostrar el lobby
        ConfiguracionPanel.SetActive(true);
    }

    public void OnVolverButtonClicked()
    {
        Menu.SetActive(true);
        Nombre.SetActive(true);

        ConfiguracionPanel.SetActive(false);
        CreditosPanel.SetActive(false);
        PracticaPanel.SetActive(false);
        PanelInfo.SetActive(false);
        PanelRanking.SetActive(false);
    }

    public void OcultarMenu()
    {
        Menu.SetActive(false);
        Nombre.SetActive(false);
    }

    public void MostrarPanelAbandonar()
    {
        SelectionTable.Instance.runningGame = false;
        AbandonarPanel.SetActive(true);
        AbandonarBoton.SetActive(false);
    }

    public void OcultarPanelAbandonar()
    {
        SelectionTable.Instance.runningGame = true;
        AbandonarPanel.SetActive(false);
        AbandonarBoton.SetActive(true);
    }

    public void AbandonarPartida()
    {
        GameSceneManager.instance.LeaveGame();
        AbandonarPanel.SetActive(false);
        AbandonarBoton.SetActive(true);
        Camera.main.transform.position = posicionCamara;
        Camera.main.transform.rotation = rotacionCamara;
        // Se muestra el personaje correcto
        CharacterSelectionController.instance.SetCorrectPreview();
        // Se oculta el joystick
        joystick.SetActive(false);
    }

    public void OnVideoEnd(VideoPlayer vp)
    {
        // Ocultar fondo cinematica
        FondoCinematica.SetActive(false);
        // Mostrar el lobby
        Menu.SetActive(true);
        Nombre.SetActive(true);
        videoPlayer.enabled = false;
        AbandonarBoton.SetActive(true);
        audioPlayer.PonerClip();

        GameSceneManager.instance.initiatedGame = true;
    }

    public void OnVideoPrepared(VideoPlayer vp)
    {
        videoPrepared = true;
    }

    public void ShowGameInfo()
    {
        PanelInfo.SetActive(true);
    }

    public void ShowRanking()
    {
        PanelRanking.SetActive(true);
        Ranking_Menu.instance.UpdateRanking();
    }
}
