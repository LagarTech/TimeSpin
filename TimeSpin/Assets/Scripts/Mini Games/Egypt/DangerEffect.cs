using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;

public class DangerEffect : MonoBehaviour
{
    public Volume postProcessingVolume; // Asigna el volumen de Post-Processing
    public float warningDistance = 2f;        // Distancia a partir de la cual empieza el efecto
    public float maxTintStrength = 0.5f;      // Máxima intensidad del tinte rojo (reducida)

    private ColorAdjustments colorAdjustments;
    private float currentIntensity = 0f;      // Intensidad actual del efecto
    private float smoothVelocity = 0f;        // Variable auxiliar para SmoothDamp

    void Start()
    {
        // Obtén el efecto Color Adjustments del perfil
        if (postProcessingVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            colorAdjustments.colorFilter.overrideState = true; // Habilita el control manual
            colorAdjustments.colorFilter.value = Color.white; // Comienza sin efecto
        }
    }

    void Update()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Momia");

        if (enemies.Length == 0)
        {
            ResetEffect(); // No hay enemigos, desactiva el tinte
            return;
        }

        // Encuentra la distancia más cercana a un enemigo
        float closestDistance = Mathf.Infinity;
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(player.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
            }
        }

        // Aplica el tinte rojo si el enemigo más cercano está dentro del rango
        if (closestDistance < warningDistance)
        {
            float targetIntensity = Mathf.Lerp(0, maxTintStrength, 1 - (closestDistance / warningDistance));
            ApplyColorFilter(targetIntensity);
        }
        else
        {
            ResetEffect(); // Si está fuera de rango, desactiva el tinte
        }
    }

    private void ApplyColorFilter(float targetIntensity)
    {
        // Ajusta la intensidad gradualmente
        currentIntensity = Mathf.SmoothDamp(currentIntensity, targetIntensity, ref smoothVelocity, 0.3f);
        colorAdjustments.colorFilter.value = Color.Lerp(Color.white, Color.red, currentIntensity);
    }

    private void ResetEffect()
    {
        // Reduce el efecto gradualmente hacia blanco
        currentIntensity = Mathf.SmoothDamp(currentIntensity, 0f, ref smoothVelocity, 0.3f);
        colorAdjustments.colorFilter.value = Color.Lerp(Color.white, Color.red, currentIntensity);
    }
}

