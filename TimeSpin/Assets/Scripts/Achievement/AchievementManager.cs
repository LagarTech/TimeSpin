using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AchievementManager
{
    public static void UnlockAchievement(string achievementKey)
    {
        Debug.Log("Clave" + achievementKey);
        // Desbloquea el logro en PlayerPrefs
        PlayerPrefs.SetInt(achievementKey, 1);
        PlayerPrefs.Save();

    }

    public static bool IsAchievementUnlocked(string achievementKey)
    {
        return PlayerPrefs.GetInt(achievementKey, 0) == 1;
    }
}


