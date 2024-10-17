using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(LocomotionController))]
public class MummyBehaviour : NetworkBehaviour
{
    private LocomotionController _locomotionController;
    private AStarMind _pathController;

    [SerializeField] private PlayerMovement _target; // Personaje objetivo al que va a perseguir
    [SerializeField] private int _numPlayers; // Número de jugadores en la escena
    private List<PlayerMovement> _players = new List<PlayerMovement>();
    private const float _timeToChangeTarget = 10f; // Tiempo que transcurre en el cambio de objetivos
    private float _currentTime = 0f;

    private Tile _currentTile;

    private void Awake()
    {
        _locomotionController = GetComponent<LocomotionController>();
        _pathController = GetComponent<AStarMind>();
    }

    // El movimiento de la momia sólo se realizará en el servidor, se sincroniza directamente en los clientes
    private void Start()
    {
        if (Application.platform != RuntimePlatform.LinuxServer) return;
        // Se evitan las colisiones con el resto de momias
        // Encuentra todos los objetos con la etiqueta "mummy"
        GameObject[] mummies = GameObject.FindGameObjectsWithTag("mummy");

        // Comprueba si hay alguna momia en la lista
        if (mummies.Length > 0)
        {
            // Desactiva la colisión con cada momia encontrada
            foreach (GameObject mummy in mummies)
            {
                // Asegúrate de que no se trata de la misma momia
                if (mummy != gameObject)
                {
                    // Asegúrate de que ambos objetos tengan un Collider
                    Collider thisCollider = GetComponent<SphereCollider>();
                    Collider mummyCollider = mummy.GetComponent<SphereCollider>();

                    // Verifica que ambos colliders no sean nulos antes de ignorar la colisión
                    if (thisCollider != null && mummyCollider != null)
                    {
                        Physics.IgnoreCollision(thisCollider, mummyCollider);
                    }
                }
            }
        }
        // Se establece un objetivo inicial
        UpdatePlayersList();
        SetTarget();
    }

    private void Update()
    {
        if (Application.platform != RuntimePlatform.LinuxServer) return;
        // Si el juego no está funcionando, no se ejecuta ninguna acción
        if (!GridManager.Instance.runningGame) return;
        _currentTime += Time.deltaTime; // Se actualiza el tiempo transcurrido
        // Si se ha terminado el movimiento anterior, se calcula uno nuevo en base al objetivo
        if(_locomotionController.finishedMove)
        {
            // Se calcula la casilla en la que se encuentra el agente
            // Para ello, se truncan los valores de su posición en los ejes x y z. A este último se le cambia el signo
            Vector2Int tilePos = new Vector2Int((int)transform.position.x, -(int)transform.position.z);
            _currentTile = GridManager.Instance.GetTile(tilePos.x, tilePos.y);
            // Si es tiempo de cambiar de objetivo, actualizar la lista de jugadores y seleccionar uno nuevo
            if (_currentTime >= _timeToChangeTarget)
            {
                UpdatePlayersList();
                SetTarget();
                _currentTime = 0f; // Reiniciar el temporizador
            }
            // Se toma la casilla en la que se encuentra el objetivo
            Tile targetTile = _target.GetCurrentTile();
            // Se establece un nuevo movimiento en función de lo calculado con el algoritmo de búsqueda
            _locomotionController.SetNewDirection(_pathController.GetNextMove(_currentTile, targetTile));
        }
    }

    private void UpdatePlayersList()
    {
        // Vacía la lista de jugadores actuales
        _players.Clear();
        // Se obtienen todos los jugadores de la escena
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        // Se añaden a la lista
        foreach(var player in playerObjects)
        {
            // Asegurarse que el jugador tiene el componente necesario
            if (player.GetComponent<PlayerMovement>() != null)
            {
                _players.Add(player.GetComponent<PlayerMovement>()); // Añadir el jugador válido a la lista
            }
        }
    }

    private void SetTarget()
    {
        if (_players.Count == 0) return; // Si no hay jugadores, no se asigna un objetivo
        _numPlayers = _players.Count;
        // Seleccionar un jugador aleatorio de la lista de jugadores conectados
        PlayerMovement targetPlayer = _players[Random.Range(0, _numPlayers)];

        if (targetPlayer != null)
        {
            _target = targetPlayer;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.SetActive(false);
            UpdatePlayersList();
            SetTarget();
            _currentTime = 0f; // Reiniciar el temporizador
            // Se avisa a los clientes para que oculten al jugador
            int clientID = collision.gameObject.GetComponent<PlayerMovement>().ownerClient;
            HidePlayerDefeatedClientRpc(clientID);
        }
    }

    [ClientRpc]
    private void HidePlayerDefeatedClientRpc(int clientID)
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in playerObjects)
        {
            // Se busca el jugador con el ID adecuado para ocultarlo
            if (player.GetComponent<PlayerMovement>().ownerClient == clientID)
            {
                player.SetActive(false);
            }
        }
    }
}
