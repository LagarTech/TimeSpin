using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager instance;

    public bool runningGame = false;
    private List<GameObject> _players;
    private List<GameObject> _playersFinished = new List<GameObject>();

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

    private void Start()
    {
        _players = GameObject.FindGameObjectsWithTag("Player").ToList();
    }

    private void PlayerReachedFinishLine(GameObject player)
    {
        // Se actualiza la lista de jugadores, por si acaso alguno se ha desconectado
        _players = GameObject.FindGameObjectsWithTag("Player").ToList();
        if (!_playersFinished.Contains(player))
        {
            _playersFinished.Add(player); // Añade al jugador que ha llegado a la meta
            CheckEndOfRace(); // Verifica si la carrera debe terminar
        }
    }

    private void CheckEndOfRace()
    {
        // Se calcula cuántos jugadores quedan por llegar a la meta
        int remainingPlayers = _players.Count - _playersFinished.Count;

        if (remainingPlayers <= 1)
        {
            EndRace();
        }
    }

    private void EndRace()
    {
        Debug.Log("La carrera ha terminado");
        runningGame = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            // Se indica que el jugador ha pasado la meta
            PlayerReachedFinishLine(collision.gameObject);
        }
    }
}
