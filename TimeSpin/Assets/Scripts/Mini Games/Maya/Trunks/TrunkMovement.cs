using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrunkMovement : NetworkBehaviour
{
    private float _speed = 3f; // Velocidad de movimiento del tronco
    private float _rotationSpeed = 100f; // Velocidad de rotaci�n del tronco

    private Rigidbody _rb;
    public MeshRenderer mesh;

    public int trunkNetworkId;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        mesh = GetComponent<MeshRenderer>();
        // S�lo se a�adir�n los troncos al pool manualmente en los clientes, una vez spawneen
        if (Application.platform == RuntimePlatform.LinuxServer) return;
        // En el cliente se almacena el identificador en red del objeto
        trunkNetworkId = (int)NetworkObjectId;
        TrunkPool.instance.AddTrunkToPool(gameObject);
        mesh.enabled = false; // Despu�s de a�adirse al pool, se desactiva el modelo
    }

    private void Update()
    {
        if (!RaceManager.instance.runningGame && Application.platform == RuntimePlatform.LinuxServer)
        {
            _rb.velocity = Vector3.zero;
            return;
        }

        // S�lo se aplica el movimiento del tronco en el servidor
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            // Mueve el tronco hacia atr�s continuamente
            transform.position += Vector3.back * _speed * Time.deltaTime;

            // Rota el tronco sobre su eje X
            transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
        }

        // Verifica si el tronco ha ca�do fuera del �rea, tanto en el cliente como en el servidor, para poder ocultarlo en ambos
        if (transform.position.y < 0)
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        // Devuelve el tronco al pool
        TrunkPool.instance.ReturnTrunkToPool(gameObject);
    }
}
