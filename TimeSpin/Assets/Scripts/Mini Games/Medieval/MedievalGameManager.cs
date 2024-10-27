using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MedievalGameManager : MonoBehaviour
{
    public GameObject[] swordPrefabs;       // Prefabs de espadas (bronce, plata, oro)
    public GameObject spawnArea;            // Donde aparecen las espadas
    public float spawnInterval = 3f;        // Eiwmpo de aparici?n de espadas
    public float gameTime = 60f;            // Duraci?n del minijuego
    public Transform baseZone;              // Donde el jugador lleva la espada

    private float timeLeft;
    private float spawnTimer;

    public bool isGameOver = false;
    public bool isGameActive = false;      // Controla si el juego est? activo

    public Text timeText;
    public Text scoreText;         // UI de la puntuaci?n de los jugadores
    private int score;                      // Puntuaci?n de los jugadores

    // Start is called before the first frame update
    void Start()
    {
        timeLeft = gameTime;
        UpdateUI();

        GameSceneManager.instance.practiceStarted = true;

        StartGame(); // Inicia autom?ticamente el juego al cargar la escena
    }

    // Update is called once per frame
    void Update()
    {
        // Si el juego no ha empezado, no hacer nada
        if (!isGameActive || isGameOver)
            return;

        // Timer
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval)
            {
                SpawnRandomSword();
                spawnTimer = 0f;
            }

            UpdateUI();
        }
        else
        {
            EndGame();
        }
    }
    //Spawn espadas
    void SpawnRandomSword()
    {
        int swordIndex = Random.Range(0, swordPrefabs.Length);  // Selecci?n de una espada aleatoria

        // Instancia la espada en una posici?n aleatoria dentro del ?rea de spawn
        Instantiate(swordPrefabs[swordIndex], GetRandomSpawnPosition(), Quaternion.identity);
    }

    Vector3 GetRandomSpawnPosition()
    {
        // Obtiene el MeshRenderer del ?rea de spawn para determinar su tama?o
        MeshRenderer meshRenderer = spawnArea.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Vector3 size = meshRenderer.bounds.size;
            Vector3 center = meshRenderer.bounds.center;
            // Genera una posici?n aleatoria dentro del ?rea del plano
            float x = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
            float z = Random.Range(center.z - size.z / 2, center.z + size.z / 2);
            return new Vector3(x, 0, z); // Asumiendo que Y=0 es el suelo
        }
        else
        {
            Debug.LogError("El objeto spawnArea no tiene un MeshRenderer.");
            return Vector3.zero; // Retorna un vector nulo si no se encuentra el MeshRenderer
        }
    }

    //Puntuaciones
    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }

    void UpdateUI()
    {
        // Verificar que el tiempo y la puntuaci?n son v?lidos
        if (timeText != null && scoreText != null)
        {
            // Tiempo restante en minutos y segundos
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);

            // Formateo el texto 
            timeText.text = string.Format("{0:0}:{1:00}", minutes, seconds);

            // Puntuaci?n
            scoreText.text = "Puntuaci?n: " + score;
        }
        else
        {
            Debug.LogError("Los objetos UI no est?n asignados correctamente.");
        }
    }




    void EndGame()
    {
        isGameOver = true;
        isGameActive = false;
        Debug.Log("El juego ha terminado");

        ReturnToMainMenu();
    }

    void ReturnToMainMenu()
    {
        // Cargar la escena del men? principal
        SceneManager.LoadScene("LobbyMenu");  
    }

    public void StartGame()
    {
        isGameActive = true;
        isGameOver = false;
        timeLeft = gameTime;   // Reiniciar el tiempo
        score = 0;             // Reiniciar la puntuaci?n
        UpdateUI();            // Actualizar la interfaz
    }
}
