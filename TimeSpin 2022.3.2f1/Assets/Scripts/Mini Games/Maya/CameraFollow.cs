using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private bool _followingPlayer = false;

    public Transform player; // Referencia al transform del jugador
    public Vector3 offset; // Distancia entre la c�mara y el jugador
    public float smoothSpeed = 0.5f; // Velocidad de suavizado

    private void LateUpdate()
    {
        if(!_followingPlayer) return;

        // Posici�n deseada: la posici�n del jugador m�s el offset
        Vector3 desiredPosition = player.position + offset;

        // Posici�n suavizada: la c�mara se mueve hacia la posici�n deseada con un suavizado
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Asigna la nueva posici�n suavizada a la c�mara
        transform.position = smoothedPosition;

    }

    public void StartFollowingPlayer(GameObject playerToFollow)
    {
        _followingPlayer = true;
        player = playerToFollow.transform;
        offset = transform.position - player.position;
    }
}
