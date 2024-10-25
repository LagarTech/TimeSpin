using UnityEngine;

public class SwordController : MonoBehaviour
{
    public int swordPoints;  // Valor de la espada (por ejemplo, 10 puntos para bronce, 20 para plata)

    private MedievalGameManager gameManager;  // Referencia al GameManager

    void Start()
    {
        // Asigna automáticamente el GameManager al inicio
        gameManager = FindObjectOfType<MedievalGameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detecta si colisiona con el jugador
        MedievalPlayerController player = other.GetComponent<MedievalPlayerController>();

        // Comprueba si realmente hemos colisionado con el jugador y si no lleva una espada
        if (player != null && player.CarriedSword == null)
        {
            // Asigna esta espada al jugador
            player.SetCarriedSword(gameObject);
            transform.SetParent(player.transform);
            transform.localPosition = new Vector3(0, 1, 0);  // Ajusta la posición para que "lleve" la espada
        }
    }

    // Método que se llama cuando el jugador entrega la espada en la base
    public void DeliverSword()
    {
        // Suma los puntos al jugador
        gameManager.AddScore(swordPoints);

        // Destruye la espada después de la entrega
        Destroy(gameObject);
    }
}

