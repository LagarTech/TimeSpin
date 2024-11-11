using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_Controller : MonoBehaviour
{
    public static UI_Controller instance;

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
    public GameObject Crear;
    public GameObject Unirse;
    public GameObject Volver;
    public GameObject LobbyCode;

    public Button avanzarButton;      // Bot?n para avanzar en la cinem?tica
    public Button jugar;              // Bot?n para jugar
    public Button creditos;           // Bot?n para pantalla de cr?ditos
    public Button configuracion;      // Bot?n para pantalla de configuraci?n
    public Button practica;
    public InputField nombreInputField; // InputField para el nombre del jugador

    private bool nombreFijado = false; // Controla si el nombre ya est? fijado

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
        if (Application.platform == RuntimePlatform.LinuxServer) return; // Evita la ejecuci?n en el servidor

        if (GameSceneManager.instance.gameStarted)
        {
            // Si ya ha comenzado el juego, es decir, se vuelve de nuevo a la escena tras ya haber estado, no se necesitan mostrar todas las pantallas previas
            // Por lo tanto, s?lo se necesita la interfaz de la propia lobby
            Desarrollador.SetActive(false);
            Cinematica.SetActive(false);
            Menu.SetActive(false);
            Crear.SetActive(false);
            Unirse.SetActive(false);
            LobbyCode.SetActive(false);
            PracticaPanel.SetActive(false);
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
        }
        else if (GameSceneManager.instance.practiceStarted)
        {
            // Si ya ha comenzado el juego, es decir, se vuelve de nuevo a la escena tras ya haber estado, no se necesitan mostrar todas las pantallas previas
            // Por lo tanto, s?lo se necesita la interfaz de la propia lobby
            Desarrollador.SetActive(false);
            Cinematica.SetActive(false);
            Menu.SetActive(false);
            Crear.SetActive(false);
            Unirse.SetActive(false);
            LobbyCode.SetActive(false);
            PracticaPanel.SetActive(true);
            ConfiguracionPanel.SetActive(false);
            CreditosPanel.SetActive(false);
            Volver.SetActive(false);
            Nombre.SetActive(false);

            GameSceneManager.instance.practiceStarted = false;

            // Se agregan los eventos a los botones asociados
            avanzarButton.onClick.AddListener(OnAvanzarButtonClicked);
            jugar.onClick.AddListener(OnJugarButtonClicked);
            creditos.onClick.AddListener(OnCreditosButtonClicked);
            configuracion.onClick.AddListener(OnConfiguracionButtonClicked);
            practica.onClick.AddListener(OnPracticaButtonClicked);
        }
        else 
        {
            // Al inicio, mostramos solo la imagen del desarrollador y ocultamos lo dem?s
            Desarrollador.SetActive(true);

            Cinematica.SetActive(false);
            Menu.SetActive(false);
            Crear.SetActive(false);
            Unirse.SetActive(false);
            LobbyCode.SetActive(false);
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
        }
    }

    IEnumerator ShowCinematicAfterDelay(float delay)
    {
        // Espera los 4 segundos
        yield return new WaitForSeconds(delay);

        // Oculta la imagen de desarrollador
        Desarrollador.SetActive(false);

        // Muestra la cinem?tica
        Cinematica.SetActive(true);
    }

    // M?todo que se ejecutar? cuando se pulse el bot?n de avanzar
    void OnAvanzarButtonClicked()
    {
        // Ocultar la cinem?tica y el UILobby
        Cinematica.SetActive(false);

        // Mostrar el lobby
        Menu.SetActive(true);
        Nombre.SetActive(true);
    }

    void OnJugarButtonClicked()
    {
        // Bloquear el InputField para que no se pueda modificar m?s
        if (!nombreFijado && !string.IsNullOrEmpty(nombreInputField.text))
        {
            nombreFijado = true;
            nombreInputField.interactable = false; // Desactiva el InputField
        }

        // Ocultar la cinem?tica y el menu
        Creditos.SetActive(false);
        Configuracion.SetActive(false);
        Practica.SetActive(false);
        Jugar.SetActive(false);
        Der.SetActive(false);
        Izq.SetActive(false);

        // Mostrar el lobby
        Nombre.SetActive(true);
        Crear.SetActive(true);
        Unirse.SetActive(true);
        Volver.SetActive(true);
        LobbyCode.SetActive(true);
    }

    void OnPracticaButtonClicked()
    {
        // Ocultar la cinem?tica y el menu
        Menu.SetActive(false);
        Crear.SetActive(false);
        Unirse.SetActive(false);
        Nombre.SetActive(false);
        LobbyCode.SetActive(false);

        // Mostrar el lobby
        PracticaPanel.SetActive(true);
    }

    void OnCreditosButtonClicked()
    {
        // Ocultar la cinem?tica y el menu
        Menu.SetActive(false);
        Crear.SetActive(false);
        Unirse.SetActive(false);
        Nombre.SetActive(false);
        LobbyCode.SetActive(false);

        // Mostrar el lobby
        CreditosPanel.SetActive(true);
    }

    void OnConfiguracionButtonClicked()
    {
        // Ocultar la cinem?tica y el menu
        Menu.SetActive(false);
        Crear.SetActive(false);
        Unirse.SetActive(false);
        Nombre.SetActive(false);
        LobbyCode.SetActive(false);

        // Mostrar el lobby
        ConfiguracionPanel.SetActive(true);
    }

    public void OnVolverButtonClicked()
    {
        // Ocultar la cinem?tica y el menu
        if (CreditosPanel.active)
        {
            Menu.SetActive(true);
            Nombre.SetActive(true);

            ConfiguracionPanel.SetActive(false);
            Crear.SetActive(false);
            Unirse.SetActive(false);
            LobbyCode.SetActive(false);
            PracticaPanel.SetActive(false);
            CreditosPanel.SetActive(false);
        }

        if (ConfiguracionPanel.active)
        {
            Menu.SetActive(true);
            Nombre.SetActive(true);

            CreditosPanel.SetActive(false);
            Crear.SetActive(false);
            Unirse.SetActive(false);
            LobbyCode.SetActive(false);
            PracticaPanel.SetActive(false);
            ConfiguracionPanel.SetActive(false);
        }

        if (Crear.active)
        {
            Menu.SetActive(true);
            Nombre.SetActive(true);
            Der.SetActive(true);
            Izq.SetActive(true);
            Jugar.SetActive(true);
            Practica.SetActive(true);
            Configuracion.SetActive(true);
            Creditos.SetActive(true);
            nombreInputField.interactable = true;

            Volver.SetActive(false);
            ConfiguracionPanel.SetActive(false);
            CreditosPanel.SetActive(false);
            PracticaPanel.SetActive(false);
            Crear.SetActive(false);
            Unirse.SetActive(false);
            LobbyCode.SetActive(false);

            UI_Lobby.instance.HideMessages();
        }

        if (PracticaPanel.active)
        {
            Menu.SetActive(true);
            Nombre.SetActive(true);

            ConfiguracionPanel.SetActive(false);
            CreditosPanel.SetActive(false);
            Crear.SetActive(false);
            Unirse.SetActive(false);
            PracticaPanel.SetActive(false);
        }
    }

    public void OcultarMenu()
    {
        Menu.SetActive(false);
        Nombre.SetActive(false);
    }
}
