using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameSelector : MonoBehaviour
{
    public GameObject infoPanel;  // Panel donde se mostrará la información
    public Text infoText;         // Texto del panel donde se mostrará la descripción
    public Button closeInfoButton; // Botón para cerrar el panel de información (la "X")
    public GameObject Descripcion; // Panel o GameObject donde está la descripción del minijuego

    void Start()
    {
        // Al iniciar, el panel de información y el botón de cerrar estarán desactivados
        infoPanel.SetActive(true);
        closeInfoButton.gameObject.SetActive(false);  // Ocultar el botón de cerrar al inicio

        // Añadimos la función para cerrar al botón "X"
        closeInfoButton.onClick.AddListener(HideInfo);
    }

    // Cargar la escena del minijuego seleccionado
    public void LoadMinigame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Mostrar la información del minijuego
    public void ShowInfo(string info)
    {
        infoText.text = info;
        infoPanel.SetActive(true);   // Mostrar el panel de información
        Descripcion.SetActive(true); // Asegurarse de que la descripción esté activa
        closeInfoButton.gameObject.SetActive(true);  // Mostrar el botón de cerrar
    }

    // Ocultar la información
    public void HideInfo()
    {
        closeInfoButton.gameObject.SetActive(false); // Ocultar el botón de cerrar
        Descripcion.SetActive(false);                // Ocultar la descripción
    }
}



