using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
{
    public static RaceManager instance;

    public bool runningGame = false;
    private float _timer = 0f;
    [SerializeField]
    private GameObject _optionsPanel;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _optionsPanel.SetActive(true);
            runningGame = false;
        }
        if (runningGame)
        {
            // Se contabiliza el tiempo que se tarda en terminar la carrera
            _timer += Time.deltaTime;
        }
    }

    // Función para terminar el minijuego
    private void EndRace()
    {
        if (!runningGame) return;
        runningGame = false;
        // Se inicia la transición
        GameSceneManager.instance.GameOverMaya(_timer);
    }

    // Se comprueba si el jugador ha pasado la meta
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            EndRace();
        }
    }
    public void Options()
    {
        _optionsPanel.SetActive(false);
        runningGame = true;
    }
}
