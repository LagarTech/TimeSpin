using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlatformManager : NetworkBehaviour
{
    public static PlatformManager instance;

    private const int _numPlatforms = 45; // N�mero de plataformas
    private const int _totalDisappeared = 30; // N�mero de plataformas que van a desaparecer

    // Lista con todas las casillas de abajo
    [SerializeField] private List<GameObject> _platformsDown;
    // Lista con todas las casillas de arriba
    [SerializeField] private List<GameObject> _platformsUp;
    // Lista con los �ndices de las casillas que ir�n desapareciendo
    private List<int> _platformsDownShuffledIndex = new List<int>(_numPlatforms);
    private List<int> _platformsUpShuffledIndex = new List<int>(_numPlatforms);
    private int _numDisappeared = 0; // Contador de las casillas que han desaparecido ya

    private float _disappearTimer = 0f; // Temporizador
    private float _disappearInterval = 4f; // Tiempo entre desapariciones

    // Variables para el temblor y la ca�da
    private float _shakeDuration = 1f; // Duraci�n del temblor
    private float _fallDuration = 1.5f; // Duraci�n de la ca�da

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
    }

    private void Start()
    {
        // La l�gica de generaci�n del orden de ca�da de las plataformas se gestionar� en el servidor, y cuando se necesite que caigan una a una, se avisar� al cliente
        // de cu�l sea, para que lo haga tambi�n
        if (Application.platform != RuntimePlatform.LinuxServer) return;
        PreparePlatformsFall();
    }

    // Update is called once per frame
    void Update()
    {
        // La ca�da de las casillas se controla en el servidor 
        if (Application.platform != RuntimePlatform.LinuxServer) return;

        if (!GravityManager.Instance.runningGame) return;
        // Solo continuar si a�n no han desaparecido todas las plataformas
        if (_numDisappeared < _totalDisappeared)
        {
            _disappearTimer += Time.deltaTime;

            // Cambiar el intervalo de desaparici�n despu�s del primer minuto
            if (_disappearTimer >= 60f && _disappearInterval != 3f)
            {
                _disappearInterval = 3f;
            }

            // Si ha pasado el tiempo de desaparici�n de la siguiente plataforma
            if (_disappearTimer >= _disappearInterval)
            {
                _disappearTimer = 0f; // Reiniciar el temporizador

                // Desaparecer una plataforma de abajo
                int index = _platformsDownShuffledIndex[_numDisappeared];
                // Comienza la secuencia de ca�da
                // Primero se notifica al cliente
                FallPlatformClientRpc(index, false);
                StartCoroutine(ShakeAndFall(_platformsDown[index]));

                // Desaparecer una plataforma de arriba
                index = _platformsUpShuffledIndex[_numDisappeared];
                // Comienza la secuencia de ca�da
                // Primero se notifica al cliente
                FallPlatformClientRpc(index, true);
                StartCoroutine(ShakeAndFall(_platformsUp[index]));

                _numDisappeared++; // Incrementar el contador de plataformas desaparecidas
            }
        }
    }

    private void PreparePlatformsFall()
    {
        // Al ser un total de 45 plataformas, se rellena una lista de 45 elementos
        for(int i = 0; i < _numPlatforms; i++)
        {
            _platformsDownShuffledIndex.Add(i);
            _platformsUpShuffledIndex.Add(i);
        }
        // Para garantizar que desaparezcan de forma aleatoria, es decir, en cada partida con un orden distinto,
        // se utiliza el algoritmo de Fisher - Yates, con coste O(N)
        // Primero con las de abajo
        for (int i = _numPlatforms - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = _platformsDownShuffledIndex[i];
            _platformsDownShuffledIndex[i] = _platformsDownShuffledIndex[j];
            _platformsDownShuffledIndex[j] = temp;
        }
        // Despu�s con las de arriba
        for (int i = _numPlatforms - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = _platformsUpShuffledIndex[i];
            _platformsUpShuffledIndex[i] = _platformsUpShuffledIndex[j];
            _platformsUpShuffledIndex[j] = temp;
        }
        // Tras esto, quedar�n los �ndices ordenados de forma aleatoria
    }

    private IEnumerator ShakeAndFall(GameObject platform)
    {
        Vector3 originalPosition = platform.transform.position;

        // Fase de temblor
        float elapsedTime = 0f;
        while (elapsedTime < _shakeDuration)
        {
            elapsedTime += Time.deltaTime;

            // Mover la plataforma ligeramente de forma aleatoria en los ejes x y z
            float shakeAmount = 0.1f; // Intensidad del temblor
            platform.transform.position = originalPosition + new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                0,
                Random.Range(-shakeAmount, shakeAmount));

            yield return null;
        }

        // Restaurar la posici�n original tras el temblor
        platform.transform.position = originalPosition;

        // Fase de ca�da
        elapsedTime = 0f;

        // Verificar si la gravedad est� invertida
        bool isGravityInverted = GravityManager.Instance.isGravityInverted;
        // Si la gravedad est� invertida, el destino de ca�da ser� hacia arriba
        float fallDirection = isGravityInverted ? 5f : -5f;
        Vector3 fallTarget = originalPosition + new Vector3(0, fallDirection, 0); // Punto de destino de la ca�da

        while (elapsedTime < _fallDuration)
        {
            elapsedTime += Time.deltaTime;

            // Interpolar la posici�n desde la original hasta el punto de ca�da
            platform.transform.position = Vector3.Lerp(originalPosition, fallTarget, elapsedTime / _fallDuration);

            yield return null;
        }

        // Se apaga la plataforma, s�lo en el cliente
        if(Application.platform != RuntimePlatform.LinuxServer)
        {
            platform.GetComponentInChildren<Platform>().FallPlatform();
        }
        // Desactivar la plataforma tras la ca�da
        platform.SetActive(false);
    }

    [ClientRpc]
    private void FallPlatformClientRpc(int idPlatform, bool up)
    {
        if(!up)
        {
            StartCoroutine(ShakeAndFall(_platformsDown[idPlatform]));
        }
        else
        {
            StartCoroutine(ShakeAndFall(_platformsUp[idPlatform]));
        }
    }

    public void PlatformEntered(int idPlatform, bool up)
    {
        // Se avisa a los clientes
        PlatformEnteredClientRpc(idPlatform, up);
    }

    [ClientRpc]
    private void PlatformEnteredClientRpc(int id, bool up)
    {
        // Se buscan los objetos con la etiqueta PlataformaF
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("PlataformaF");
        foreach (GameObject plat in platforms)
        {
            // Se busca en la lista de plataformas aquella con las caracter�sticas indicadas
            if(plat.GetComponent<Platform>().idPlatform == id && plat.GetComponent<Platform>().isUp == up)
            {
                plat.GetComponent<Platform>().OnPlatformEnter();
            }
        }
    }

    public void PlatformExited(int idPlatform, bool up)
    {
        // Se avisa a los clientes
        PlatformExitedClientRpc(idPlatform, up);
    }

    [ClientRpc]
    private void PlatformExitedClientRpc(int id, bool up)
    {
        // Se buscan los objetos con la etiqueta PlataformaF
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("PlataformaF");
        foreach (GameObject plat in platforms)
        {
            // Se busca en la lista de plataformas aquella con las caracter�sticas indicadas
            if (plat.GetComponent<Platform>().idPlatform == id && plat.GetComponent<Platform>().isUp == up)
            {
                plat.GetComponent<Platform>().OnPlatformExit();
            }
        }
    }

}
