using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    [SerializeField] List<GameObject> _charactersList;
    [SerializeField] string _playerName;

    private void Start()
    {
        // Se muestra el personaje correcto
        _charactersList[SelectionController.instance.GetCharacterSelected()].SetActive(true);

        // Se obtiene su nombre
        _playerName = SelectionController.instance.GetName();

        // Se hace referencia a la mesa
        SelectionTable.Instance.AddPlayerReference(transform);

    }

}
