using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prehistory : MonoBehaviour
{
    //Timer
    public float gameTime = 60f;  // Duración del juego
    private float timeLeft;
    public Text timeText;

    //Puntuacion
    public int score = 0;
    public Text scoreText;

    //Dinosaurios
    public GameObject[] dinosaurPrefabs;  // Array de prefabs de dinosaurios
    public Transform[] holes;             // Posiciones de los agujeros
    public float spawnInterval = 2f;      // Intervalo de aparición
    private float spawnTimer;

    //Jugador
    public float hitDistance = 2.0f;  // Distancia para que sea válido el golpe
    public Transform player;          // Referencia al jugador


    // Start is called before the first frame update
    void Start()
    {
        timeLeft = gameTime;
    }

    // Update is called once per frame
    void Update()
    {
        //Timer
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            timeText.text = "Time: " + Mathf.Round(timeLeft);
        }
        else
        {
            // Termina el juego
        }

        //Dinosaurios
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnRandomDinosaur();
            spawnTimer = 0;
        }

        //Tiempos y puntuacion
        timeText.text = "Tiempo: " + Mathf.Round(timeLeft);
        scoreText.text = "Puntos: " + score;
    }

    //El jugador ganará puntos dependiendo del dinosaurio que golpee
    public void AddScore(int points)
    {
        score += points;
        scoreText.text = "Score: " + score;
    }

    //Se spawnean los dinosaurios
    void SpawnRandomDinosaur()
    {
        int holeIndex = Random.Range(0, holes.Length);  // Escoge un agujero aleatorio
        int dinoIndex = Random.Range(0, dinosaurPrefabs.Length);  // Escoge un dinosaurio aleatorio

        Instantiate(dinosaurPrefabs[dinoIndex], holes[holeIndex].position, Quaternion.identity);
    }

    //Cuando el jugador hace clic en un dinosaurio, se debe verificar la distancia entre el jugador y el dinosaurio
    void OnMouseDown()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= hitDistance)
        {
            // Golpear dinosaurio
            HitDinosaur();
        }
    }
    void HitDinosaur()
    {
        if (gameObject.CompareTag("Velociraptor"))
        {
            // Añade 2 puntos
            AddScore(2);
        }
        else if (gameObject.CompareTag("TRex"))
        {
            // Si es un T-Rex, necesita ser golpeado dos veces
            AddScore(3);
        }
        else
        {
            // Añade 1 punto
            AddScore(1);
        }
        Destroy(gameObject);  // Elimina el dinosaurio
    }
}
