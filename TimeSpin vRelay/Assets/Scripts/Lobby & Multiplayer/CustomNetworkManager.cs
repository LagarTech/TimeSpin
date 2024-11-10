using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomNetworkManager : MonoBehaviour
{
    // Se garantiza que s�lo haya un NetworkManager
    public static CustomNetworkManager Instance;

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
}
