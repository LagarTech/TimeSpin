using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class SelectionTable : MonoBehaviour
{
    public static SelectionTable Instance;

    private Transform _playerTransform;
    [SerializeField] private int MAX_DISTANCE = 5;

    // Control del movimiento de c�mara hacia la mesa
    private Camera _mainCamera;
    private float _zoomSpeed = 1.75f;
    private float _rotationSpeed = 1.25f;
    [SerializeField] private bool _isZooming = false;
    [SerializeField] private bool _isReturning = false;

    [SerializeField] private Vector3 _targetPosition; // Posici�n destino
    [SerializeField] private Vector3 _targetRotation; // Rotaci�n destino
    private Quaternion _targetQuaternionRotation; // Rotaci�n destino en Quaternions, para poder interpolar

    [SerializeField] private Vector3 _originPosition; // Posici�n de origen
    [SerializeField] private Quaternion _originQuaternionRotation; // Rotaci�n de origen

    // Canvas de elecci�n de minijuego
    [SerializeField] private CanvasGroup _gameSelectionPanel;

    public bool runningGame = true;

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
        // Interacci�n con el jugador al estar cerca
        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            if (_playerTransform != null)
            {
                if (Vector3.Distance(transform.position, _playerTransform.position) < MAX_DISTANCE && !_isReturning && !_isZooming)
                {
                    StartCoroutine(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().InteractPlayer());
                    UI_Controller.instance.TextoConsejo.SetActive(false);
                    _isZooming = true;
                    runningGame = false;
                }
            }
        }

        if(_isZooming)
        {
            // Mueve la c�mara hacia la posici�n objetivo
            _mainCamera.transform.position = Vector3.Lerp(
                _mainCamera.transform.position,
                _targetPosition,
                _zoomSpeed * Time.deltaTime
            );

            // Rota la c�mara hacia la rotaci�n objetivo
            _mainCamera.transform.rotation = Quaternion.Lerp(
                _mainCamera.transform.rotation,
                _targetQuaternionRotation,
                _rotationSpeed * Time.deltaTime
            );

            // Det�n el acercamiento cuando est� lo suficientemente cerca en posici�n y rotaci�n
            if (Vector3.Distance(_mainCamera.transform.position, _targetPosition) < 0.1f &&
                Quaternion.Angle(_mainCamera.transform.rotation, _targetQuaternionRotation) < 1f)
            {
                // Una vez se llega a la posici�n de destino, se muestra la pantalla de elecci�n de minijuegos
                _isZooming = false;
                _gameSelectionPanel.gameObject.SetActive(true);
                UI_Controller.instance.AbandonarBoton.SetActive(false);
                StartCoroutine(LoadingScreenManager.instance.FadeCanvasGroup(_gameSelectionPanel, 1f, false));
            }
        }

        if (_isReturning)
        {
            // Mueve la c�mara hacia la posici�n objetivo
            _mainCamera.transform.position = Vector3.Lerp(
                _mainCamera.transform.position,
                _originPosition,
                _zoomSpeed * Time.deltaTime
            );

            // Rota la c�mara hacia la rotaci�n objetivo
            _mainCamera.transform.rotation = Quaternion.Lerp(
                _mainCamera.transform.rotation,
                _originQuaternionRotation,
                _rotationSpeed * Time.deltaTime
            );

            // Det�n el acercamiento cuando est� lo suficientemente cerca en posici�n y rotaci�n
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

    // Funci�n para ocultar la pantalla de elecci�n de minijuegos
    public void HideGameSelectionPanel()
    {
        StartCoroutine(LoadingScreenManager.instance.FadeCanvasGroup(_gameSelectionPanel, 0f, false));
        _isReturning = true; // La c�mara vuelve a su posici�n de origen
        runningGame = true;
        UI_Controller.instance.AbandonarBoton.SetActive(true);
    }

}
