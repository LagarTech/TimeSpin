using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrunkMovement : MonoBehaviour
{
    private float _speed = 4f; // Velocidad de movimiento del tronco
    private float _rotationSpeed = 60f; // Velocidad de rotación del tronco

    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        TrunkPool.instance.AddTrunkToPool(gameObject);
    }

    private void Update()
    {
        if (!RaceManager.instance.runningGame)
        {
            _rb.velocity = Vector3.zero;
            return;
        }

        // Mueve el tronco hacia atrás continuamente
        transform.position += Vector3.back * _speed * Time.deltaTime;

        // Rota el tronco alrededor de su propio eje Z
        transform.Rotate(Vector3.forward * _rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void ReturnToPool()
    {
        // Devuelve el tronco al pool
        TrunkPool.instance.ReturnTrunkToPool(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Limite")
        {
            ReturnToPool();
        }
    }
}
