using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GravityManager : MonoBehaviour
{
    public static GravityManager Instance;

    // Variable que controla el flujo del juego
    public bool runningGame = false;

    // Tiempo que se espera entre cambios de gravedad
    private const float _gravitySwitchTime = 10f;
    private float _gravityTimer; // Temporizador
    private bool _isGravityInverted = false; // Controla si se ha invertido la gravedad o no

    // Gestión de que los jugadores floten
    private const float _gravity = 9.81f;
    public bool floating;
    private float _floatTimer; // Temporizador
    public float _floatTime; // Tiempo que tardan los jugadores en ir de una plataforma a otra
    // Referencias a las plataformas, para calcular el tiempo que se tarda en ir de una a otra
    [SerializeField] private Transform _topPlatform;
    [SerializeField] private Transform _bottomPlatform;

    // Temporizador del juego
    [SerializeField] private TMP_Text _timerText;
    private float _remainingTime = 120f; // El tiempo de juego son 2 minutos (120 segundos)

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        // Se calcula el tiempo de flotación utilizando la ecuación del MRUA
        // d = d0 + v0*t + 1/2*a*t^2 -> Se parte del reposo -> d = 1/2*a*t^2
        // Despejando, se obtiene que t = sqrt(2*d/g), siendo d la distancia entre las plataformas y g la gravedad
        float distance = Vector3.Distance(_topPlatform.position, _bottomPlatform.position);
        _floatTime = Mathf.Sqrt(2 * distance / _gravity);
    }

    private void Update()
    {
        if (!runningGame) return;

        // GESTIÓN DEL TIEMPO RESTANTE
        if (_remainingTime > 0f)
        {
            // Disminuir el tiempo restante
            _remainingTime -= Time.deltaTime;
            // Se actualiza el temporizador
            UpdateTimer();
        }
        else
        {
            _remainingTime = 0f;
            // Se actualiza el temporizador
            UpdateTimer();
            // Se indica que el juego ha finalizado
            runningGame = false;
            GameOver();
            return;
        }

        // GESTIÓN DE LA INVERSIÓN DE LA GRAVEDAD
        _gravityTimer += Time.deltaTime;

        if (_gravityTimer >= _gravitySwitchTime)
        {
            // Se inicia el proceso de flotación
            StartFloating();
            _gravityTimer = 0; // También se reinicia el temporizador
        }

        if(floating)
        {
            // Se comienza el contador
            _floatTimer += Time.deltaTime;
            if(_floatTimer >= _floatTime)
            {
                // Terminar la flotación cuando se acabe el tiempo calculado
                StopFloating();
            }
        }
    }

    private void StartFloating()
    {
        // Los jugadores comienzan a flotar
        floating = true;
        _floatTimer = 0f;

        // Se invierte la gravedad
        if (_isGravityInverted)
        {
            Physics.gravity = new Vector3(0, -_gravity, 0);  // Gravedad normal
        }
        else
        {
            Physics.gravity = new Vector3(0, _gravity, 0);   // Gravedad invertida
        }
        _isGravityInverted = !_isGravityInverted;

    }

    private void StopFloating()
    {
        floating = false;
        _floatTimer = 0f;
    }

    private void UpdateTimer()
    {
        // Calcular minutos y segundos
        int displayMinutes = Mathf.FloorToInt(_remainingTime / 60);
        int displaySeconds = Mathf.FloorToInt(_remainingTime % 60);

        // Actualizar el texto del TMP para que muestre el tiempo restante
        _timerText.text = string.Format("{0:00}:{1:00}", displayMinutes, displaySeconds);
    }

    private void GameOver()
    {

    }


}
