using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementEgipt : MonoBehaviour
{
    private Vector3 _movementDirection = Vector3.zero;
    private float _speed = 1f;

    [SerializeField] private Tile _currentTile;

    // Update is called once per frame
    void Update()
    {
        // Gestión de los controles
        _movementDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
        if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
        if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
        if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;

        transform.position += _movementDirection * _speed * Time.deltaTime;

        // Se obtiene la casilla en la que se encuentra el jugador, para que el agente pueda realizar la búsqueda
        // Para ello, se truncan los valores de su posición en los ejes x y z. A este último se le cambia el signo
        Vector2Int tilePos = new Vector2Int((int)transform.position.x, -(int)transform.position.z);
        _currentTile = GridManager.Instance.GetTile(tilePos.x, tilePos.y);
    }
}
