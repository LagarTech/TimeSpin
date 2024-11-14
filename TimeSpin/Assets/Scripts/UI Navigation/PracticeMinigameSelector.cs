using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PracticeMinigameSelector : MonoBehaviour
{
    public GameObject infoPanel;  // Panel donde se mostrar? la informaci?n
    public TMP_Text infoText;         // Texto del panel donde se mostrar? la descripci?n
    public Button closeInfoButton; // Bot?n para cerrar el panel de informaci?n (la "X")
    public GameObject Descripcion; // Panel o GameObject donde est? la descripci?n del minijuego
    public GameObject infoButton; // Botón para mostrar la información
    public GameObject fondoMinijuego;
    public Image botonMinijuego;

    void Start()
    {
        // Al iniciar, el panel de informaci?n y el bot?n de cerrar estar?n desactivados
        infoPanel.SetActive(true);
        closeInfoButton.gameObject.SetActive(false);  // Ocultar el bot?n de cerrar al inicio

        // A?adimos la funci?n para cerrar al boton "X"
        closeInfoButton.onClick.AddListener(HideInfo);

        botonMinijuego = GetComponent<Image>();
    }

    // Cargar la escena del minijuego seleccionado
    public void LoadMinigame(string sceneName)
    {
        PlayerPrefs.SetInt("GoToSpecificPanel", 1);

        // Transición al minijuego marcando el modo de práctica
        GameSceneManager.instance.practiceStarted = true;
        LoadingScreenManager.instance.SceneToGameTransition(sceneName);
    }

    // Mostrar la informaci?n del minijuego
    public void ShowInfo(string info)
    {
        infoText.text = info;
        infoPanel.SetActive(true);   // Mostrar el panel de información
        Descripcion.SetActive(true); // Asegurarse de que la descripción está activa
        closeInfoButton.gameObject.SetActive(true);  // Mostrar el bot?n de cerrar
        infoButton.SetActive(false);
        fondoMinijuego.SetActive(false);
        Color color = botonMinijuego.color;
        color.a = 0;
        botonMinijuego.color = color;
    }

    // Ocultar la informaci?n
    public void HideInfo()
    {
        closeInfoButton.gameObject.SetActive(false); // Ocultar el bot?n de cerrar
        Descripcion.SetActive(false);                // Ocultar la descripci?n
        infoButton.SetActive(true);
        fondoMinijuego.SetActive(true);
        Color color = botonMinijuego.color;
        color.a = 1;
        botonMinijuego.color = color;
    }
}
