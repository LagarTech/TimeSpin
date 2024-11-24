using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenTexts : MonoBehaviour
{
    public static LoadingScreenTexts Instance;

    public const int NUM_SCENES = 6;
    public const int NUM_TEXTS_PER_SCENE = 4;
    public List<string> advices = new List<string>(NUM_SCENES * NUM_TEXTS_PER_SCENE);

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

        DontDestroyOnLoad(gameObject);
    }

    public string GetAdviceText(int sceneID)
    {
        // Verificar si sceneID est� dentro del rango permitido
        if (sceneID < 0 || sceneID >= NUM_SCENES)
        {
            Debug.LogError($"sceneID ({sceneID}) est� fuera del rango permitido.");
            return "Error: escena inv�lida";
        }

        // Obtener un �ndice aleatorio
        int randomText = Random.Range(0, NUM_TEXTS_PER_SCENE);
        int id = sceneID * NUM_TEXTS_PER_SCENE + randomText;

        // Verificar si id est� dentro de los l�mites de la lista
        if (id < 0 || id >= advices.Count)
        {
            Debug.LogError($"�ndice fuera de rango: id={id}, advices.Count={advices.Count}");
            return "Error: �ndice fuera de rango";
        }

        // Devolver el texto correspondiente
        return advices[id];
    }

}
