using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConfused : MonoBehaviour
{
    private PlayerMovement playerMovement;

    [SerializeField] private GameObject _confusedDuck;

    // Start is called before the first frame update
    void Start()
    {
        playerMovement= GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        // Se activa o desactiva el modelo del pato en función de si el jugador está confuso o no
        if(playerMovement.isConfused)
        {
            _confusedDuck.SetActive(true);
        }
        else
        {
            _confusedDuck.SetActive(false);
        }
    }
}
