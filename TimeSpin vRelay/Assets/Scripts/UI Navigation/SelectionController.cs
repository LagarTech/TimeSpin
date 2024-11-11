using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class SelectionController : MonoBehaviour
{
    public static SelectionController instance;
    // Variables que almacenan la información con la que se caracteriza un jugador en línea
    [SerializeField] private int _selectedCharacter;
    [SerializeField] private string _namePlayer;

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

    // Función que devuelve un jugador con sus datos para incluirlo en el lobby
    public Player GetPlayer()
    {
        // Este jugador se caracteriza por un nombre y un personaje seleccionado
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                // Solo almacenamos el nombre del jugador, junto con el color del coche que lleve
                { "Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _namePlayer) },
                { "Character", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _selectedCharacter.ToString()) }
            }
        };
    }

    public int GetCharacterSelected() { return _selectedCharacter; }
    public string GetName() { return _namePlayer; }


}
