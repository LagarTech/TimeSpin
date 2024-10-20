using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrunkSpawner : MonoBehaviour
{
    // Extremos para la generación aleatoria de los troncos
    private float _minX = -3;
    private float _maxX = 3;

    private float _spawnInterval = 1.5f; // Tiempo entre spawns
    private float _spawnTimer = 0f;   // Temporizador para controlar el spawn

    private void Update()
    {
        if (!RaceManager.instance.runningGame) return;
        // Sólo se gestiona el spawn de los troncos desde el servidor, que avisará a los clientes cuando se deba tomar uno
        if (Application.platform != RuntimePlatform.LinuxServer) return;
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
        // Genera una posición aleatoria en el eje X
        float randomX = Random.Range(_minX, _maxX);

        // Define la posición de generación (en Z = 58)
        Vector3 spawnPosition = new Vector3(randomX, 2f, 58f);  // Ajusta según el escenario
        Quaternion spawnRotation = Quaternion.Euler(0, 0, 90); // Rotación en Z

        // Instancia el tronco en el servidor
        GameObject newTrunk = TrunkPool.instance.GetTrunkFromPool();

        if (newTrunk != null)
        {
            // Ubica el tronco en la posición inicial
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
