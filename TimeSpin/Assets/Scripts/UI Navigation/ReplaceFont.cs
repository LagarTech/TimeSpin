using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReplaceFont : MonoBehaviour
{
    public TMP_FontAsset newFont; // Arrastra aquí la nueva fuente

    void Update()
    {
        // Encuentra todos los objetos en la escena con Text
        TMP_Text[] texts = FindObjectsOfType<TMP_Text>();

        foreach (TMP_Text text in texts)
        {
            // Cambia la fuente de cada uno
            text.font = newFont;
        }
    }
}
