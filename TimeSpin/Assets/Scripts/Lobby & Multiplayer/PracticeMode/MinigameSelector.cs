using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameSelector : MonoBehaviour
{
    public GameObject infoPanel;  // Panel donde aparecerá la información del minijuego
    public Text infoText;         // Cuadro de texto donde se mostrará la descripción del minijuego

    // Método para cargar la escena del minijuego
    public void LoadMinigame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Método para mostrar la información del minijuego
    public void ShowInfo(string info)
    {
        infoText.text = info;     // Cambia el texto de la info según el minijuego
        infoPanel.SetActive(true); // Activa el panel de información
    }

    // Método para ocultar la información
    public void HideInfo()
    {
        infoPanel.SetActive(false); // Desactiva el panel de información
    }
}


