using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementFuture : MonoBehaviour
{
    private Vector3 _movementDirection = Vector3.zero;
    private float _speed = 1f;

    // Variable que controla la rotaci�n del personaje
    [SerializeField] private bool _startedRotation = false;
    // Rotaci�n del personaje
    private Quaternion _targetRotation;

    private void Update()
    {
        // Si el juego no est� funcionando, se evita realizar movimientos
        if (!GravityManager.Instance.runningGame) return;

        // Se comprueba que el jugador no ha salido de los l�mites establecidos, es decir, que no se ha ca�do
        if(transform.position.y < -5f || transform.position.y > 15f)
        {
            // Si se ha ca�do, se desactiva
            gameObject.SetActive(false);
        }

        // Mientras los jugadores est�n flotando, no se podr�n controlar
        if (GravityManager.Instance.floating && !_startedRotation)
        {
            // Se indica que ha comenzado la rotaci�n, y se calcula la rotaci�n destino
            _startedRotation = true;
            RotatePlayerToGround();
            return;
        }
        else if (GravityManager.Instance.floating && _startedRotation)
        {
            // Aplicar rotaci�n suavemente, mientras que los jugadores est�n flotando
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, Time.deltaTime * (180 / GravityManager.Instance._floatTime));
            return;
        }

        // Se indica que ya se termin� la rotaci�n
        _startedRotation = false;

        // Gesti�n de los controles
        _movementDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
        if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
        if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
        if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;

        transform.position += _movementDirection * _speed * Time.deltaTime;
    }

    private void RotatePlayerToGround()
    {
        // Calcular la rotaci�n objetivo (180 grados en el eje X)
        _targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x + 180 % 360, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }
}
