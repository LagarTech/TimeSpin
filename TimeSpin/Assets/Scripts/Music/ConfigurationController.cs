using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ConfigurationController : MonoBehaviour
{
    public AudioMixer mezclador;

    [SerializeField] private Slider sliderBrillo;
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSonido;


    private void Start()
    {
        sliderBrillo.value = GameSceneManager.instance.Brigthness;
        sliderMusica.value = GameSceneManager.instance.MusicVolume;
        sliderSonido.value = GameSceneManager.instance.EffectsVolume;
    }

    public void CambiarVolumenMusica(float volumenMusica)
    {
        mezclador.SetFloat("volumenMusica", volumenMusica);
        GameSceneManager.instance.MusicVolume = volumenMusica;
    }

    public void CambiarVolumenSonido(float volumenSonido)
    {
        mezclador.SetFloat("volumenSonido", volumenSonido);
        GameSceneManager.instance.EffectsVolume = volumenSonido;
    }

    public void CambiarBrillo(float intensidad)
    {
        GameSceneManager.instance.Brigthness = intensidad;
    }
}
