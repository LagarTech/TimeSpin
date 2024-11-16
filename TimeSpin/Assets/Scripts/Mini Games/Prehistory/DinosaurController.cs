using System.Collections;
using UnityEngine;

public class DinosaurController : MonoBehaviour
{
    [SerializeField] private int _dinosaurPoints; // Puntos que aporta el dinosaurio
    [SerializeField] private float _visibleTime = 1.5f; // Tiempo que estar� visible
    [SerializeField] private float _timer;
    [SerializeField] private string _dinosaurType; // Tipo de dinosaurio (Normal, Velociraptor, TRex)

    private Vector3 _player; // Referencia al jugador
    private const float MAX_HIT_DISTANCE = 3.0f; // Distancia m�nima para golpear al dinosaurio
    [SerializeField] float _distanceToPlayer = 0f;

    private int _hitCount = 0; // Cuenta los golpes (para el T-Rex)
    private int _requiredHits = 1; // Golpes minimos para matar al dinosaurio (1 por defecto)

    public int holeIndex; // �ndice del agujero ocupado

    private Vector3 _initialPosition;
    private const float RISE_AMOUNT = 0.7f;
    private const float RISE_DURATION = 0.5f; // Duraci�n de la animaci�n de aparici�n
    private const float DISAPPEAR_DURATION = 0.5f; // Duraci�n de la animaci�n de desaparici�n

    private bool _isHit = false;

    void Start()
    {
        // Si el dinosaurio es un T-Rex, necesita dos golpes
        if (_dinosaurType == "T-Rex")
        {
            _requiredHits = 2;
        }

        // Se busca una referencia a la posici�n del jugador
        _player = GameObject.FindGameObjectWithTag("Player").transform.position;       
    }

    void Update()
    {
        // Si el juego no ha empezado o ha terminado, que no se pueda hacer nada
        if (!PrehistoryManager.Instance.runningGame) return;
        // Comprobar la distancia del jugador para que lo pueda golpear o no
        _distanceToPlayer = Vector3.Distance(_player, transform.position);
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (_distanceToPlayer <= MAX_HIT_DISTANCE)
            {
                // Si el jugador est� cerca, lo golpea
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

    // Funci�n que anima la aparici�n del dinosaurio del agujero
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
        _hitCount++;
        if (_hitCount >= _requiredHits && !_isHit)
        {
            _isHit = true;

            // Logro: Derrota al T-Rex
            if (_dinosaurType == "T-Rex")
            {
                PrehistoryManager.Instance._tRexDefeated = true;
                AchievementManager.UnlockAchievement("Prehistory_PrimerosAsentamientos");
            }

            // Logro: Raptor letal
            if (_dinosaurType == "Velociraptor")
            {
                PrehistoryManager.Instance._velociraptorHits++;
                if (PrehistoryManager.Instance._velociraptorHits >= 3)
                {
                    AchievementManager.UnlockAchievement("Prehistory_Duraci�nExtensa");
                }
            }

            // Logro: Reacci�n instant�nea
            if (_timer <= 1f)
            {
                AchievementManager.UnlockAchievement("Prehistory_ElFuego");
            }

            // Logro: Golpear todos los agujeros
            PrehistoryManager.Instance._holesHit.Add(holeIndex);

            // A�adir puntos
            PrehistoryManager.Instance.AddScore(_dinosaurPoints);

            // Se desaparece el dinosaurio
            HideDinosaur();
        }
    }


    private void HideDinosaur()
    {
        _hitCount = 0; // Reiniciar el contador de golpes
        _timer = 0; // Reiniciar el temporizador
        StartCoroutine(DisappearAnimation());
    }

    private IEnumerator DisappearAnimation()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = _initialPosition; // La posici�n original hacia la cual descender�
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
        // Una vez finalizada la animaci�n, se devuelve el dinosaurio al pool
        DinosaurPool.Instance.ReturnDinosaur(gameObject);
    }
}

