using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText; // Texto del nombre
    [SerializeField] private TMP_Text statusText; // Texto del estado
    [SerializeField] private GameObject statusIcon; // Icono del estado
    [SerializeField] private Slider progressSlider; // Barra de progreso
    [SerializeField] private GameObject prefabContainer; // Contenedor principal del prefab
    [SerializeField] private TMP_Text detailText; // Texto para mostrar Condition o Description
    [SerializeField] private GameObject closeButton; // Bot�n para cerrar detalles
    [SerializeField] private Button conditionButton; // Bot�n de condici�n
    [SerializeField] private Button descriptionButton; // Bot�n de descripci�n

    private bool unlockedAchievement; // Estado del logro

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

        // Configurar texto y estado
        if (titleText != null) titleText.text = data.Title;
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

        // Configurar barra de progreso
        if (progressSlider != null)
        {
            progressSlider.value = data.IsUnlocked ? progressSlider.maxValue : 0;
        }

        // Configurar botones
        conditionButton.gameObject.SetActive(!data.IsUnlocked); // Mostrar solo si est� bloqueado
        descriptionButton.gameObject.SetActive(data.IsUnlocked); // Mostrar solo si est� desbloqueado
        detailText.gameObject.SetActive(false); // Ocultar detalles inicialmente
        closeButton.SetActive(false); // Ocultar bot�n de cerrar inicialmente

        // Asignar texto de descripci�n y condici�n
        conditionButton.onClick.AddListener(() => ShowDetails(data.Condition));
        descriptionButton.onClick.AddListener(() => ShowDetails(data.Description));
        closeButton.GetComponent<Button>().onClick.AddListener(HideDetails);
    }

    // Mostrar los detalles (Condition o Description)
    private void ShowDetails(string detail)
    {
        prefabContainer.SetActive(false); // Ocultar el prefab
        detailText.text = detail; // Configurar texto
        detailText.gameObject.SetActive(true); // Mostrar texto de detalle
        closeButton.SetActive(true); // Mostrar bot�n de cerrar
    }

    // Ocultar detalles y volver al prefab
    public void HideDetails()
    {
        detailText.gameObject.SetActive(false); // Ocultar texto de detalle
        closeButton.SetActive(false); // Ocultar bot�n de cerrar
        prefabContainer.SetActive(true); // Mostrar el prefab original
    }
}
