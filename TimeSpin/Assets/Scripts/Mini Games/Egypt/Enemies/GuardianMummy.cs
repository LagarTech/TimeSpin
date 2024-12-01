using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianMummy : MonoBehaviour
{
    [SerializeField] private Vector3[] _positions;
    [SerializeField] private Vector3[] _velocitys;
    [SerializeField] private int _currentGoal;
    [SerializeField] private float _speed = 1f;


    void Update()
    {
        if (!GridManager.Instance.runningGame) return;

        // Se comprueba si se ha llegado al destino
        if (!AtDestination())
        {
            // Se continúa moviendo
            transform.position += _velocitys[_currentGoal] * _speed * Time.deltaTime;

            // Rotación suave hacia la dirección de movimiento
            RotateTowards(_velocitys[_currentGoal]);
        }
        else
        {
            // Se indica que se ha terminado el movimiento, lo que va a poder calcular una nueva dirección
            transform.position = _positions[_currentGoal];
            _currentGoal = (_currentGoal+ 1) % _positions.Length; // Se da un nuevo objetivo
        }
    }

    private bool AtDestination()
    {
        if (Vector3.Distance(transform.position, _positions[_currentGoal]) < 0.25)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void RotateTowards(Vector3 direction)
    {
        // Calcula la rotación deseada en base a la dirección de movimiento
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Mantén la rotación del eje X original del agente
        targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);

        // Aplica una rotación suave hacia el objetivo
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 180 * Time.deltaTime);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // Se oculta el jugador, sin desactivarlo por completo
            collision.gameObject.GetComponent<PlayerMovement>().HidePlayer();
            // Se indica al gestor que se ha atrapado al jugador, por lo que debe terminar el juego
            GridManager.Instance.GameOver();
            GridManager.Instance.PlayerCaught = true;
        }
    }
}
