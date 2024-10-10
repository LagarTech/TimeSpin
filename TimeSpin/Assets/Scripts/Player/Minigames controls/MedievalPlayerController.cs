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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Sword") && carriedSword == null)
        {
            // Recoger espada si no llevamos una
            carriedSword = other.gameObject;
            carriedSword.transform.SetParent(transform);
            carriedSword.transform.localPosition = new Vector3(0, 1, 0);
        }
    }

    void DeliverSword()
    {
        if (carriedSword != null)
        {
            SwordController sword = carriedSword.GetComponent<SwordController>();
            gameManager.AddScore(playerIndex, sword.GetPoints());
            Destroy(carriedSword);
        }
    }
}
