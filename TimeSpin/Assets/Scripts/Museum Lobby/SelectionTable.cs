using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class SelectionTable : MonoBehaviour
{
    public static SelectionTable Instance;

    private Transform _playerTransform;
    private const int MAX_DISTANCE = 2;

    // Control del movimiento de cámara hacia la mesa
    private Camera _mainCamera;
    private float _zoomSpeed = 1.25f;
    private float _rotationSpeed = 0.7f;
    [SerializeField] private bool _isZooming = false;
    [SerializeField] private bool _isReturning = false;

    [SerializeField] private Vector3 _targetPosition; // Posición destino
    [SerializeField] private Vector3 _targetRotation; // Rotación destino
    private Quaternion _targetQuaternionRotation; // Rotación destino en Quaternions, para poder interpolar

    [SerializeField] private Vector3 _originPosition; // Posición de origen
    [SerializeField] private Quaternion _originQuaternionRotation; // Rotación de origen

    // Canvas de elección de minijuego
    [SerializeField] private CanvasGroup _gameSelectionPanel;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        _originPosition = _mainCamera.transform.position;
        _originQuaternionRotation = _mainCamera.transform.rotation;
        _targetQuaternionRotation = Quaternion.Euler(_targetRotation);
    }

    private void Update()
    {
        if(_isZooming)
        {
            // Mueve la cámara hacia la posición objetivo
            _mainCamera.transform.position = Vector3.Lerp(
                _mainCamera.transform.position,
                _targetPosition,
                _zoomSpeed * Time.deltaTime
            );

            // Rota la cámara hacia la rotación objetivo
            _mainCamera.transform.rotation = Quaternion.Lerp(
                _mainCamera.transform.rotation,
                _targetQuaternionRotation,
                _rotationSpeed * Time.deltaTime
            );

            // Detén el acercamiento cuando esté lo suficientemente cerca en posición y rotación
            if (Vector3.Distance(_mainCamera.transform.position, _targetPosition) < 0.1f &&
                Quaternion.Angle(_mainCamera.transform.rotation, _targetQuaternionRotation) < 1f)
            {
                // Una vez se llega a la posición de destino, se muestra la pantalla de elección de minijuegos
                _isZooming = false;
                _gameSelectionPanel.gameObject.SetActive(true);
                StartCoroutine(LoadingScreenManager.instance.FadeCanvasGroup(_gameSelectionPanel, 1f, false));
            }
        }

        if (_isReturning)
        {
            // Mueve la cámara hacia la posición objetivo
            _mainCamera.transform.position = Vector3.Lerp(
                _mainCamera.transform.position,
                _originPosition,
                _zoomSpeed * Time.deltaTime
            );

            // Rota la cámara hacia la rotación objetivo
            _mainCamera.transform.rotation = Quaternion.Lerp(
                _mainCamera.transform.rotation,
                _originQuaternionRotation,
                _rotationSpeed * Time.deltaTime
            );

            // Detén el acercamiento cuando esté lo suficientemente cerca en posición y rotación
            if (Vector3.Distance(_mainCamera.transform.position, _originPosition) < 0.5f &&
                Quaternion.Angle(_mainCamera.transform.rotation, _originQuaternionRotation) < 2f)
            {
                _isReturning = false;
            }
        }
    }

    // Una vez spawnea el jugador, se toma una referencia
    public void AddPlayerReference (Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    private void OnMouseDown()
    {
        if (_playerTransform != null)
        {
            if (Vector3.Distance(transform.position, _playerTransform.position) < MAX_DISTANCE && !_isReturning && !_isZooming)
            {
                _isZooming = true;
            }
        }
    }

    // Función para ocultar la pantalla de elección de minijuegos
    public void HideGameSelectionPanel()
    {
        StartCoroutine(LoadingScreenManager.instance.FadeCanvasGroup(_gameSelectionPanel, 0f, false));
        _isReturning = true; // La cámara vuelve a su posición de origen
    }

}
