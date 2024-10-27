using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private List<GameObject> _characterList; // Lista que almacena todos los personajes seleccionables
    [SerializeField] private TMP_Text _playerName; // Nombre del jugador
    [SerializeField] private List<Vector3> _spawnPositionList; // Lista de posiciones donde comienza el jugador

    public int playerID;


    private void Start()
    {
        SetupPlayer();
    }

    private void SetupPlayer()
    {
        // Se obtiene el identificador del jugador, para poder establecer sus datos personalizados
        playerID = (int)OwnerClientId - 1;

        // Este código solo se ejecuta en el servidor
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            // Se coloca al jugador en la parte correcta de la escena
            PositionPlayer();
        }

        if(IsOwner)
        {
            // Se almacenan los datos del jugador en el registro
            PlayerRegister.instance.RegisterPlayerServerRpc(SelectionController.instance.GetCharacterSelected(), SelectionController.instance.GetName());
        }
        else
        {
            // Se establecen las características de los personajes que ya estén spawneados en red
            PlayerRegister.instance.GetPlayerServerRpc(playerID);
        }
    }

    private void PositionPlayer()
    {
        transform.position = _spawnPositionList[playerID];
    }

    public void NamePlayer(string name)
    {
        _playerName.text = name;
    }

    public void CharacterPlayer(int characterSelected)
    {
        // Por si acaso, primero se ocultan todos los modelos de la lista
        foreach (GameObject p in _characterList)
        {
            p.SetActive(false);
        }
        // Después, se obtiene el personaje que se debe visibilizar
        _characterList[characterSelected].SetActive(true);
    }
   
}
