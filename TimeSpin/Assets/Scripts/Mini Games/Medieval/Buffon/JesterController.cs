using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class JesterController : MonoBehaviour
{
    [SerializeField] private float _speed = 2f; // Velocidad del bufón
    private GameObject _targetSword = null;     // Espada objetivo
    private bool _isCarryingSword = false;
    private NavMeshAgent _agent;
    private Vector3 _wanderTarget;
    [SerializeField] private Vector3[] _wanderDestinies;

    public bool IsActive { get; private set; } = false;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _speed;
        WanderAround();
    }

    private void Update()
    {
        if (_isCarryingSword)
        {
            // Si lleva una espada, huye durante 3 segundos y luego destruye la espada
            if (!_agent.hasPath || _agent.remainingDistance < 1f)
            {
                WanderAround();
            }
        }
        else if (_targetSword != null)
        {
            // Si tiene una espada objetivo, se mueve hacia ella
            // Se debe comprobar si la espada objetivo ha sido tomada por el jugador. Verificando su altura se puede comprobar
            if (_targetSword.transform.position.y > 1f)
            {
                _targetSword = null;
            }
            else
            {
                _agent.SetDestination(_targetSword.transform.position);
            }
        }
        else
        {
            // Si no hay objetivo, sigue vagando
            if (!_agent.hasPath || _agent.remainingDistance < 1f)
            {
                WanderAround();
            }
        }
    }

    public void ActivateJester(GameObject targetSword)
    {
        if (!IsActive)
        {
            Debug.Log("Yendo a por espadas");
            _targetSword = targetSword;
            IsActive = true;
            _agent.SetDestination(_targetSword.transform.position);
        }
    }

    public bool HasTarget()
    {
        return _targetSword != null || _isCarryingSword;
    }

    public bool IsTargeting(GameObject sword)
    {
        return _targetSword == sword;
    }

    private void WanderAround()
    {
        // Si no hay un objetivo, define un nuevo destino aleatorio
        ChooseRandomWanderTarget();
        _agent.SetDestination(_wanderTarget);
    }

    private void ChooseRandomWanderTarget()
    {
        int randomIndex = Random.Range(0, _wanderDestinies.Length - 1);
        // Define un punto aleatorio dentro del área del mapa
        _wanderTarget = _wanderDestinies[randomIndex];
    }

    private void StealSword(GameObject sword)
    {
        _isCarryingSword = true;
        sword.transform.SetParent(transform);
        sword.transform.localPosition = new Vector3(0, 1, 0);
        sword.GetComponent<Rigidbody>().useGravity = false;
        sword.GetComponent<Collider>().enabled = false;

        StartCoroutine(DestroySwordAfterDelay(sword));
        _targetSword = null;
    }

    private IEnumerator DestroySwordAfterDelay(GameObject sword)
    {
        yield return new WaitForSeconds(3f);
        Destroy(sword);
        _isCarryingSword = false;
        IsActive = false;
        WanderAround();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si llega a la espada objetivo, la roba
        if (!_isCarryingSword && other.gameObject.tag == "Sword")
        {
            StealSword(other.gameObject);
        }
    }
}









