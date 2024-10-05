using UnityEngine;

public class DinosaurController : MonoBehaviour
{
    public int points;               // Puntos del dinosaurio
    public float visibleTime = 1.5f; // Tiempo que estar� visible
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= visibleTime)
        {
            Destroy(gameObject);  // Se elimina despu�s de un tiempo
        }
    }

    void OnMouseDown()
    {
        // Al hacer clic sobre el dinosaurio
        GameObject gameManager = GameObject.Find("GameManager");
        gameManager.GetComponent<GameManager>().AddScore(points);

        Destroy(gameObject);  // Elimina el dinosaurio despu�s de pulsarlo
    }
}

