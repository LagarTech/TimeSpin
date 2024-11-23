using UnityEngine;
using TMPro;

public class AchievementItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText; // Texto del nombre
    [SerializeField] private TMP_Text statusText; // Texto del estado
    [SerializeField] private GameObject statusIcon; // Icono del estado
    [SerializeField] private UnityEngine.UI.Slider progressSlider; // Barra de progreso
    [SerializeField] private GameObject conditionButton; // Botón de condición
    [SerializeField] private GameObject descriptionButton; // Botón de descripción
    [SerializeField] private TMP_Text detailText; // Texto de condición/descripción
    [SerializeField] private GameObject closeButton; // Botón para cerrar los detalles

    private bool unlockedAchievement; // Estado del logro
    private string detailToShow; // Almacena el texto de condición o descripción a mostrar
    private AchievementUIManager uiManager; // Referencia al script central


    // Estructura para recibir datos del logro
    public struct AchievementData
    {
        public string Title;
        public string Description;
        public string Condition;
        public bool IsUnlocked;
    }

    void Start()
    {
        // Encontrar el script central en la escena
        uiManager = FindObjectOfType<AchievementUIManager>();
        if (uiManager != null)
        {
            uiManager.RegisterAchievementPrefab(gameObject);
        }
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
            statusIcon.GetComponent<UnityEngine.UI.Image>().color = data.IsUnlocked ? Color.green : Color.gray;
        }

        // Configurar progreso
        if (progressSlider != null)
        {
            progressSlider.value = data.IsUnlocked ? progressSlider.maxValue : 0;
        }

        // Configurar botones
        conditionButton.SetActive(!data.IsUnlocked);
        descriptionButton.SetActive(data.IsUnlocked);

        // Asignar acciones a los botones
        conditionButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        conditionButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ShowCondition(data.Condition));

        descriptionButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        descriptionButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ShowDescription(data.Description));
    }

    private void ShowCondition(string condition)
    {
        detailToShow = condition;

        Debug.Log("Mostrando condición: " + condition);

        // Ocultar solo el contenido del prefab actual
        if (titleText != null) titleText.gameObject.SetActive(false);
        if (statusText != null) statusText.gameObject.SetActive(false);
        if (statusIcon != null) statusIcon.SetActive(false);
        if (progressSlider != null) progressSlider.gameObject.SetActive(false);
        conditionButton.SetActive(false);
        descriptionButton.SetActive(false);

        // Mostrar el texto de condición
        if (detailText != null)
        {
            detailText.text = detailToShow;
            detailText.gameObject.SetActive(true);
        }

        // Mostrar el botón de cerrar
        if (closeButton != null)
            closeButton.SetActive(true);
    }

    public void HideConditionDetails()
    {
        Debug.Log("Ocultando detalles y restaurando contenido original...");

        // Restaurar el contenido del prefab actual
        if (titleText != null) titleText.gameObject.SetActive(true);
        if (statusText != null) statusText.gameObject.SetActive(true);
        if (statusIcon != null) statusIcon.SetActive(true);
        if (progressSlider != null) progressSlider.gameObject.SetActive(true);

        conditionButton.SetActive(!unlockedAchievement);
        descriptionButton.SetActive(unlockedAchievement);

        // Ocultar el texto de condición y el botón de cerrar
        if (detailText != null)
            detailText.gameObject.SetActive(false);
        if (closeButton != null)
            closeButton.SetActive(false);

        SelectionTable.Instance.runningGame = true;
    }

    private void ShowDescription(string description)
    {
        Debug.Log("Descripción enviada al UI Manager: " + description);

        if (uiManager != null)
        {
            uiManager.ShowDescription(description);
        }
        else
        {
            Debug.LogError("No se encontró una referencia válida a AchievementUIManager.");
        }
    }

}





