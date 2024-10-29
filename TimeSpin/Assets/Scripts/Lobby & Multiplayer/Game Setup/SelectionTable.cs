using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectionTable : MonoBehaviour
{
    public static SelectionTable Instance;

    private const float MAX_DISTANCE = 2f; // Distancia a la que se puede encontrar el jugador al interactuar con el objeto
    private Transform _playerPosition;

    // Textos y paneles
    [SerializeField] private GameObject _notStartedSelection;
    [SerializeField] private GameObject _selectionMenu;
    [SerializeField] private GameObject _selectionText;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayer(GameObject player)
    {
        _playerPosition = player.transform;
    }

    private void OnMouseDown()
    {
        float distance = Vector3.Distance(transform.position, _playerPosition.position);
        if (distance > MAX_DISTANCE) return; // Se comprueba que el jugador pueda interactuar con el objeto
        // Si aún no se ha comenzado la votación, se muestra por pantalla
        if(!StartingManager.instance.startedSelection)
        {
            StartCoroutine(ShowNotSelection());
        }
        else
        {
            _selectionMenu.SetActive(true);
            _selectionText.SetActive(false);
        }
    }

    private IEnumerator ShowNotSelection()
    {
        _notStartedSelection.SetActive(true);
        yield return new WaitForSeconds(4f);
        _notStartedSelection.SetActive(false);
    }
}
