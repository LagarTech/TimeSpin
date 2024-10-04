using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LocomotionController))]
public class MommyBehaviour : MonoBehaviour
{
    private LocomotionController _locomotionController;
    private AStarMind _pathController;

    private PlayerMovementEgipt _target; // Personaje objetivo al que va a perseguir
    private List<PlayerMovementEgipt> _players = new List<PlayerMovementEgipt>();
    private const float _timeToChangeTarget = 10f; // Tiempo que transcurre en el cambio de objetivos
    private float _currentTime = 0f;

    private void Awake()
    {
        _locomotionController = GetComponent<LocomotionController>();
        _pathController = GetComponent<AStarMind>();
    }

    private void Start()
    {
        // Se establece un objetivo inicial
        UpdatePlayersList();
        SetTarget();
    }

    private void Update()
    {
        _currentTime += Time.deltaTime; // Se actualiza el tiempo transcurrido
        // Si se ha terminado el movimiento anterior, se calcula uno nuevo en base al objetivo
        if(_locomotionController.finishedMove)
        {
            // Se calcula la casilla en la que se encuentra el agente
            // Para ello, se truncan los valores de su posici�n en los ejes x y z. A este �ltimo se le cambia el signo
            Vector2Int tilePos = new Vector2Int((int)transform.position.x, -(int)transform.position.z);
            Tile _currentTile = GridManager.Instance.GetTile(tilePos.x, tilePos.y);
            // Si es tiempo de cambiar de objetivo, actualizar la lista de jugadores y seleccionar uno nuevo
            if (_currentTime >= _timeToChangeTarget)
            {
                UpdatePlayersList();
                SetTarget();
                _currentTime = 0f; // Reiniciar el temporizador
            }
            // Se toma la casilla en la que se encuentra el objetivo
            Tile targetTile = _target.GetCurrentTile();
            // Se establece un nuevo movimiento en funci�n de lo calculado con el algoritmo de b�squeda
            _locomotionController.SetNewDirection(_pathController.GetNextMove(_currentTile, targetTile));
        }
    }

    private void UpdatePlayersList()
    {
        // Vac�a la lista de jugadores actuales
        _players.Clear();
        // Se obtienen todos los jugadores de la escena
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        // Se a�aden a la lista
        foreach(var player in playerObjects)
        {
            // Asegurarse que el jugador tiene el componente necesario
            if (player.GetComponent<PlayerMovementEgipt>() != null)
            {
                _players.Add(player.GetComponent<PlayerMovementEgipt>()); // A�adir el jugador v�lido a la lista
            }
        }
    }

    private void SetTarget()
    {
        if (_players.Count == 0) return; // Si no hay jugadores, no se asigna un objetivo

        // Seleccionar un jugador aleatorio de la lista de jugadores conectados
        PlayerMovementEgipt targetPlayer = _players[Random.Range(0, _players.Count)];

        if (targetPlayer != null)
        {
            _target = targetPlayer;
        }
    }
}
