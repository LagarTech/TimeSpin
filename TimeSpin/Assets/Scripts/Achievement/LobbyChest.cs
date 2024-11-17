using System.Collections.Generic;
using UnityEngine;

public class LobbyChest : MonoBehaviour
{
    public static LobbyChest Instance;

    [SerializeField] private string minigameName; // Nombre del minijuego
    [SerializeField] private GameObject achievementMenu; // Referencia al menú de logros
    [SerializeField] private GameObject achievementPrefab; // Prefab de cada logro
    [SerializeField] private int MAX_DISTANCE = 2; // Distancia máxima para interactuar

    private Transform _playerTransform; // Referencia dinámica al jugador
    private string[] achievements; // Logros asociados al minijuego

    [SerializeField] private List <GameObject> listAchievements;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        // Define los logros para cada minijuego
        if (minigameName == "Prehistory")
        {
            achievements = new string[]
            {
                "Duración Extensa",
                "El Fuego",
                "Primeros Instrumentos Musicales",
                "Pinturas Rupestres",
                "Herramientas de Piedra",
                "Domesticación de Animales",
                "El Descubrimiento de Ötzi",
                "Primeros Asentamientos"
            };
        }

        // Inicializar logros como bloqueados si aún no se han registrado
        InitializeAchievements();

        // Ocultar el menú al inicio
        achievementMenu.SetActive(false);
    }

    void Update()
    {
        // Verificar si se presiona la tecla Espacio
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Verificar que haya un jugador y que esté cerca
            if (_playerTransform != null && Vector3.Distance(transform.position, _playerTransform.position) < MAX_DISTANCE)
            {
                ShowAchievements();
            }
        }
    }

    public void AddPlayerReference(Transform playerTransform)
    {
        _playerTransform = playerTransform;
        Debug.Log("Referencia del jugador asignada al baúl.");
    }

    private void ShowAchievements()
    {
        // Activar el menú y la lista
        achievementMenu.SetActive(true);

        // Rellenar la lista con logros
        for (int i = 0; i< achievements.Length; i++)
        {
            string achievementName = achievements[i];
            string achievementKey = $"{minigameName}_{achievementName.Replace(" ", "")}";

            // Imprimir la clave generada
            Debug.Log($"Clave generada: {achievementKey}");

            bool unlocked = AchievementManager.IsAchievementUnlocked(achievementKey);

            Debug.Log($"Logro: {achievementKey}, Estado: {(unlocked ? "Desbloqueado" : "Bloqueado")}");

            GameObject achievementItem = listAchievements[i];
            AchievementItemUI achievementUI = achievementItem.GetComponent<AchievementItemUI>();

            if (achievementUI != null)
            {
                achievementUI.SetAchievementData(achievementName, unlocked);
                achievementItem.GetComponent<RectTransform>().localScale = Vector3.one; // Asegurar escala correcta
            }
            else
            {
                Debug.LogError("Prefab de logro no tiene el script AchievementItemUI.");
            }
        }
    }

    private void InitializeAchievements()
    {
        foreach (string achievementName in achievements)
        {
            string achievementKey = $"{minigameName}_{achievementName.Replace(" ", "")}";
            if (!AchievementManager.IsAchievementUnlocked(achievementKey) && PlayerPrefs.GetInt(achievementKey, -1) == -1)
            {
                // Si no existe el logro en PlayerPrefs, inicializarlo como bloqueado (valor 0)
                PlayerPrefs.SetInt(achievementKey, 0);
            }
        }
    }

}

