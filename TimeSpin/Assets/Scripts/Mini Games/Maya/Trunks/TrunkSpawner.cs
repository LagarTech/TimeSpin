using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrunkSpawner : MonoBehaviour
{
    // Extremos para la generaci�n aleatoria de los troncos
    private float _minX = -2.8f;
    private float _maxX = 2.8f;

    private float _spawnInterval = 1.5f; // Tiempo entre spawns
    private float _spawnTimer = 0f;   // Temporizador para controlar el spawn

    private void Update()
    {
        if (!RaceManager.instance.runningGame) return;
        // Incrementa el temporizador basado en el tiempo real transcurrido
        _spawnTimer += Time.deltaTime;

        // Si el temporizador supera el intervalo, genera un tronco
        if (_spawnTimer >= _spawnInterval)
        {
            GenerateTrunk();
            _spawnTimer = 0f; // Reinicia el temporizador
        }
    }

    void GenerateTrunk()
    {
        // Genera una posici�n aleatoria en el eje X
        float randomX = Random.Range(_minX, _maxX);

        // Define la posici�n de generaci�n (en Z = 53)
        Vector3 spawnPosition = new Vector3(randomX, 2f, 53f);  // Ajusta seg�n el escenario
        Quaternion spawnRotation = Quaternion.Euler(0, 90, 90); // Rotaci�n en Z

        // Instancia el tronco 
        GameObject newTrunk = TrunkPool.instance.GetTrunkFromPool();

        if (newTrunk != null)
        {
            // Ubica el tronco en la posici�n inicial
            newTrunk.transform.position = spawnPosition;
            newTrunk.transform.rotation = spawnRotation;
        }

        // Aplica una fuerza para que ruede hacia el inicio (en Z = 0)
        Rigidbody trunkRb = newTrunk.GetComponent<Rigidbody>();
        if (trunkRb != null)
        {
            trunkRb.AddForce(Vector3.back * 500f);
        }

    }

}
