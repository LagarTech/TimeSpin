using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instancia;
    public bool iniciado = false;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia=this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        iniciado = true;
    }
    public static void PonerMusica(AudioClip cancion, AudioSource reproductor, bool debeRepetirse)
    {
        reproductor.clip = cancion;
        reproductor.loop = debeRepetirse;
        reproductor.Play();
    }

    public static void QuitarMusica(AudioSource reproductor)
    {
        reproductor.Stop();
    }
}
