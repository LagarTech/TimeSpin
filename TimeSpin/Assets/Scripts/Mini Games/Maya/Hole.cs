using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Hole : MonoBehaviour
{
    [SerializeField] private Vector3 _respawnOffset; // Desplazamiento para el respawn
    private float _respawnDelay = 1.5f; // Tiempo de espera antes de reaparecer
    [SerializeField]
    private AudioSource _reproductor;
    [SerializeField]
    private AudioClip _clipAudio;
    [SerializeField]
    private AudioMixer mezclador;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Aseg�rate de que el jugador tiene la etiqueta "Player"
        {
            StartCoroutine(RespawnPlayer(other.transform));
            MusicManager.PonerMusica(_clipAudio, _reproductor, false);

            // Ca�das en el sistema de logros
            AchievementsManager.instance.RegisterFall();
        }
    }


    private IEnumerator RespawnPlayer(Transform player)
    {
        // Desactiva la colisi�n entre el jugador y el terreno
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Terrain"), true);

        // Simula la ca�da
        yield return new WaitForSeconds(_respawnDelay / 3);

        // Desactiva al jugador durante un breve periodo de tiempo
        player.gameObject.SetActive(false);

        // Simula el tiempo de reaparici�n
        yield return new WaitForSeconds(_respawnDelay * 2 / 3);

        // Reposiciona al jugador un poco atr�s del agujero
        Vector3 respawnPosition = transform.position + _respawnOffset;
        player.position = respawnPosition;

        // Reactiva al jugador
        player.gameObject.SetActive(true);

        // Reactiva la colisi�n entre el jugador y el terreno
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Terrain"), false);

    }
}
