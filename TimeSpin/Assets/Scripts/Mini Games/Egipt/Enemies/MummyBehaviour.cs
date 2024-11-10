using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LocomotionController))]
public class MummyBehaviour : MonoBehaviour
{
    private LocomotionController _locomotionController;
    private AStarMind _pathController;

    [SerializeField] private PlayerMovement _target; // Personaje objetivo al que va a perseguir
    [SerializeField] private int _numPlayers; // Número de jugadores en la escena

    private Tile _currentTile;

    private void Awake()
    {
        _locomotionController = GetComponent<LocomotionController>();
        _pathController = GetComponent<AStarMind>();
    }

    // El movimiento de la momia sólo se realizará en el servidor, se sincroniza directamente en los clientes
    private void Start()
    {
        // Se establece un objetivo inicial
        _target = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        // Si el juego no está funcionando, no se ejecuta ninguna acción
        if (!GridManager.Instance.runningGame) return;
        // Si se ha terminado el movimiento anterior, se calcula uno nuevo en base al objetivo
        if(_locomotionController.finishedMove)
        {
            // Se calcula la casilla en la que se encuentra el agente
            // Para ello, se truncan los valores de su posición en los ejes x y z. A este último se le cambia el signo
            Vector2Int tilePos = new Vector2Int((int)transform.position.x, -(int)transform.position.z);
            _currentTile = GridManager.Instance.GetTile(tilePos.x, tilePos.y);

            // Se toma la casilla en la que se encuentra el objetivo
            Tile targetTile = _target.GetCurrentTile();
            // Se establece un nuevo movimiento en función de lo calculado con el algoritmo de búsqueda
            _locomotionController.SetNewDirection(_pathController.GetNextMove(_currentTile, targetTile));
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("pillado");
            // Se oculta el jugador, sin desactivarlo por completo
            collision.gameObject.GetComponent<PlayerMovement>().HidePlayer();
            // Se indica al gestor que se ha atrapado al jugador, por lo que debe terminar el juego
            GridManager.Instance.GameOver();
        }
    }
}
