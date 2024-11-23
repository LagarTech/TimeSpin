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
        // Se obtiene un consejo de los 4 posibles
        int randomText = Random.Range(0, NUM_TEXTS_PER_SCENE - 1);
        // Se calcula el ID de la lista
        int id = sceneID * NUM_TEXTS_PER_SCENE + randomText;
        return advices[id];
    }
}
