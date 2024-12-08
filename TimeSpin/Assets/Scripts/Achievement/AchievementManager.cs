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

        // Derivar el título automáticamente del achievementKey (ejemplo: "Medieval_CaballerosYTorneos" -> "Medieval Caballeros Y Torneos")
        string achievementTitle = achievementKey.Replace("_", " ");

        // Mostrar la notificación usando el sistema de notificaciones
        AchievementNotificationSystem.ShowNotification(achievementTitle);
    }

    public static bool IsAchievementUnlocked(string achievementKey)
    {
        return PlayerPrefs.GetInt(achievementKey, 0) == 1;
    }
}


