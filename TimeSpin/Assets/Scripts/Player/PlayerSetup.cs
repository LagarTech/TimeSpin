using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    [SerializeField] List<GameObject> _charactersList;
    [SerializeField] string _playerName;

    private void Start()
    {
        _charactersList[SelectionController.instance.GetCharacterSelected()].SetActive(true);
        _playerName = SelectionController.instance.GetName();
        SelectionTable.Instance.AddPlayerReference(transform);
    }

}
