using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrunkMovement : MonoBehaviour
{
    private float _speed = 2.5f; // Velocidad de movimiento del tronco
    private float _rotationSpeed = 100f; // Velocidad de rotación del tronco

    private void Update()
    {
        // Mueve el tronco hacia atrás continuamente
        transform.position += Vector3.back * _speed * Time.deltaTime;

        // Rota el tronco sobre su eje X
        transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
    }
}
