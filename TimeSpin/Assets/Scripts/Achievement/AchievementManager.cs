using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class AchievementManager
{
    public static void UnlockAchievement(string achievementKey)
    {
        if (GameSceneManager.instance.practiceStarted)
        {
            return;
        }

        Debug.Log("Clave" + achievementKey);
        // Desbloquea el logro en PlayerPrefs
        if (!IsAchievementUnlocked(achievementKey))
        {
            PlayerPrefs.SetInt(achievementKey, 1);
            PlayerPrefs.Save();
            // Derivar el título automáticamente del achievementKey (ejemplo: "Medieval_CaballerosYTorneos" -> "Medieval Caballeros Y Torneos")
            string achievementTitle = achievementKey.Substring(achievementKey.IndexOf('_') + 1).Replace("_", " ");

            // Mostrar la notificación usando el sistema de notificaciones
            AchievementNotificationSystem.ShowNotification(achievementTitle);
        }

    }

    public static bool IsAchievementUnlocked(string achievementKey)
    {
        return PlayerPrefs.GetInt(achievementKey, 0) == 1;
    }
}


