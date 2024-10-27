using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class LoadingScreenManager : NetworkBehaviour
{
    public static LoadingScreenManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator LoadingScreenCoroutine(string startedScene)
    {
        // Se busca la pantalla de carga en la escena, s?lo en los clientes
        if (Application.platform != RuntimePlatform.LinuxServer)
        {
            GameObject loadingScreen = GameObject.FindGameObjectWithTag("PantallaCarga");
            // Se activa el objeto con la pantalla de carga
            foreach (Transform child in loadingScreen.transform)
            {
                child.gameObject.SetActive(true);
            }
            yield return null;
        }
        else
        {
            // La l?gica de espera se ejecuta en el servidor
            // Se esperan 5 segundos, para dar tiempo a la colocaci?n de cada escena
            yield return new WaitForSeconds(5f);
            // Una vez hecho esto, en funci?n de la escena a la que se ha transicionado, se activa el minijuego adecuado y se avisa a los clientes
            switch (startedScene)
            {
                case "LobbyMenu":
                    StartLobbyClientRpc();
                    break;
                case "Prehistory":
                    StartPrehistoryClientRpc();
                    break;
                case "Egipt":
                    GridManager.Instance.runningGame = true; // Se inicia la l?gica del juego en el servidor
                    StartEgiptClientRpc();
                    break;
                case "Medieval":
                    StartMedievalClientRpc();
                    break;
                case "Maya":
                    RaceManager.instance.runningGame = true; // Se inicia la l?gica del juego en el servidor
                    StartMayaClientRpc();
                    break;
                case "Future":
                    GravityManager.Instance.runningGame = true; // Se inicia la l?gica del juego en el servidor
                    StartFutureClientRpc();
                    break;
            }
        }
    }

    private void HideLoadingScreen()
    {
        // Se oculta la pantalla de carga
        GameObject loadingScreen = GameObject.FindGameObjectWithTag("PantallaCarga");
        if (loadingScreen == null) return;
        // Se desactiva el objeto con la pantalla de carga
        foreach (Transform child in loadingScreen.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    private void StartLobbyClientRpc()
    {
        HideLoadingScreen();
    }

    [ClientRpc]
    private void StartPrehistoryClientRpc()
    {
        HideLoadingScreen();
    }

    [ClientRpc]
    private void StartEgiptClientRpc()
    {
        HideLoadingScreen();
        // Se activa el minijuego
        GridManager.Instance.runningGame = true;
    }

    [ClientRpc]
    private void StartMedievalClientRpc()
    {
        HideLoadingScreen();
    }

    [ClientRpc]
    private void StartMayaClientRpc()
    {
        HideLoadingScreen();
        // Se activa el miniijuego
        RaceManager.instance.runningGame = true;
    }

    [ClientRpc]
    private void StartFutureClientRpc()
    {
        HideLoadingScreen();
        // Se activa el minijuego
        GravityManager.Instance.runningGame = true;
    }

}
