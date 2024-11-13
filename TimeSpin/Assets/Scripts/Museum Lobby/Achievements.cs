using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Achievements : MonoBehaviour
{
    [System.Serializable]
    public class Logro
    {
        public string nombre;
        public bool desbloqueado;
        public GameObject prefabUI;  // Referencia al prefab del logro en la UI

        public Image imagenLogro;               // Referencia directa al componente Image
        public TextMeshProUGUI textoProgreso;
    }

    public List<Logro> logros = new List<Logro>();
    public GameObject panelLogros;

    void Start()
    {
        CargarLogros();
        ActualizarUI();
    }

    public void InteractuarConBaul(int idBaul)
    {
        panelLogros.SetActive(true);
    }

    void ActualizarUI()
    {
        /*
        foreach (var logro in logros)
        {
            if (logro.desbloqueado)
            {
                logro.prefabUI.GetComponent<Image>().color = Color.white;
                logro.prefabUI.transform.Find("Progreso").GetComponent<TextMeshProUGUI>().text = "1/1";
            }
            else
            {
                logro.prefabUI.GetComponent<Image>().color = Color.gray;
                logro.prefabUI.transform.Find("Progreso").GetComponent<TextMeshProUGUI>().text = "0/1";
            }
        }
        */
            foreach (var logro in logros)
            {
                if (logro == null)
                {
                    Debug.LogWarning("Logro es nulo.");
                    continue;
                }

                if (logro.prefabUI == null)
                {
                    Debug.LogWarning("prefabUI en el logro " + logro.nombre + " es nulo.");
                    continue;
                }

                var imageComponent = logro.prefabUI.GetComponent<Image>();
                var progresoText = logro.prefabUI.transform.Find("Progreso")?.GetComponent<TextMeshProUGUI>();

                if (imageComponent == null)
                {
                    Debug.LogWarning("El prefabUI de " + logro.nombre + " no tiene un componente Image.");
                    continue;
                }

                if (progresoText == null)
                {
                    Debug.LogWarning("El objeto Progreso no existe o no tiene TextMeshProUGUI en el prefabUI de " + logro.nombre);
                    continue;
                }

                if (logro.desbloqueado)
                {
                    imageComponent.color = Color.white;
                    progresoText.text = "1/1";
                }
                else
                {
                    imageComponent.color = Color.gray;
                    progresoText.text = "0/1";
                }
            
        }

    }

    public void DesbloquearLogro(int id)
    {
        if (!logros[id].desbloqueado)
        {
            logros[id].desbloqueado = true;
            GuardarLogros();
            ActualizarUI();
        }
    }

    public void GuardarLogros()
    {
        for (int i = 0; i < logros.Count; i++)
        {
            PlayerPrefs.SetInt("Logro" + i, logros[i].desbloqueado ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    public void CargarLogros()
    {
        for (int i = 0; i < logros.Count; i++)
        {
            logros[i].desbloqueado = PlayerPrefs.GetInt("Logro" + i, 0) == 1;
        }
    }

    void OnApplicationQuit()
    {
        GuardarLogros();
    }


}

