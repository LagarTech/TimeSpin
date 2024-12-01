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
    public bool playerOnPlatform = false;

    private bool _isStepped;

    public bool isFallen;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.enabled = false; // Se desactiva el componente por defecto
    }

    private void Update()
    {
        if (!GravityManager.Instance.runningGame) return;
        // Se cuenta el tiempo que está el jugador en la plataforma
        if (playerOnPlatform)
        {
            playerOnPlatformTimer += Time.deltaTime;
        }
        else
        {
            playerOnPlatformTimer = 0f;
        }

        // Logro: AutomatizaciónDelHogar
        // A los 5 segundos se cae la plataforma
        if (playerOnPlatformTimer >= 3f)
        {
            StartCoroutine(PlatformManager.instance.ShakeAndFall(gameObject));
        }
        if (playerOnPlatformTimer >= 4f)
        {
            AchievementManager.UnlockAchievement("Future_AutomatizaciónDelHogar");
        }

        // Si hay jugadores en la plataforma y no está iluminada
        if (platformCount > 0 && !_isHighlighted)
        {
            HighlightPlatform(); // Ilumina la plataforma
            _isHighlighted = true; // Marcar la plataforma como iluminada

        }

        // Si no hay jugadores y la plataforma está iluminada
        else if (platformCount <= 0 && _isHighlighted)
        {
            HidePlatform(); // Oculta la plataforma
            _isHighlighted = false; // Marcar la plataforma como no iluminada
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        // Si el jugador entra en la casilla
        if (collision.gameObject.tag == "Player")
        {
            playerOnPlatform = true;
            // Se deja la plataforma iluminada
            _isStepped = true;
            _meshRenderer.enabled = true;
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
            playerOnPlatform = false;
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
        if (!_isStepped)
        {
            _meshRenderer.enabled = false;
        }
    }

    public void FallPlatform()
    {
        isFallen = true;
        // Se disminuye el contador de la plataforma
        platformCount = 0;
        // Se disminuye el contador de las vecinas
        foreach (Platform platform in _neighbourPlatforms)
        {
            if (platform.platformCount > 0)
            {
                platformCount--;
            }
        }
    }

}
