using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LocomotionController))]
public class MommyBehaviour : MonoBehaviour
{
    private LocomotionController _locomotionController;
    private AStarMind _pathController;

    private Tile _target;

    private void Awake()
    {
        _locomotionController = GetComponent<LocomotionController>();
        _pathController = GetComponent<AStarMind>();
    }

    private void Update()
    {
        // Si se ha terminado el movimiento anterior, se calcula uno nuevo en base al objetivo
        if(_locomotionController.finishedMove)
        {
            // Se calcula la casilla en la que se encuentra el agente
            // Para ello, se truncan los valores de su posición en los ejes x y z. A este último se le cambia el signo
            Vector2Int tilePos = new Vector2Int((int)transform.position.x, -(int)transform.position.z);
            Tile _currentTile = GridManager.Instance.GetTile(tilePos.x, tilePos.y);
            // Se calcula el objetivo
            SetTarget();
            // Se establece un nuevo movimiento en función de lo calculado con el algoritmo de búsqueda (Hill Climbing)
            _locomotionController.SetNewDirection(_pathController.GetNextMove(_currentTile, _target));
        }
    }

    private void SetTarget()
    {

    }
}
