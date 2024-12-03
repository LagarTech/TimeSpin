using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfusedDuck : MonoBehaviour
{
    // Velocidad de rotación alrededor del eje Y
    [SerializeField] private float rotationSpeed = 90f;
    public float accumulatedAngle = 0f;

    private Vector3 _playerPosition;
    private Vector3 _offsetPosition;

    private void Start()
    {
        _offsetPosition = transform.localPosition;
        transform.parent = null;
        _playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        transform.position = _playerPosition + _offsetPosition;
    }


    private void Update()
    {
        // Rotar el objeto alrededor del eje Y
        accumulatedAngle = (rotationSpeed * Time.deltaTime + accumulatedAngle) % 360;
        transform.rotation = Quaternion.Euler(0f, accumulatedAngle, 0f);

        if(GameObject.FindGameObjectWithTag("Player") == null)
        {
            gameObject.SetActive(false);
            return;
        }

        _playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        transform.position = _playerPosition + _offsetPosition;

    }


}
