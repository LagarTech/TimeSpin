using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject[] dinosaurPrefabs;  // Prefabs de los dinosaurios
    public Transform[] holes;             // Posiciones de los agujeros
    public float spawnInterval = 2f;      // Intervalo de aparición
    public float gameTime = 60f;          // Duración del juego

    private float timeLeft;
    private float spawnTimer;

    public bool isGameOver = false;      // Controla si el juego ha terminado
    public bool isGameActive = false;    // Controla si el juego está activo

    public Text timeText;                 // UI del tiempo
    public Text scoreText;                // UI de la puntuación
    private int score;

    void Start()
    {
        timeLeft = gameTime;
        UpdateUI();

        StartGame(); // Inicia automáticamente el juego al cargar la escena
    }

    void Update()
    {
        // Si el juego no ha empezado, no hace na
        if (!isGameActive || isGameOver)
            return;

        // Timer
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval)
            {
                SpawnRandomDinosaur();
                spawnTimer = 0f;
            }

            UpdateUI();
        }
        else
        {
            // Fin
            EndGame();
        }
    }

    void SpawnRandomDinosaur()
    {
        int holeIndex = Random.Range(0, holes.Length);  // Selección de un agujero aleatorio
        int dinoIndex = Random.Range(0, dinosaurPrefabs.Length);  // Selección de un dinosaurio aleatorio

        Instantiate(dinosaurPrefabs[dinoIndex], holes[holeIndex].position, Quaternion.identity);
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }

    void UpdateUI()
    {
        // Tiempo restante en minutos y segundos
        int minutes = Mathf.FloorToInt(timeLeft / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);

        // Formateo el texto 
        timeText.text = string.Format("{0:0}:{1:00}", minutes, seconds);

        //Puntuacion
        scoreText.text = "Puntuación: " + score;
    }

    // Detengo el juego cuando se acabe el tiempo
    void EndGame()
    {
        isGameOver = true;
        isGameActive = false;
        Debug.Log("¡El juego ha terminado!");

        // Añadir la lógica para regresar al menú principal
        ReturnToMainMenu();
    }

    void ReturnToMainMenu()
    {
        // Cargar la escena del menú principal
        SceneManager.LoadScene("LobbyMenu");  // Asegúrate de que la escena se llame "MainMenu"
    }

    // Método para comenzar el juego
    public void StartGame()
    {
        isGameActive = true;
        isGameOver = false;
        timeLeft = gameTime;   // Reiniciar el tiempo
        score = 0;             // Reiniciar la puntuación
        UpdateUI();            // Actualizar la interfaz
    }
    public bool IsGameOver()
    {
        return isGameOver;
    }

    public bool IsGameActive()
    {
        return isGameActive;
    }

}