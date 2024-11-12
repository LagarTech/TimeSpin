using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : MonoBehaviour
{
    [SerializeField] private GameObject _arrowImage;
    public int baseIndex;

    private void Update()
    {
        // Se indica de forma visual que es la base a la que el jugador debe llevar la espada
        if(baseIndex == MedievalGameManager.Instance.nextBaseIndex)
        {
            _arrowImage.SetActive(true);
        }
        else
        {
            _arrowImage.SetActive(false);
        }
    }
}
