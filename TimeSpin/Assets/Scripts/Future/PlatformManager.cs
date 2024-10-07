using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    private const int _numPlatforms = 45; // Número de plataformas
    private const int _totalDisappeared = 30; // Número de plataformas que van a desaparecer

    // Lista con todas las casillas
    [SerializeField] private List<GameObject> _platformsUp;
    // Lista con los índices de las casillas que irán desapareciendo
    private List<int> _platformsUpSuffledIndex = new List<int>(_numPlatforms);
    private int _numDisappeared = 0; // Contador de las casillas que han desaparecido ya

    private float _disappearTimer = 0f; // Temporizador
    private float _disappearInterval = 4f; // Tiempo entre desapariciones
    private const float _gameDuration = 120f; // Duración total del juego

    private void Start()
    {
        PreparePlatformsFall();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PreparePlatformsFall()
    {
        // Al ser un total de 45 plataformas, se rellena una lista de 45 elementos
        for(int i = 0; i < _numPlatforms; i++)
        {
            _platformsUpSuffledIndex.Add(i);
        }
        // Para garantizar que desaparezcan de forma aleatoria, es decir, en cada partida con un orden distinto,
        // se utiliza el algoritmo de Fisher - Yates, con coste O(N)
        for (int i = _numPlatforms - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = _platformsUpSuffledIndex[i];
            _platformsUpSuffledIndex[i] = _platformsUpSuffledIndex[j];
            _platformsUpSuffledIndex[j] = temp;
        }
        // Tras esto, quedarán los índices ordenados de forma aleatoria
    }


}
