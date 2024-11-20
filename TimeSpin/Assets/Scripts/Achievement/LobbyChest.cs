using System.Collections.Generic;
using UnityEngine;
using static AchievementItemUI;

public class LobbyChest : MonoBehaviour
{
    [SerializeField] private string minigameName; // Nombre del minijuego
    [SerializeField] private GameObject achievementMenu; // Referencia al menú de logros
    [SerializeField] private int MAX_DISTANCE = 2; // Distancia máxima para interactuar

    private Transform _playerTransform; // Referencia dinámica al jugador

    // Lista de ScriptableObjects que contienen los datos de los logros
    [SerializeField] private List<AchievementScriptable> achievements;

    // Prefabs o elementos de UI en la jerarquía que representan logros
    [SerializeField] private List<GameObject> listAchievements;

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
        // Comprobar si el jugador está cerca y pulsa "Espacio" para abrir logros
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (_playerTransform != null && Vector3.Distance(transform.position, _playerTransform.position) < MAX_DISTANCE)
            {
                LobbyChestManager.Instance?.OpenAchievements(this);
            }
        }
    }

    public void CloseAchievements()
    {
        achievementMenu.SetActive(false);
    }

    public void ShowAchievements()
    {
        if (achievements.Count != listAchievements.Count)
        {
            Debug.LogError("La cantidad de logros no coincide con la cantidad de elementos visuales en listAchievements.");
            return;
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
}


