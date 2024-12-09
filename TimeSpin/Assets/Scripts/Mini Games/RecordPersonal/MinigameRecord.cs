using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MinigameRecord
{
    public string minigameName;
    public int highScore;

    public MinigameRecord(string name, int score)
    {
        minigameName = name;
        highScore = score;
    }
}

