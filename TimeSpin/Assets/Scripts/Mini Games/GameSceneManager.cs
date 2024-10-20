using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager instance;

    [SerializeField] private GameObject[] _playersList;

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
        // Se hace que el objeto navegue entre escenas y no se destruya
        DontDestroyOnLoad(this);
    }

    public void InitializePlayersList()
    {
        _playersList = GameObject.FindGameObjectsWithTag("Player");
    }

    public void ActivePlayersList()
    {
        // Se reactivan los jugadores de la lista
        foreach(var player in _playersList)
        {
            PlayerMovement pMovement = player.GetComponent<PlayerMovement>();
            if (pMovement != null)
            {
                pMovement.ShowPlayer();
            }
        }
    }
}
