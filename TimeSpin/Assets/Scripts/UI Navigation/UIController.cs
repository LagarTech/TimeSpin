using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameObject Desarrollador;  // Imagen desarrollador
    public GameObject Cinematica;     // Cinematica
    public GameObject Menu;           //Menu principal
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
    public GameObject volver;


    public Button avanzarButton;      // Botón para avanzar en la cinemática
    public Button jugar;              // Botón para jugar
    public Button creditos;           // Botón para pantalla de creditos
    public Button configuracion;      // Botón para pantalla de config
    public Button practica;
   

    public InputField playerNameInput;  // El InputField donde el jugador escribe su nombre



    void Start()
    {
        if (Application.platform == RuntimePlatform.LinuxServer) return; // Evita la ejecución en el servidor

        playerNameInput.interactable = true;

        // Al inicio, mostramos solo la imagen del desarrollador y ocultamos lo demás
        Desarrollador.SetActive(true);

        Cinematica.SetActive(false);
        Menu.SetActive(false);
        Crear.SetActive(false);
        Unirse.SetActive(false);
        PracticaPanel.SetActive(false);
        ConfiguracionPanel.SetActive(false);
        CreditosPanel.SetActive(false);
        volver.SetActive(false);
        

        // Corrutina que espera 4 segundos antes de cambiar la visibilidad
        StartCoroutine(ShowCinematicAfterDelay(4.0f));

        // Agregar el listener para el botón avanzar
        avanzarButton.onClick.AddListener(OnAvanzarButtonClicked);

        // Agregar el listener para el botón jugar
        jugar.onClick.AddListener(OnJugarButtonClicked);

        // Agregar el listener para el botón jugar
        creditos.onClick.AddListener(OnCreditosButtonClicked);

        // Agregar el listener para el botón jugar
        configuracion.onClick.AddListener(OnConfiguracionButtonClicked);

        practica.onClick.AddListener(OnPracticaButtonClicked);

    }

    IEnumerator ShowCinematicAfterDelay(float delay)
    {
        // Espera los 4 segundos
        yield return new WaitForSeconds(delay);

        // Oculta la imagen de desarrollador
        Desarrollador.SetActive(false);

        // Muestra la cinematica
        Cinematica.SetActive(true);
    }

    // Método que se ejecutará cuando se pulse el botón de avanzar
    void OnAvanzarButtonClicked()
    {
        // Ocultar la cinemática y el UILobby
        Cinematica.SetActive(false);

        // Mostrar el lobby
        Menu.SetActive(true);

    }

    void OnJugarButtonClicked()
    {
       // Ocultar la cinemática y el menu
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
        volver.SetActive(true);

        // Bloquea el InputField para que no se pueda editar más
        playerNameInput.interactable = false;

        // Opcionalmente, puedes hacer algo más aquí, como pasar a otra escena o mostrar un mensaje
        Debug.Log("Nombre del jugador: " + playerNameInput.text);

    }

    void OnPracticaButtonClicked()
    {
        // Ocultar la cinemática y el menu
        Menu.SetActive(false);
        Crear.SetActive(false);
        Unirse.SetActive(false);



        // Mostrar el lobby
        PracticaPanel.SetActive(true);
    }

    void OnCreditosButtonClicked()
    {
        // Ocultar la cinemática y el menu
        Menu.SetActive(false);
        Crear.SetActive(false);
        Unirse.SetActive(false);
        // Mostrar el lobby

        CreditosPanel.SetActive(true);
    }

    void OnConfiguracionButtonClicked()
    {
        // Ocultar la cinemática y el menu
        Menu.SetActive(false);
        Crear.SetActive(false);
        Unirse.SetActive(false);

        // Mostrar el lobby
        ConfiguracionPanel.SetActive(true);

    }

    public void OnVolverButtonClicked()
    {
        // Ocultar la cinemática y el menu
        if (CreditosPanel.active)
        {
            Menu.SetActive(true);
            ConfiguracionPanel.SetActive(false);
            Crear.SetActive(false);
            Unirse.SetActive(false);
            PracticaPanel.SetActive(false);
            CreditosPanel.SetActive(false);
        };

        if (ConfiguracionPanel.active)
        {
            Menu.SetActive(true);
            CreditosPanel.SetActive(false);
            Crear.SetActive(false);
            Unirse.SetActive(false);
            PracticaPanel.SetActive(false);
            ConfiguracionPanel.SetActive(false);
        };

        if (Crear.active)
        {
            Menu.SetActive(true);
            ConfiguracionPanel.SetActive(false);
            CreditosPanel.SetActive(false);
            PracticaPanel.SetActive(false);
            Crear.SetActive(false);
            Unirse.SetActive(false);
        };

        if (PracticaPanel.active)
        {
            Menu.SetActive(true);
            ConfiguracionPanel.SetActive(false);
            CreditosPanel.SetActive(false);
            Crear.SetActive(false);
            Unirse.SetActive(false);
            PracticaPanel.SetActive(false);
        };

    }



}
