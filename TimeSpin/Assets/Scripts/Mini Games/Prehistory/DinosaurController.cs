using UnityEngine;

public class DinosaurController : MonoBehaviour
{
    public int points;               // Puntos del dinosaurio
    public float visibleTime = 1.5f; // Tiempo que estará visible
    private float timer;
    public string dinosaurType;         // Tipo de dinosaurio (Normal, Velociraptor, TRex)

    public Transform player;         // Referencia al jugador
    public float hitDistance = 2.0f; // Distancia mínima para golpear al dinosaurio

    private int hitCount = 0;           // Cuenta los clicks (para el T-Rex)
    public int requiredHits = 1;        // Golpes minimos para matar al dinosaurio (1 por defecto)

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager")?.GetComponent<GameManager>();

        // Si el dinosaurio es un T-Rex, necesita dos golpes
        if (dinosaurType == "TRex")
        {
            requiredHits = 2;
        }

        // Encontramos al jugador en la escena, por si no lo hemos asignado manualmente en el inspector
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        // Si el juego no ha empezado o ha terminado, que no se pueda hacer nada
        if (!gameManager.IsGameActive() || gameManager.IsGameOver())
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer >= visibleTime)
        {
            Destroy(gameObject);  // Se elimina después de un tiempo
        }
    }

    void OnMouseDown()
    {
        // Comprobar si el jugador esta cerca para pincharle
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if (distanceToPlayer <= hitDistance)
        {
            // El jugador esta cerca, lo golpea 
            HitDinosaur();
        }
        else
        {
            // el jugador esta lejos
            Debug.Log("Estás demasiado lejos para golpear el dinosaurio.");
        }
    }

    void HitDinosaur()
    {
        hitCount++;

        if (hitCount >= requiredHits)
        {
            // Se suman los puntos al pinchar el dinosaurio
            GameObject gameManager = GameObject.Find("GameManager");
            gameManager.GetComponent<GameManager>().AddScore(points);

            Destroy(gameObject);
        }
        else
        {
            Debug.Log("El TRex ha sido golpeado, pero aún necesita más golpes.");
        }
    }
}

