using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private bool _followingPlayer = false;

    public Transform player; // Referencia al transform del jugador
    public Vector3 offset; // Distancia entre la cámara y el jugador
    public float smoothSpeed = 0.5f; // Velocidad de suavizado

    private void LateUpdate()
    {
        if(!_followingPlayer) return;

        // Posición deseada: la posición del jugador más el offset
        Vector3 desiredPosition = player.position + offset;

        // Posición suavizada: la cámara se mueve hacia la posición deseada con un suavizado
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Asigna la nueva posición suavizada a la cámara
        transform.position = smoothedPosition;

    }

    public void StartFollowingPlayer(GameObject playerToFollow)
    {
        _followingPlayer = true;
        player = playerToFollow.transform;
        offset = transform.position - player.position;
    }
}
