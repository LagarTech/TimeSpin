using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AchievementManager
{
    public static void UnlockAchievement(string achievementKey)
    {
        if (!IsAchievementUnlocked(achievementKey))
        {
            PlayerPrefs.SetInt(achievementKey, 1); // Logro desbloqueado
            Debug.Log("Logro desbloqueado: " + achievementKey);
        }
    }

    public static bool IsAchievementUnlocked(string achievementKey)
    {
        return PlayerPrefs.GetInt(achievementKey, 0) == 1;
    }

    /*
    public static void ResetAchievements()
    {
        PlayerPrefs.DeleteAll(); //Borra todos los logros
    }
    */
}

