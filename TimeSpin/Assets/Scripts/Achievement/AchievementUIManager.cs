using UnityEngine;
using TMPro;
using System.Collections;


public class AchievementUIManager : MonoBehaviour
{
    [SerializeField] private GameObject achievementsMenu; // Panel principal de AchievementsMenu
    [SerializeField] private GameObject fullScreenDetailContainer; // Contenedor de texto a pantalla completa
    [SerializeField] private GameObject fullScreenDetailText; // Texto a pantalla completa para descripción
    [SerializeField] private GameObject closeButton; // Botón de cerrar para fullscreen
    [SerializeField] private GameObject panel;

    private GameObject currentAchievementPrefab; // Prefab actual del logro

    // Registrar el prefab de un logro
    public void RegisterAchievementPrefab(GameObject prefab)
    {
        currentAchievementPrefab = prefab;
    }

    // Mostrar la descripción en pantalla completa
    public void ShowDescription(string description)
    {
        Debug.Log("Mostrando descripción: " + description);

        fullScreenDetailText.SetActive(true);

        // Asignar el texto de la descripción
        fullScreenDetailText.GetComponent<TMP_Text>().text = description;

        // Ocultar el menú principal de logros
        achievementsMenu.SetActive(false);

        // Mostrar solo el contenedor de detalles
        fullScreenDetailContainer.SetActive(true);
        panel.SetActive(true);
        closeButton.SetActive(true);
    }

    // Ocultar el contenedor de detalles y volver al menú principal
    public void HideFullScreenDetails()
    {
        if (fullScreenDetailContainer != null && achievementsMenu != null)
        {
            // Ocultar el contenedor de texto completo
            fullScreenDetailContainer.SetActive(false);
            panel.SetActive(false);
            closeButton.SetActive(false);

            // Volver a mostrar el menú principal
            achievementsMenu.SetActive(true);
            SelectionTable.Instance.runningGame = true;
        }
        else
        {
            Debug.LogError("No se han asignado las referencias necesarias (AchievementsMenu o FullScreenDetailContainer) en el Inspector.");
        }
    }
}



