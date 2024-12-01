using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GravityWarning : MonoBehaviour
{
    public static GravityWarning Instance;

    public Image flashImage; // Asigna aquí la imagen blanca del Canvas
    public float totalDuration = 0.5f;   // Duración total del proceso en segundos
    public int flashCount = 2;         // Número de parpadeos

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

    public IEnumerator FlashEffect()
    {
        float halfFlashDuration = totalDuration / (flashCount * 2); // Duración de cada encendido o apagado

        for (int i = 0; i < flashCount; i++)
        {
            // Incrementa la opacidad (efecto de encendido)
            yield return LerpAlpha(0, 1, halfFlashDuration);

            // Reduce la opacidad (efecto de apagado)
            yield return LerpAlpha(1, 0, halfFlashDuration);
        }

    }

    private IEnumerator LerpAlpha(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color color = flashImage.color;

        while (elapsedTime < duration)
        {
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            flashImage.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegúrate de aplicar el valor final
        color.a = endAlpha;
        flashImage.color = color;
    }
}
