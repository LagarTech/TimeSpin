using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedievalPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform basePosition;  // Posición de la base del jugador
    private GameObject carriedSword = null;
    private MedievalGameManager gameManager;

    private int playerIndex;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<MedievalGameManager>();
    }

    void Update()
    {
        if (!gameManager.IsGameActive() || gameManager.IsGameOver())
            return;

        // Movimiento del jugador
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        transform.Translate(new Vector3(moveX, 0, moveZ));

        // Entregar espada en la base
        if (carriedSword != null && Vector3.Distance(transform.position, basePosition.position) < 2f)
        {
            DeliverSword();
        }
    }
    public GameObject CarriedSword
    {
        get { return carriedSword; }
    }

    public void SetCarriedSword(GameObject sword)
    {
        carriedSword = sword;
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Sword") && carriedSword == null)
        {
            // Recoger la espada
            carriedSword = other.gameObject;

            // Fijamos la espada como hija del jugador para que se mueva con él
            carriedSword.transform.SetParent(transform);

            // Reposicionamos la espada en una posición relativa al jugador (por ejemplo, sobre la cabeza)
            carriedSword.transform.localPosition = new Vector3(0, 1, 0);

            // Desactivar el Collider de la espada para que no siga siendo detectable como "Sword"
            carriedSword.GetComponent<Collider>().enabled = false;
        }
    }

    void DeliverSword()
    {
        if (carriedSword != null)
        {
            // Obtener los puntos de la espada
            SwordController sword = carriedSword.GetComponent<SwordController>();

            // Añadir los puntos a la puntuación del jugador
            gameManager.AddScore(playerIndex, sword.GetPoints());

            // Empezar el temporizador para que la puedan robar (10 segundos)
            sword.StartStealTimer(this, gameManager, playerIndex);

            // Liberar la espada de la "mano" del jugador, pero dejarla en el mapa
            carriedSword.transform.SetParent(null);
            carriedSword.transform.position = basePosition.position;  // Colocar espada en la base
            carriedSword.GetComponent<Collider>().enabled = true;  // Habilitar el collider de nuevo

            // Ya no llevamos la espada
            carriedSword = null;
        }
    }
}
