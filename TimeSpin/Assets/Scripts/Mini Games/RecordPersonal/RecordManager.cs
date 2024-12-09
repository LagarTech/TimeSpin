using UnityEngine;

public class RecordManager : MonoBehaviour
{
    public static RecordManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveRecord(string minigameName, int score)
    {
        int currentHighScore = PlayerPrefs.GetInt(minigameName + "_HighScore", 0);
        Debug.Log($"Puntuación actual: {score}, Récord actual: {currentHighScore}");

        if (score > currentHighScore)
        {
            PlayerPrefs.SetInt(minigameName + "_HighScore", score);
            PlayerPrefs.Save();
            Debug.Log($"Nuevo récord guardado para {minigameName}: {score}");
        }
        else
        {
            Debug.Log("No se ha superado el récord existente.");
        }
    }

    public int LoadRecord(string minigameName)
    {
        int highScore = PlayerPrefs.GetInt(minigameName + "_HighScore", 0);
        Debug.Log($"Cargando récord para {minigameName}: {highScore}");
        return highScore;
    }
}
