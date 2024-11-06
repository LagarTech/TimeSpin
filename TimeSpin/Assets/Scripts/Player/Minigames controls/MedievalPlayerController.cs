using UnityEngine;

public class MedievalPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;                    // Velocidad de movimiento
    public GameObject CarriedSword { get; private set; }
    public int playerIndex;                         // Índice del jugador (0 a 3)

    private Rigidbody rb;
    private MedievalGameManager gameManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<MedievalGameManager>();
    }

    void Update()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        //Movimiento
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");


        // Calcula el movimiento en función de la velocidad
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical) * moveSpeed * Time.deltaTime;

        // Aplica el movimiento al Rigidbody para mover al jugador
        rb.MovePosition(transform.position + movement);
    }

    // Método para asignar la espada al jugador
    public void SetCarriedSword(GameObject sword)
    {
        CarriedSword = sword;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el jugador ha entrado en su propia base
        if (CarriedSword != null && other.transform == gameManager.playerBases[playerIndex])
        {
            // Llama a DeliverSword() de SwordController y elimina la espada del jugador
            CarriedSword.GetComponent<SwordController>().DeliverSword(playerIndex);
            CarriedSword = null;
        }
    }
}


