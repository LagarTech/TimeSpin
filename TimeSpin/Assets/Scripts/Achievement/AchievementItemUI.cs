using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText; // Texto del nombre
    [SerializeField] private TMP_Text statusText; // Texto del estado
    [SerializeField] private GameObject statusIcon; // Icono del estado
    [SerializeField] private Slider progressSlider; // Barra de progreso
    [SerializeField] private TMP_Text detailText; // Texto para Condition/Description
    [SerializeField] private GameObject conditionButton; // Botón de condición
    [SerializeField] private GameObject descriptionButton; // Botón de descripción
    [SerializeField] private GameObject closeButton; // Botón para cerrar detalles

    private bool unlockedAchievement; // Estado del logro
    private string detailToShow; // Texto a mostrar (Condition o Description)

    // Estructura para recibir datos del logro
    public struct AchievementData
    {
        public string Title;
        public string Description;
        public string Condition;
        public bool IsUnlocked;
    }

    public void SetAchievementData(AchievementData data)
    {
        unlockedAchievement = data.IsUnlocked;

        // Configurar título
        if (titleText != null)
            titleText.text = data.Title;

        // Configurar estado
        if (statusText != null)
        {
            statusText.text = data.IsUnlocked ? "1/1" : "0/1";
            statusText.color = data.IsUnlocked ? Color.green : Color.red;
        }

        // Configurar icono
        if (statusIcon != null)
        {
            statusIcon.GetComponent<Image>().color = data.IsUnlocked ? Color.green : Color.gray;
        }

        // Configurar progreso
        if (progressSlider != null)
        {
            progressSlider.value = data.IsUnlocked ? progressSlider.maxValue : 0;
        }

        // Configurar botones
        conditionButton.SetActive(!data.IsUnlocked);
        descriptionButton.SetActive(data.IsUnlocked);
        closeButton.SetActive(false); // Botón de cerrar oculto inicialmente

        // Asignar acciones a los botones
        conditionButton.GetComponent<Button>().onClick.RemoveAllListeners();
        conditionButton.GetComponent<Button>().onClick.AddListener(() => ShowDetails(data.Condition));

        descriptionButton.GetComponent<Button>().onClick.RemoveAllListeners();
        descriptionButton.GetComponent<Button>().onClick.AddListener(() => ShowDetails(data.Description));

        closeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        closeButton.GetComponent<Button>().onClick.AddListener(HideDetails);

        // Ocultar detalles iniciales
        if (detailText != null)
            detailText.gameObject.SetActive(false);
    }

    // Mostrar Condition o Description
    private void ShowDetails(string detail)
    {
        detailToShow = detail;
        Debug.Log(detailToShow);
        // Ocultar elementos principales
        if (titleText != null) titleText.gameObject.SetActive(false);
        if (statusText != null) statusText.gameObject.SetActive(false);
        if (statusIcon != null) statusIcon.SetActive(false);
        if (progressSlider != null) progressSlider.gameObject.SetActive(false);
        conditionButton.SetActive(false);
        descriptionButton.SetActive(false);

        // Mostrar detalles y botón de cerrar
        if (detailText != null)
        {
            detailText.text = detailToShow;
            detailText.gameObject.SetActive(true);
        }
        closeButton.SetActive(true);
    }

    // Ocultar detalles y restaurar elementos principales
    private void HideDetails()
    {
        // Mostrar elementos principales
        if (titleText != null) titleText.gameObject.SetActive(true);
        if (statusText != null) statusText.gameObject.SetActive(true);
        if (statusIcon != null) statusIcon.SetActive(true);
        if (progressSlider != null) progressSlider.gameObject.SetActive(true);

        // Restaurar botones según el estado del logro
        conditionButton.SetActive(!unlockedAchievement);
        descriptionButton.SetActive(unlockedAchievement);

        // Ocultar detalles y botón de cerrar
        if (detailText != null) detailText.gameObject.SetActive(false);
        closeButton.SetActive(false);
    }
}

