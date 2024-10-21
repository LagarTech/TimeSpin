using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameSelector : MonoBehaviour
{
    public GameObject infoPanel;  // Panel donde aparecer� la informaci�n del minijuego
    public Text infoText;         // Cuadro de texto donde se mostrar� la descripci�n del minijuego

    // M�todo para cargar la escena del minijuego
    public void LoadMinigame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // M�todo para mostrar la informaci�n del minijuego
    public void ShowInfo(string info)
    {
        infoText.text = info;     // Cambia el texto de la info seg�n el minijuego
        infoPanel.SetActive(true); // Activa el panel de informaci�n
    }

    // M�todo para ocultar la informaci�n
    public void HideInfo()
    {
        infoPanel.SetActive(false); // Desactiva el panel de informaci�n
    }
}


