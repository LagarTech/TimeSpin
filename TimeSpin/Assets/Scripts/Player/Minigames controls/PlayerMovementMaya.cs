using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementMaya : MonoBehaviour
{
    private Vector3 _movementDirection = Vector3.zero;
    private Rigidbody _rb;

    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private bool _isGrounded = true;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true; // Para evitar que el jugador se voltee al colisionar
    }

    private void Update()
    {
        // Gestión de los controles
        _movementDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) _movementDirection.z = 1f;
        if (Input.GetKey(KeyCode.S)) _movementDirection.z = -1f;
        if (Input.GetKey(KeyCode.A)) _movementDirection.x = -1f;
        if (Input.GetKey(KeyCode.D)) _movementDirection.x = 1f;

        // Movimiento en los ejes X y Z
        transform.position += _movementDirection * _speed * Time.deltaTime;

        // Salto
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            Jump();
        }
    }

    private void Jump()
    {
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        _isGrounded = false; // Desactivamos el estado de "en el suelo" hasta que colisione con el suelo otra vez
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Detectamos cuando el jugador está de vuelta en el suelo
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
        }
    }
}
