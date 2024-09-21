using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionController : MonoBehaviour
{
    public static SelectionController instance;
    // Variables que almacenan la información con la que se caracteriza un jugador en línea
    private int _selectedCharacter;
    private string _namePlayer;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    // Función para modificar el personaje escogido
    public void ModifyCharacter(int newCharacter)
    {
        _selectedCharacter = newCharacter;
    }
    // Función para cambiar el nombre del jugador
    public void ModifyName(string newName)
    {
        _namePlayer = newName;
    }



}
