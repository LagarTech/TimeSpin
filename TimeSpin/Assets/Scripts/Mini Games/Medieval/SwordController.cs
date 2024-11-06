using UnityEngine;
using System.Collections;

public class SwordController : MonoBehaviour
{
    public float swordHoldTime = 5f;  // Tiempo que la espada se mantiene en la base
    private bool isHeldAtBase = false;
    private int lastPlayerIndex = -1;
    public int swordPoints = 0;
    private MedievalGameManager gameManager;


    private void Start()
    {
        gameManager = FindObjectOfType<MedievalGameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        MedievalPlayerController player = other.GetComponent<MedievalPlayerController>();

        if (player != null && player.CarriedSword == null && !isHeldAtBase)
        {
            player.SetCarriedSword(gameObject);
            transform.SetParent(player.transform);
            transform.localPosition = new Vector3(0, 1, 0);
        }
    }

    public void DeliverSword(int playerIndex)
    {
        if (isHeldAtBase) return;  // No hacer nada si ya está siendo retenida

        isHeldAtBase = true;
        lastPlayerIndex = playerIndex;

        // Mueve la espada a la base del jugador
        transform.position = gameManager.playerBases[playerIndex].position;
        transform.parent = gameManager.playerBases[playerIndex]; // Fija la espada en la base

        // Inicia la corutina para mantener la espada en la base
        StartCoroutine(HoldSwordAtBase(playerIndex));

    }
    private IEnumerator HoldSwordAtBase(int playerIndex)
    {
        // Espera 5 segundos mientras la espada está en la base
        yield return new WaitForSeconds(swordHoldTime);

        // Si nadie la ha robado en ese tiempo, otorga puntos al jugador y elimina la espada
        if (isHeldAtBase)
        {
            gameManager.AddScore(playerIndex, swordPoints); // Agrega puntos al jugador
            Destroy(gameObject);  // Elimina la espada del juego
        }
    }

    public void StealSword()
    {
        if (isHeldAtBase)
        {
            isHeldAtBase = false;
            lastPlayerIndex = -1;

            // Despega la espada de la base para que pueda ser llevada por otro jugador
            transform.parent = null;
        }
    }

}





