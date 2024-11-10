using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrunkMovement : MonoBehaviour
{
    private float _speed = 3f; // Velocidad de movimiento del tronco
    private float _rotationSpeed = 100f; // Velocidad de rotación del tronco

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

        // Rota el tronco sobre su eje X
        transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);


        // Verifica si el tronco ha caído fuera del área, tanto en el cliente como en el servidor, para poder ocultarlo en ambos
        if (transform.position.y < -2f)
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        // Devuelve el tronco al pool
        TrunkPool.instance.ReturnTrunkToPool(gameObject);
    }
}
