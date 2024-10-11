using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrunkSpawner : MonoBehaviour
{
    // Prefab del tronco, para generarlo correctamente
    [SerializeField] private GameObject _trunkPrefab;
    // Extremos para la generación aleatoria de los troncos
    private float _minX = -3;
    private float _maxX = 3;

    private float _spawnInterval = 3f; // Tiempo entre spawns
    private float _spawnTimer = 0f;   // Temporizador para controlar el spawn

    private void Update()
    {
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

        // Instancia el tronco
        GameObject newTrunk = Instantiate(_trunkPrefab, spawnPosition, spawnRotation);

        // Aplica una fuerza para que ruede hacia el inicio (en Z = 0)
        Rigidbody trunkRb = newTrunk.GetComponent<Rigidbody>();
        if (trunkRb != null)
        {
            trunkRb.AddForce(Vector3.back * 500f); // Ajusta la fuerza según sea necesario
        }
    }

}
