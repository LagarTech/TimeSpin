using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameSelector : MonoBehaviour
{
    public GameObject infoPanel;  // Panel donde se mostrar? la informaci?n
    public TMP_Text infoText;         // Texto del panel donde se mostrar? la descripci?n
    public Button closeInfoButton; // Bot?n para cerrar el panel de informaci?n (la "X")
    public GameObject Descripcion; // Panel o GameObject donde est? la descripci?n del minijuego

    void Start()
    {
        // Al iniciar, el panel de informaci?n y el bot?n de cerrar estar?n desactivados
        infoPanel.SetActive(true);
        closeInfoButton.gameObject.SetActive(false);  // Ocultar el bot?n de cerrar al inicio

        // A?adimos la funci?n para cerrar al bot?n "X"
        closeInfoButton.onClick.AddListener(HideInfo);
    }

    // Cargar la escena del minijuego seleccionado
    public void LoadMinigame(string sceneName)
    {
        PlayerPrefs.SetInt("GoToSpecificPanel", 1);
        SceneManager.LoadScene(sceneName);
    }

    // Mostrar la informaci?n del minijuego
    public void ShowInfo(string info)
    {
        infoText.text = info;
        infoPanel.SetActive(true);   // Mostrar el panel de informaci?n
        Descripcion.SetActive(true); // Asegurarse de que la descripci?n est? activa
        closeInfoButton.gameObject.SetActive(true);  // Mostrar el bot?n de cerrar
    }

    // Ocultar la informaci?n
    public void HideInfo()
    {
        closeInfoButton.gameObject.SetActive(false); // Ocultar el bot?n de cerrar
        Descripcion.SetActive(false);                // Ocultar la descripci?n
    }
}



