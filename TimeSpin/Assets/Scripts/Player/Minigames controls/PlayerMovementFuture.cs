using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementFuture : MonoBehaviour
{
    private Vector3 _movementDirection = Vector3.zero;
    private float _speed = 1f;

    // Variable que controla la rotación del personaje
    [SerializeField] private bool _startedRotation = false;
    // Rotación del personaje
    private Quaternion _targetRotation;

    private void Update()
    {
        // Si el juego no está funcionando, se evita realizar movimientos
        if (!GravityManager.Instance.runningGame) return;

        // Se comprueba que el jugador no ha salido de los límites establecidos, es decir, que no se ha caído
        if(transform.position.y < -5f || transform.position.y > 15f)
        {
            // Si se ha caído, se desactiva
            gameObject.SetActive(false);
        }

        // Mientras los jugadores estén flotando, no se podrán controlar
        if (GravityManager.Instance.floating && !_startedRotation)
        {
            // Se indica que ha comenzado la rotación, y se calcula la rotación destino
            _startedRotation = true;
            RotatePlayerToGround();
            return;
        }
        else if (GravityManager.Instance.floating && _startedRotation)
        {
            // Aplicar rotación suavemente, mientras que los jugadores están flotando
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, Time.deltaTime * (180 / GravityManager.Instance._floatTime));
            return;
        }

        // Se indica que ya se terminó la rotación
        _startedRotation = false;

        // Gestión de los controles
        _movementDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
        if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
        if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
        if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;

        transform.position += _movementDirection * _speed * Time.deltaTime;
    }

    private void RotatePlayerToGround()
    {
        // Calcular la rotación objetivo (180 grados en el eje X)
        _targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x + 180 % 360, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }
}
