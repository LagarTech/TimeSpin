using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterModel : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 30f; // Velocidad de rotaci�n en grados por segundo
    private void Update()
    {
        RotateAroundY();
    }

    private void RotateAroundY()
    {
        // Calcula la rotaci�n incremental basada en el tiempo y la velocidad
        float rotationIncrement = rotationSpeed * Time.deltaTime;

        // Aplica la rotaci�n al objeto en el eje Y
        transform.Rotate(0f, rotationIncrement, 0f);
    }
}
