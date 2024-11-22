using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionController : MonoBehaviour
{
    public static SelectionController instance;
    // Variables que almacenan la informaci�n con la que se caracteriza un jugador en l�nea
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
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _namePlayer = "";
    }

    // Funci�n para modificar el personaje escogido
    public void ModifyCharacter(int newCharacter)
    {
        _selectedCharacter = newCharacter;
    }

    // Funci�n para cambiar el nombre del jugador
    public void ModifyName(string newName)
    {
        _namePlayer = newName;
    }

    public int GetCharacterSelected() { return _selectedCharacter; }
    public string GetName() { return _namePlayer; }


}
