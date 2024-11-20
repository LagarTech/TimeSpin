using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class DinosaurController : MonoBehaviour
{
    [SerializeField] private int _dinosaurPoints; // Puntos que aporta el dinosaurio
    [SerializeField] private float _visibleTime = 1.5f; // Tiempo que estará visible
    [SerializeField] private float _timer;
    [SerializeField] private string _dinosaurType; // Tipo de dinosaurio (Normal, Velociraptor, TRex)

    private Vector3 _player; // Referencia al jugador
    private const float MAX_HIT_DISTANCE = 3.0f; // Distancia mínima para golpear al dinosaurio
    [SerializeField] float _distanceToPlayer = 0f;

    private int _hitCount = 0; // Cuenta los golpes (para el T-Rex)
    private int _requiredHits = 1; // Golpes minimos para matar al dinosaurio (1 por defecto)

    public int holeIndex; // Índice del agujero ocupado

    private Vector3 _initialPosition;
    private const float RISE_AMOUNT = 0.7f;
    private const float RISE_DURATION = 0.5f; // Duración de la animación de aparición
    private const float DISAPPEAR_DURATION = 2.5f; // Duración de la animación de desaparición

    [SerializeField] private bool _isHit = false;

    [SerializeField] private AudioSource _reproductor;
    [SerializeField] private AudioClip _clipAudio; // Sonido de golpeo al dinosaurio
    [SerializeField] private AudioClip _clipAudio2; // Sonido de derrota del T-Rex

    void Start()
    {
        // Si el dinosaurio es un T-Rex, necesita dos golpes
        if (_dinosaurType == "T-Rex")
        {
            _requiredHits = 2;
        }

    }

    void Update()
    {
        // Si el juego no ha empezado o ha terminado, que no se pueda hacer nada
        if (!PrehistoryManager.Instance.runningGame) return;
        // Se busca una referencia a la posición del jugador
        _player = GameObject.FindGameObjectWithTag("Player").transform.position;       
        // Comprobar la distancia del jugador para que lo pueda golpear o no
        _distanceToPlayer = Vector3.Distance(_player, transform.position);
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (_distanceToPlayer <= MAX_HIT_DISTANCE)
            {
                // Si el jugador está cerca, lo golpea
                HitDinosaur();
            }
        }
        _timer += Time.deltaTime;
        if (_timer >= _visibleTime)
        {
            // Se desaparece el dinosaurio
            HideDinosaur();
        }
    }

    // Función que anima la aparición del dinosaurio del agujero
    public IEnumerator AppearAnimation()
    {
        _initialPosition = transform.position;
        Vector3 targetPosition = _initialPosition + Vector3.up * RISE_AMOUNT;
        float elapsedTime = 0;

        while (elapsedTime < RISE_DURATION)
        {
            transform.position = Vector3.Lerp(_initialPosition, targetPosition, elapsedTime / RISE_DURATION);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }

    void HitDinosaur()
    {
        // Animación de golpeo
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().HitDinosaur();
        _hitCount++;
        if (_hitCount >= _requiredHits && !_isHit)
        {
            _isHit = true;

            // Logro PrimerosAsentamientos
            if (_dinosaurType == "T-Rex")
            {
                PrehistoryManager.Instance._tRexDefeated = true;
                AchievementManager.UnlockAchievement("Prehistory_PrimerosAsentamientos");
            }

            // Logro DuraciónExtensa
            if (_dinosaurType == "Velocirraptor")
            {
                PrehistoryManager.Instance._velociraptorHits++;
                if (PrehistoryManager.Instance._velociraptorHits >= 3)
                {
                    AchievementManager.UnlockAchievement("Prehistory_DuraciónExtensa");
                }
            }

            // Logro ElFuego
            if (_timer <= 1f)
            {
                AchievementManager.UnlockAchievement("Prehistory_ElFuego");
            }

            // Logro: Golpear todos los agujeros
            PrehistoryManager.Instance._holesHit.Add(holeIndex);

            // Añadir puntos
            PrehistoryManager.Instance.AddScore(_dinosaurPoints);

            // Se desaparece el dinosaurio
            HideDinosaur();
        }
        if (_hitCount < _requiredHits && _dinosaurType == "T-Rex")
        {
            MusicManager.PonerMusica(_clipAudio, _reproductor, false);
        }
    }


    private void HideDinosaur()
    {
        // Reproducción del sonido
        if(_dinosaurType == "Base" || _dinosaurType == "Velocirraptor")
        {
            MusicManager.PonerMusica(_clipAudio, _reproductor, false);
        }
        else
        {
            MusicManager.PonerMusica(_clipAudio2, _reproductor, false);
        }
        _hitCount = 0; // Reiniciar el contador de golpes
        _timer = 0; // Reiniciar el temporizador
        StartCoroutine(DisappearAnimation());
    }

    private IEnumerator DisappearAnimation()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = _initialPosition - new Vector3 (0, 3, 0); // La posición original hacia la cual descenderá
        float elapsedTime = 0;

        while (elapsedTime < DISAPPEAR_DURATION)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / DISAPPEAR_DURATION);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        _isHit = false;

        // Se indica que se desocupa su agujero
        PrehistoryManager.Instance.ReleaseHole(holeIndex);
        // Una vez finalizada la animación, se devuelve el dinosaurio al pool
        DinosaurPool.Instance.ReturnDinosaur(gameObject);
    }
}

