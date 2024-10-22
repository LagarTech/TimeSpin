using System.Collections;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    public int swordPoints;  // Valor de la espada (bronce, plata, oro)

    private bool canBeStolen = true;  // Indica si la espada puede ser robada
    private MedievalGameManager gameManager;
    private int ownerPlayerIndex;     // Índice del jugador que ha entregado la espada en base

    // Devuelve los puntos de la espada
    public int GetPoints()
    {
        return swordPoints;
    }

    // Comienza el temporizador para permitir que la roben durante 10 segundos
    public void StartStealTimer(MedievalPlayerController player, MedievalGameManager gameManager, int playerIndex)
    {
        this.gameManager = gameManager;
        this.ownerPlayerIndex = playerIndex;

        // Empezamos el temporizador de 10 segundos
        StartCoroutine(StealWindow());
    }

    // Corutina que espera 10 segundos antes de eliminar la espada
    private IEnumerator StealWindow()
    {
        canBeStolen = true;

        // Espera 10 segundos
        yield return new WaitForSeconds(10f);

        // Después de los 10 segundos, la espada ya no puede ser robada
        canBeStolen = false;

        // Eliminar la espada definitivamente del mapa
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Intentamos obtener el componente MedievalPlayerController del objeto con el que colisionamos
        MedievalPlayerController player = other.GetComponent<MedievalPlayerController>();

        // Comprobamos si realmente hemos colisionado con un jugador
        if (player != null)
        {
            // Si el jugador no lleva ninguna espada
            if (player.CarriedSword == null)
            {
                // El jugador roba la espada
                player.SetCarriedSword(gameObject);  // Usamos el método público para asignar la espada
                transform.SetParent(player.transform);
                transform.localPosition = new Vector3(0, 1, 0);

                // Eliminar los puntos del jugador original (dueño de la espada)
                gameManager.AddScore(ownerPlayerIndex, -swordPoints);

                // El nuevo jugador la lleva para puntuar
                canBeStolen = false;  // Ya no puede ser robada de nuevo
            }
        }
    }

}
