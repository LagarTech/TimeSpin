using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocomotionController : MonoBehaviour
{
    // Esta variable se utiliza para indicar hasta dónde se debe mover la momia en cada movimiento
    private Vector3 _targetPosition;
    // Esta variable sirve para determinar la dirección de movimiento del agente
    private Vector3 _moveDirection;
    [SerializeField] private float _speed = 1f;
    private float _timer = 0f;
    // Esta variable indica si se ha finalizado el movimiento, es decir, se ha llegado a la casilla calculada
    public bool finishedMove;
    // Enumerador con las direcciones en las que se puede mover el agente
    public enum MoveDirection
    {
        Up,
        Down,
        Left,
        Right, None
    }

    private void Start()
    {
        // Por defecto, se indica que se ha terminado un movimiento, para calcular otro nuevo
        finishedMove = true;
        // Se inicializa a cero el objetivo
        _targetPosition = Vector3.zero;
    }

    private void Update()
    {
        // Se evitan los movimientos si el juego no ha comenzado
        if (!GridManager.Instance.runningGame) return;

        // Se cuenta el tiempo transcurrido para aumentar su velocidad
        _timer += Time.deltaTime;
        float increaseSpeed = _timer / 200f;

        if (_targetPosition == Vector3.zero) return; // Cuando no se ha iniciado el movimiento, no se pueden realizar las comprobaciones
        // Se comprueba si se ha llegado al destino
        if(!AtDestination())
        {
            // Si no, se indica que el movimiento no se ha terminado
            finishedMove = false;
            // Se continúa moviendo
            transform.position += _moveDirection * (_speed + increaseSpeed) * Time.deltaTime;
        }
        else
        {
            // Se indica que se ha terminado el movimiento, lo que va a poder calcular una nueva dirección
            finishedMove = true;
            transform.position = _targetPosition;
        }
    }


    // Con esta función se establece un nuevo movimiento, en función de lo calculado
    public void SetNewDirection(MoveDirection newDirection)
    {
        switch (newDirection)
        {
            case MoveDirection.Up: 
                MoveUp(); 
                break;
            case MoveDirection.Down: 
                MoveDown(); 
                break;
            case MoveDirection.Left:
                MoveLeft();
                break;
            case MoveDirection.Right:
                MoveRight();
                break;
            case MoveDirection.None:
                break;
        }
    }

    // Estas funciones se utilizan para establecer el nuevo objetivo de movimiento de la momia
    private void MoveUp()
    {
        _moveDirection = new Vector3(0, 0, 1);
        _targetPosition = transform.position + new Vector3(0, 0, 1);
    }

    private void MoveDown()
    {
        _moveDirection = new Vector3(0, 0, -1);
        _targetPosition = transform.position + new Vector3(0, 0, -1);
    }

    private void MoveLeft()
    {
        _moveDirection = new Vector3(-1, 0, 0);
        _targetPosition = transform.position + new Vector3(-1, 0, 0);
    }

    private void MoveRight()
    {
        _moveDirection = new Vector3(1, 0, 0);
        _targetPosition = transform.position + new Vector3(1, 0, 0);
    }

    // Función para calcular si el agente ha llegado a su destino
    private bool AtDestination()
    {
        if (Vector3.Distance(transform.position, _targetPosition) < 0.01)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
