using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class Cinematic : MonoBehaviour
{

    public TMP_Text varibaleTexto;
    public bool cinematicaSaltada = false;

    public GameObject Cinematica;
    public GameObject Menu;
    public GameObject Nombre;

    public InputAction saltarSaltar;

    private void Start()
    {
        // Habilitar la acci�n de entrada cuando el script est� activo.
        saltarSaltar.Enable();

        if (MobileController.instance.isMobile())
        {
            varibaleTexto.text = "PULSA PARA CONTINUAR";
        }
        else
        {
            varibaleTexto.text = "PULSA ESPACE PARA CONTINUAR";
        }
    }
    private void Update()
    {
        if(saltarSaltar.triggered)
        {
            ComenzarJuego();
        }
        if (MobileController.instance.isMobile())
        {
            MobileController.instance.interactuar.onClick.AddListener(HandleMobileInteraction);
        }
    }

    private void HandleMobileInteraction()
    {
        ComenzarJuego();
    }

    public void ComenzarJuego()
    {
        if (cinematicaSaltada) return;
        cinematicaSaltada = true;

        if (MobileController.instance.isMobile())
        {
            MobileController.instance.interactuar.onClick.RemoveListener(HandleMobileInteraction);
            Cinematica.SetActive(false);
            // Mostrar el lobby
            UI_Controller.instance.Menu.SetActive(true);
            UI_Controller.instance.Nombre.SetActive(true);
            UI_Controller.instance.AbandonarBoton.SetActive(true);
            UI_Controller.instance.audioPlayer.PonerClip();

            GameSceneManager.instance.initiatedGame = true;
            return;
        }
        Cinematica.SetActive(false);
        UI_Controller.instance.StartVideo();
    }
}
