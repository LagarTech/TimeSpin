using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinigameController : MonoBehaviour
{
    public static MinigameController Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] public string minigameName;
    public int currentScore = 0; // Esta puntuación se actualizará durante el juego.
    public TMP_Text recordText;


    public void ShowRecord()
    {
        int highScore = RecordManager.Instance.LoadRecord(minigameName);
        recordText.text = $"Récord: {highScore}";
    }
}

