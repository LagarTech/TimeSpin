using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    private const int _numPlatforms = 45; // Número de plataformas
    private const int _totalDisappeared = 30; // Número de plataformas que van a desaparecer

    // Lista con todas las casillas de abajo
    [SerializeField] private List<GameObject> _platformsDown;
    // Lista con todas las casillas de arriba
    [SerializeField] private List<GameObject> _platformsUp;
    // Lista con los índices de las casillas que irán desapareciendo
    private List<int> _platformsDownShuffledIndex = new List<int>(_numPlatforms);
    private List<int> _platformsUpShuffledIndex = new List<int>(_numPlatforms);
    private int _numDisappeared = 0; // Contador de las casillas que han desaparecido ya

    private float _disappearTimer = 0f; // Temporizador
    private float _disappearInterval = 4f; // Tiempo entre desapariciones

    // Variables para el temblor y la caída
    private float _shakeDuration = 1f; // Duración del temblor
    private float _fallDuration = 1.5f; // Duración de la caída

    private void Start()
    {
        PreparePlatformsFall();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GravityManager.Instance.runningGame) return;
        // Solo continuar si aún no han desaparecido todas las plataformas
        if (_numDisappeared < _totalDisappeared)
        {
            _disappearTimer += Time.deltaTime;

            // Cambiar el intervalo de desaparición después del primer minuto
            if (_disappearTimer >= 60f && _disappearInterval != 3f)
            {
                _disappearInterval = 3f;
            }

            // Si ha pasado el tiempo de desaparición de la siguiente plataforma
            if (_disappearTimer >= _disappearInterval)
            {
                _disappearTimer = 0f; // Reiniciar el temporizador

                // Desaparecer una plataforma de abajo
                int index = _platformsDownShuffledIndex[_numDisappeared];
                // Comienza la secuencia de caída
                StartCoroutine(ShakeAndFall(_platformsDown[index]));

                // Desaparecer una plataforma de arriba
                index = _platformsUpShuffledIndex[_numDisappeared];
                // Comienza la secuencia de caída
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
        // Después con las de arriba
        for (int i = _numPlatforms - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = _platformsUpShuffledIndex[i];
            _platformsUpShuffledIndex[i] = _platformsUpShuffledIndex[j];
            _platformsUpShuffledIndex[j] = temp;
        }
        // Tras esto, quedarán los índices ordenados de forma aleatoria
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

        // Restaurar la posición original tras el temblor
        platform.transform.position = originalPosition;

        // Fase de caída
        elapsedTime = 0f;

        // Verificar si la gravedad está invertida
        bool isGravityInverted = GravityManager.Instance.isGravityInverted;
        // Si la gravedad está invertida, el destino de caída será hacia arriba
        float fallDirection = isGravityInverted ? 5f : -5f;
        Vector3 fallTarget = originalPosition + new Vector3(0, fallDirection, 0); // Punto de destino de la caída

        while (elapsedTime < _fallDuration)
        {
            elapsedTime += Time.deltaTime;

            // Interpolar la posición desde la original hasta el punto de caída
            platform.transform.position = Vector3.Lerp(originalPosition, fallTarget, elapsedTime / _fallDuration);

            yield return null;
        }

        // Se apaga la plataforma
        platform.GetComponentInChildren<Platform>().FallPlatform();

        // Desactivar la plataforma tras la caída
        platform.SetActive(false);
    }

}
