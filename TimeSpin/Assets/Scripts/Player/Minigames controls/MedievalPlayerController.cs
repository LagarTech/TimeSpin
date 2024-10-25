using UnityEngine;

public class MedievalPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;          // Velocidad de movimiento
    public GameObject CarriedSword { get; private set; }

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        // Obt�n las entradas de movimiento
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Calcula el movimiento en funci�n de la velocidad
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical) * moveSpeed * Time.deltaTime;

        // Aplica el movimiento al Rigidbody para mover al jugador
        rb.MovePosition(transform.position + movement);
    }

    // M�todo para asignar la espada al jugador
    public void SetCarriedSword(GameObject sword)
    {
        CarriedSword = sword;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detecta si el jugador est� en la base
        if (other.CompareTag("Base") && CarriedSword != null)
        {
            // Llama a DeliverSword() de SwordController y elimina la espada del jugador
            CarriedSword.GetComponent<SwordController>().DeliverSword();
            CarriedSword = null;
        }
    }
}

