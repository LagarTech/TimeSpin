using System.Collections;
using UnityEngine;
using TMPro;

public class AchievementNotificationSystem : MonoBehaviour
{
    public static AchievementNotificationSystem Instance;

    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private GameObject exclamacion;

    private void Awake()
    {
        // Singleton para asegurar que solo haya una instancia
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Hace que el objeto persista entre escenas

        }
        else
        {
            Destroy(gameObject);
        }


    }

    // Método estático para mostrar una notificación
    public static void ShowNotification(string achievementTitle)
    {
        if (Instance != null)
        {
            Instance.ShowAchievementNotification(achievementTitle);
        }
        else
        {
            Debug.LogError("No se encontró una instancia de AchievementNotificationSystem.");
        }
    }

    private void ShowAchievementNotification(string achievementTitle)
    {
        if (notificationPanel != null && notificationText != null)
        {
            notificationPanel.SetActive(true);
            exclamacion.SetActive(true);
            notificationText.text = "¡Logro Desbloqueado!\n" + achievementTitle;
            

            StartCoroutine(HideNotificationAfterDelay(3f));
        }
        else
        {
            Debug.LogError("Faltan referencias al panel o texto de notificación.");
        }
    }

    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        notificationPanel.SetActive(false);
        exclamacion?.SetActive(false);
    }
}

