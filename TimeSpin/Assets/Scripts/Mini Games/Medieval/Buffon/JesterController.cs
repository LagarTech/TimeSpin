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

    public bool IsActive { get; private set; } = false;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _speed;
        ChooseRandomWanderTarget();
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
            _agent.SetDestination(_targetSword.transform.position);
        }
        else
        {
            // Si no hay espadas, vaga por el mapa
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
            _targetSword = targetSword;
            IsActive = true;
            _agent.SetDestination(_targetSword.transform.position);
        }
    }

    private void WanderAround()
    {
        // Si no hay un objetivo, define un nuevo destino aleatorio
        ChooseRandomWanderTarget();
        _agent.SetDestination(_wanderTarget);
    }

    private void ChooseRandomWanderTarget()
    {
        // Define un punto aleatorio dentro del área del mapa
        _wanderTarget = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
    }

    private void StealSword()
    {
        if (_targetSword != null)
        {
            _isCarryingSword = true;
            _targetSword.transform.SetParent(transform);
            _targetSword.transform.localPosition = new Vector3(0, 1, 0);
            _targetSword.GetComponent<Rigidbody>().useGravity = false;
            _targetSword.GetComponent<Collider>().enabled = false;

            StartCoroutine(DestroySwordAfterDelay(_targetSword));
            _targetSword = null;
        }
    }

    private IEnumerator DestroySwordAfterDelay(GameObject sword)
    {
        yield return new WaitForSeconds(3f);
        Destroy(sword);
        _isCarryingSword = false;
        IsActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si llega a la espada objetivo, la roba
        if (other.gameObject == _targetSword && !_isCarryingSword)
        {
            StealSword();
        }
    }
}








