using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private Vector3 _movementDirection = Vector3.zero;
    private float _speed = 1f;

    // Update is called once per frame
    void Update()
    {
        // El servidor procesa el movimiento del jugador, y este se sincroniza en los clientes debido al NetworkTransform
        if (NetworkManager.Singleton.IsServer)
        {
            transform.position += _movementDirection * _speed * Time.deltaTime;
        }
        // Los clientes capturan los inputs y los envían al servidor, que actualizará la dirección de movimiento en función de ello
        else
        {
            // Sólo se capturan los inputs del propietario del jugador
            if (!IsOwner) return;

            _movementDirection = Vector3.zero;

            if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
            if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
            if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
            if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;

            ChangePlayerDirectionServerRpc(_movementDirection, (int)OwnerClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerDirectionServerRpc(Vector3 direction, int playerId)
    {
        // Se comprueba primero si el jugador que se está comprobando es del que se ha recibido el input
        if (playerId != (int)OwnerClientId) return;
        // Se modifica la dirección de movimiento del jugador en el servidor
        _movementDirection = direction;
    }
}
