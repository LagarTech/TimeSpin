using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MedievalGameManager : MonoBehaviour
{
    public GameObject[] swordPrefabs;       // Prefabs de espadas (bronce, plata, oro)
    public GameObject spawnArea;            // Donde aparecen las espadas
    public float spawnInterval = 3f;        // Tiempo de aparición de espadas
    public float gameTime = 60f;            // Duración del minijuego
    public Transform[] playerBases;         // Bases de cada jugador
    public float swordHoldTime = 5f;        // Tiempo que la espada debe quedarse en la base para sumar puntos

    private float timeLeft;
    private float spawnTimer;

    public bool isGameOver = false;
    public bool isGameActive = false;

    public Text timeText;
    public Text[] scoreTexts;               // UI de las puntuaciones de cada jugador
    private int[] scores = new int[4];      // Puntuaciones de cada jugador

    private void Start()
    {
        timeLeft = gameTime;
        UpdateUI();

        GameSceneManager.instance.practiceStarted = true;

        StartGame(); // Inicia automáticamente el juego al cargar la escena
    }

    private void Update()
    {
        if (!isGameActive || isGameOver) return;

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

    void SpawnRandomSword()
    {
        int swordIndex = Random.Range(0, swordPrefabs.Length);
        Instantiate(swordPrefabs[swordIndex], GetRandomSpawnPosition(), Quaternion.identity);
    }

    Vector3 GetRandomSpawnPosition()
    {
        MeshRenderer meshRenderer = spawnArea.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Vector3 size = meshRenderer.bounds.size;
            Vector3 center = meshRenderer.bounds.center;
            float x = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
            float z = Random.Range(center.z - size.z / 2, center.z + size.z / 2);
            return new Vector3(x, 0, z);
        }
        else
        {
            Debug.LogError("El objeto spawnArea no tiene un MeshRenderer.");
            return Vector3.zero;
        }
    }

    public void AddScore(int playerIndex, int points)
    {
        scores[playerIndex] += points;
        UpdateUI();
    }

    void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);
        timeText.text = string.Format("{0:0}:{1:00}", minutes, seconds);

        for (int i = 0; i < scores.Length; i++)
        {
            scoreTexts[i].text = "Jugador " + (i + 1) + ": " + scores[i];
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
        SceneManager.LoadScene("LobbyMenu");
    }

    public void StartGame()
    {
        isGameActive = true;
        isGameOver = false;
        timeLeft = gameTime;
        scores = new int[4];
        UpdateUI();
    }
}
