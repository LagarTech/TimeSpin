using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Cinematic : MonoBehaviour
{
    public GameObject Cinematica;
    public GameObject Menu;
    public GameObject Nombre;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ComenzarJuego();
        }
    }
    public void ComenzarJuego()
    {
        Debug.Log("Pulsado");
        // Ocultar la cinem?tica y el UILobby
        Cinematica.SetActive(false);

        // Mostrar el lobby
        Menu.SetActive(true);
        Nombre.SetActive(true);
    }
}
