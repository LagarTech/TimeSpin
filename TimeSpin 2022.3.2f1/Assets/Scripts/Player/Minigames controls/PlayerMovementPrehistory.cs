using UnityEngine;

public class PlayerMovementPrehistory : MonoBehaviour
{
    public float moveSpeed = 5f;
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager")?.GetComponent<GameManager>();

    }
    void Update()
    {
        // Si el juego no ha empezado o ha terminado, que no se pueda hacer nada
        if (!gameManager.IsGameActive() || gameManager.IsGameOver())
        {
            return;
        }

        // Movimiento del jugador
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        transform.Translate(new Vector3(moveX, 0, moveZ));
    }
}

