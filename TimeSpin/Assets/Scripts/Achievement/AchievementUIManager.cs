using UnityEngine;
using TMPro;

public class AchievementUIManager : MonoBehaviour
{
    [SerializeField] private GameObject achievementsMenu; // Panel principal de AchievementsMenu
    [SerializeField] private GameObject fullScreenDetailContainer; // Contenedor de texto a pantalla completa
    [SerializeField] private GameObject fullScreenDetailText; // Texto a pantalla completa para descripci�n
    [SerializeField] private GameObject closeButton; // Bot�n de cerrar para fullscreen
    [SerializeField] private GameObject panel;

    [SerializeField] private GameObject notificationPanel; // Panel para la notificaci�n
    [SerializeField] private TMP_Text notificationText;    // Texto de la notificaci�n

    private GameObject currentAchievementPrefab; // Prefab actual del logro

    // Registrar el prefab de un logro
    public void RegisterAchievementPrefab(GameObject prefab)
    {
        currentAchievementPrefab = prefab;
    }

    // Mostrar la descripci�n en pantalla completa
    public void ShowDescription(string description)
    {
        Debug.Log("Mostrando descripci�n: " + description);

        fullScreenDetailText.SetActive(true);

        // Asignar el texto de la descripci�n
        fullScreenDetailText.GetComponent<TMP_Text>().text = description;

        // Ocultar el men� principal de logros
        achievementsMenu.SetActive(false);

        // Mostrar solo el contenedor de detalles
        fullScreenDetailContainer.SetActive(true);
        panel.SetActive(true);
        closeButton.SetActive(true);
    }

    // Ocultar el contenedor de detalles y volver al men� principal
    public void HideFullScreenDetails()
    {
        if (fullScreenDetailContainer != null && achievementsMenu != null)
        {
            // Ocultar el contenedor de texto completo
            fullScreenDetailContainer.SetActive(false);
            panel.SetActive(false);
            closeButton.SetActive(false);

            // Volver a mostrar el men� principal
            achievementsMenu.SetActive(true);
            SelectionTable.Instance.runningGame = true;
        }
        else
        {
            Debug.LogError("No se han asignado las referencias necesarias (AchievementsMenu o FullScreenDetailContainer) en el Inspector.");
        }
    }

    // Funci�n para mostrar la notificaci�n
    public void ShowAchievementNotification(string achievementTitle)
    {
        if (notificationPanel != null && notificationText != null)
        {
            notificationText.text = "�Logro Desbloqueado!\n" + achievementTitle;
            notificationPanel.SetActive(true);

            // Ocultar la notificaci�n despu�s de unos segundos
            StartCoroutine(HideNotificationAfterDelay(3f));
        }
        else
        {
            Debug.LogError("Faltan referencias al panel o texto de notificaci�n en el Inspector.");
        }
    }

    // Corrutina para ocultar la notificaci�n despu�s de un tiempo
    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        notificationPanel.SetActive(false);
    }
}



