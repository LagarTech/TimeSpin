using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioClip clipAudio;
    private AudioSource reproductor;
    public bool ponerAlEmpezar;
    public bool debeRepetirse;

    private void Awake()
    {
        reproductor = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if(GameSceneManager.instance.initiatedGame)
        {
            ponerAlEmpezar = true;
        }

        if(ponerAlEmpezar)
        {
            PonerClip();
        }
    }

    public void PonerClip()
    {
        MusicManager.PonerMusica(clipAudio, reproductor, debeRepetirse);
    }
}
