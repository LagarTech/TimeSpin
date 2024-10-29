using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : NetworkBehaviour
{
    public static LoadingScreenManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator LoadingScreenCoroutine(string startedScene)
    {
        // Se busca la pantalla de carga en la escena, solo en los clientes
        if (Application.platform != RuntimePlatform.LinuxServer)
        {
            GameSceneManager.instance.ActivePlayersList();
            GameObject loadingScreen = GameObject.FindGameObjectWithTag("PantallaCarga");
            // Se activa el objeto con la pantalla de carga
            foreach (Transform child in loadingScreen.transform)
            {
                child.gameObject.SetActive(true);
            }

            // Se gestionan los fundidos de la imagen y los textos
            // Se obtiene el fondo
            Image background = loadingScreen.GetComponentInChildren<Image>();
            // Se obtienen los dos textos de la pantalla
            List<TMP_Text> screenText = new List<TMP_Text>();
            TMP_Text loading = GameObject.FindGameObjectWithTag("Cargando").GetComponent<TMP_Text>();
            screenText.Add(loading);
            TMP_Text loadingText = GameObject.FindGameObjectWithTag("TextoCarga").GetComponent<TMP_Text>(); // Este texto después se podrá modificar en cada ocasión
            screenText.Add(loadingText);

            // Fade in (aparecer)
            yield return Fade(background, screenText, 1f, 0f, 1f); // De 0 (invisible) a 1 (visible)
            yield return new WaitForSeconds(4f); // Espera 4 segundos
            // Fade out (desaparecer)
            yield return Fade(background, screenText, 1f, 1f, 0f); // De 1 (visible) a 0 (invisible)
            // Se desactiva el objeto con la pantalla de carga
            foreach (Transform child in loadingScreen.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
        else
        {
            // La logica de espera se ejecuta en el servidor
            // Se esperan 5 segundos, para dar tiempo a la colocacion de cada escena
            yield return new WaitForSeconds(5f);
            // Una vez hecho esto, en funcion de la escena a la que se ha transicionado, se activa el minijuego adecuado y se avisa a los clientes
            switch (startedScene)
            {
                case "LobbyMenu":
                    StartLobbyClientRpc();
                    break;
                case "Prehistory":
                    StartPrehistoryClientRpc();
                    break;
                case "Egipt":
                    GridManager.Instance.runningGame = true; // Se inicia la logica del juego en el servidor
                    StartEgiptClientRpc();
                    break;
                case "Medieval":
                    StartMedievalClientRpc();
                    break;
                case "Maya":
                    RaceManager.instance.runningGame = true; // Se inicia la logica del juego en el servidor
                    StartMayaClientRpc();
                    break;
                case "Future":
                    GravityManager.Instance.runningGame = true; // Se inicia la logica del juego en el servidor
                    StartFutureClientRpc();
                    break;
            }
        }
    }

    private IEnumerator Fade(Image image, List<TMP_Text> texts, float startAlphaImage, float startAlphaTexts, float endAlpha, float duration = 0.5f)
    {
        float elapsed = 0f;

        // Establece el valor inicial de opacidad para la imagen y los textos
        Color imageColor = image.color;
        imageColor.a = startAlphaImage;
        image.color = imageColor;

        foreach (var text in texts)
        {
            Color textColor = text.color;
            textColor.a = startAlphaTexts;
            text.color = textColor;
        }

        // Realiza la interpolación de opacidad
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlphaImage = Mathf.Lerp(startAlphaImage, endAlpha, elapsed / duration);

            // Actualiza la opacidad de la imagen
            imageColor.a = newAlphaImage;
            image.color = imageColor;

            // Actualiza la opacidad de cada texto
            float newAlphaText = Mathf.Lerp(startAlphaTexts, endAlpha, elapsed / duration);
            foreach (var text in texts)
            {
                Color textColor = text.color;
                textColor.a = newAlphaText;
                text.color = textColor;
            }

            yield return null;
        }

        // Asegura el valor final de opacidad para la imagen y los textos
        imageColor.a = endAlpha;
        image.color = imageColor;

        foreach (var text in texts)
        {
            Color textColor = text.color;
            textColor.a = endAlpha;
            text.color = textColor;
        }
    }

    public IEnumerator FinalFade()
    {
        Image background = GameObject.FindGameObjectWithTag("Fundido").GetComponent<Image>();

        float elapsed = 0f;
        float duration = 0.5f;
        Color imageColor = background.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(0f, 1f, elapsed / duration);

            // Actualiza la opacidad de la imagen
            imageColor.a = newAlpha;
            background.color = imageColor;

            yield return null;
        }
    }

    public IEnumerator ServerSceneTransition(string sceneName)
    {
        // Avisa a los clientes para que realicen el fundido
        SceneTransitionClientRpc();
        // Espera 0.5 segundos para que se realice el fundido en los clientes
        yield return new WaitForSeconds(0.5f);
        // Carga la siguiente escena
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    [ClientRpc]
    public void SceneTransitionClientRpc()
    {
        // Se inicia el fundido a negro de la escena
        StartCoroutine(FinalFade());
    }


    [ClientRpc]
    private void StartLobbyClientRpc()
    {

    }

    [ClientRpc]
    private void StartPrehistoryClientRpc()
    {

    }

    [ClientRpc]
    private void StartEgiptClientRpc()
    {
        // Se activa el minijuego
        GridManager.Instance.runningGame = true;
    }

    [ClientRpc]
    private void StartMedievalClientRpc()
    {
    }

    [ClientRpc]
    private void StartMayaClientRpc()
    {
        // Se activa el miniijuego
        RaceManager.instance.runningGame = true;
    }

    [ClientRpc]
    private void StartFutureClientRpc()
    {
        // Se activa el minijuego
        GravityManager.Instance.runningGame = true;
    }

}
