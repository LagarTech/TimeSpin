using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

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
        // Habilitar la acción de entrada cuando el script esté activo.
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
        }
        Cinematica.SetActive(false);
        UI_Controller.instance.StartVideo();
    }
}
