using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MedievalGameManager : MonoBehaviour
{
    public GameObject[] swordPrefabs;       // Prefabs de espadas (bronce, plata, oro)
    public Transform[] spawnPoints;         // Puntos de spawns de las espadas
    public float spawnInterval = 3f;        // tiwmpo de aparici�n de espadas
    public float gameTime = 60f;            // Duraci�n del minijuego

    private float timeLeft;
    private float spawnTimer;
    public bool isGameOver = false;
    public bool isGameActive = false;      // Controla si el juego est� activo

    public Text timeText;
    public Text[] playerScoreTexts;         // UI de la puntuaci�n de los jugadores
    private int[] playerScores;             // Puntuaci�n de los jugadores

    // Start is called before the first frame update
    void Start()
    {
        timeLeft = gameTime;
        playerScores = new int[1];  // Asumiendo 2 jugadores para este minijuego 
        UpdateUI();

        StartGame(); // Inicia autom�ticamente el juego al cargar la escena
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
        int spawnIndex = Random.Range(0, spawnPoints.Length);   // Posici�n aleatoria
        int swordIndex = Random.Range(0, swordPrefabs.Length);  // Espada aleatoria

        Instantiate(swordPrefabs[swordIndex], spawnPoints[spawnIndex].position, Quaternion.identity);
    }

    //Puntuaciones
    public void AddScore(int playerIndex, int points)
    {
        playerScores[playerIndex] += points;
        // Para que la puntuaci�n no sea negativa
        if (playerScores[playerIndex] < 0)
        {
            playerScores[playerIndex] = 0;
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);

        timeText.text = string.Format("{0:0}:{1:00}", minutes, seconds);

        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScoreTexts[i].text = "Jugador " + (i + 1) + ": " + playerScores[i] + " puntos";
        }
    }




    void EndGame()
    {
        isGameOver = true;
        isGameActive = false;
        Debug.Log("El juego ha terminado");

        // A�adir la l�gica para regresar al men� principal
        ReturnToMainMenu();
    }

    void ReturnToMainMenu()
    {
        // Cargar la escena del men� principal
        SceneManager.LoadScene("MainMenu");  // Aseg�rate de que la escena se llame "MainMenu"
    }

    public void StartGame()
    {
        isGameActive = true;
        isGameOver = false;
        timeLeft = gameTime;
        playerScores = new int[2];
        UpdateUI();
    }

    public bool IsGameActive()
    {
        return isGameActive;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
}
