using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    private MeshRenderer _meshRenderer; // Componente de la plataforma
    [SerializeField] private List<Platform> _neighbourPlatforms; // Lista de plataformas vecinas

    public int platformCount = 0;
    private bool _isHighlighted = false;

    public int idPlatform;
    public bool isUp;

    public float playerOnPlatformTimer = 0f;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.enabled = false; // Se desactiva el componente por defecto
    }

    private void Update()
    {
        // Si hay jugadores en la plataforma y no est� iluminada
        if (platformCount > 0 && !_isHighlighted)
        {
            HighlightPlatform(); // Ilumina la plataforma
            _isHighlighted = true; // Marcar la plataforma como iluminada

            playerOnPlatformTimer += Time.deltaTime;

            // Logro: Automatizaci�nDelHogar
            if (playerOnPlatformTimer >= 5f)
            {
                AchievementManager.UnlockAchievement("Future_Automatizaci�nDelHogar");
            }
        }

        // Si no hay jugadores y la plataforma est� iluminada
        else if (platformCount <= 0 && _isHighlighted)
        {
            playerOnPlatformTimer = 0f; // Reiniciar el temporizador si el jugador se baja

            HidePlatform(); // Oculta la plataforma
            _isHighlighted = false; // Marcar la plataforma como no iluminada
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        // Si el jugador entra en la casilla
        if (collision.gameObject.tag == "Player")
        {
            // Se activan las plataformas vecinas
            OnPlatformEnter();

            // Logro: CochesVoladores
            AchievementManager.UnlockAchievement("Future_CochesVoladores");
        }
    }

    public void OnPlatformEnter()
    {
        // Se aumenta el contador de la plataforma
        platformCount++;
        // Se aumenta el contador de las vecinas
        foreach (Platform platform in _neighbourPlatforms)
        {
            platform.platformCount++;
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        // Si el jugador entra en la casilla
        if (collision.gameObject.tag == "Player")
        {
            // Se desactivan las plataformas vecinas
            OnPlatformExit();
        }
    }

    public void OnPlatformExit()
    {
        // Se disminuye el contador de la plataforma
        platformCount--;
        // Se disminuye el contador de las vecinas
        foreach (Platform platform in _neighbourPlatforms)
        {
            platform.platformCount--;
        }
    }

    public void HighlightPlatform()
    {
        _meshRenderer.enabled = true;
    }

    public void HidePlatform()
    {
        _meshRenderer.enabled = false;
    }

    public void FallPlatform()
    {
        // Se disminuye el contador de la plataforma
        platformCount = 0;
        // Se disminuye el contador de las vecinas
        foreach (Platform platform in _neighbourPlatforms)
        {
            if(platform.platformCount > 0)
            {
                platformCount--;
            }
        }
    }

}
