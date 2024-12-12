using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static AchievementItemUI;

public class LobbyChest : MonoBehaviour
{
    [SerializeField] public string minigameName; // Nombre del minijuego
    [SerializeField] private GameObject achievementMenu; // Referencia al menú de logros
    [SerializeField] private int MAX_DISTANCE = 3; // Distancia máxima para interactuar

    private Transform _playerTransform; // Referencia dinámica al jugador

    // Lista de ScriptableObjects que contienen los datos de los logros
    [SerializeField] public List<AchievementScriptable> achievements;

    // Prefabs o elementos de UI en la jerarquía que representan logros
    [SerializeField] private List<GameObject> listAchievements;

    // Título del panel
    [SerializeField] private TMP_Text titulo;

    void Start()
    {
        // Registra este baúl con el manager
        LobbyChestManager.Instance?.RegisterChest(this);

        // Inicializar logros si aún no están registrados
        InitializeAchievements();

        // Ocultar el menú al inicio
        achievementMenu.SetActive(false);
    }

    void Update()
    {
    }

    public void CloseAchievements()
    {
        achievementMenu.SetActive(false);
        SelectionTable.Instance.runningGame = true;
    }

    public void ShowAchievements()
    {   
        SelectionTable.Instance.runningGame = false;
        UI_Controller.instance.joystick.SetActive(false);
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().InteractPlayer();
        string title = "";
        switch(minigameName)
        {
            case "Prehistory":
                title = "PREHISTORIA";
                break;
            case "Future":
                title = "FUTURO";
                break;
            default:
                title = minigameName.ToUpper();
                break;

        }
        // Se cambia el título del panel
        titulo.text = "LOGROS " + title;
        achievementMenu.SetActive(true);
        if (achievements.Count != listAchievements.Count)
        {
            Debug.LogError("La cantidad de logros no coincide con la cantidad de elementos visuales en listAchievements.");
            return;
        }
        for (int j = 0; j < achievements.Count; j++)
        {
            string achievementKey = $"{minigameName}_{achievements[j].Title.Replace(" ", "")}";
            int desbloqueado = PlayerPrefs.GetInt(achievementKey);

            if (desbloqueado == 0)
            {
                achievements[j].IsUnlocked = false;
            }
            else
            {
                achievements[j].IsUnlocked = true;
            }
        }

        for (int i = 0; i < achievements.Count; i++)
        {
            if (achievements[i] == null || listAchievements[i] == null)
            {
                Debug.LogError($"El logro o el elemento visual en la posición {i} es null.");
                continue;
            }

            AchievementScriptable achievementData = achievements[i];
            GameObject achievementItem = listAchievements[i];
            AchievementItemUI achievementUI = achievementItem.GetComponent<AchievementItemUI>();

            if (achievementUI != null)
            {
                achievementUI.SetAchievementData(new AchievementItemUI.AchievementData
                {
                    Title = achievementData.Title,
                    Description = achievementData.Description,
                    Condition = achievementData.Condition,
                    IsUnlocked = achievementData.IsUnlocked
                });
            }
            else
            {
                Debug.LogError("Prefab de logro no tiene el script AchievementItemUI.");
            }
        }
    }


    private void InitializeAchievements()
    {
        foreach (AchievementScriptable achievementData in achievements)
        {
            string achievementKey = $"{minigameName}_{achievementData.Title.Replace(" ", "")}";
            if (!AchievementManager.IsAchievementUnlocked(achievementKey) && PlayerPrefs.GetInt(achievementKey, -1) == -1)
            {
                // Si no existe el logro en PlayerPrefs, inicializarlo como bloqueado (valor 0)
                PlayerPrefs.SetInt(achievementKey, 0);
            }
        }
    }

    public void InteractChest()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (_playerTransform != null && Vector3.Distance(transform.position, _playerTransform.position) < MAX_DISTANCE)
        {
            ShowAchievements();

            Debug.Log("space pulsado");
        }
    }
}


