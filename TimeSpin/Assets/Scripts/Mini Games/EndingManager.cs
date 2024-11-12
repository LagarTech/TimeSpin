using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance;

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

    void Start()
    {
        StartCoroutine(InitializeUnityServicesCoroutine());
    }

    private IEnumerator InitializeUnityServicesCoroutine()
    {
        var initializationTask = UnityServices.InitializeAsync();

        // Esperar a que termine la inicialización
        yield return new WaitUntil(() => initializationTask.IsCompleted);

        if (initializationTask.IsFaulted)
        {
            Debug.LogError("Error al inicializar Unity Services: " + initializationTask.Exception);
            yield break;
        }

        // Autenticación del usuario
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            var signInTask = AuthenticationService.Instance.SignInAnonymouslyAsync();
            yield return new WaitUntil(() => signInTask.IsCompleted);

            if (signInTask.IsFaulted)
            {
                Debug.LogError("Error al autenticar al usuario: " + signInTask.Exception);
            }
            else
            {
                Debug.Log("Usuario autenticado exitosamente.");
            }
        }
    }

    public void ShowResults()
    {
        
    }
}
