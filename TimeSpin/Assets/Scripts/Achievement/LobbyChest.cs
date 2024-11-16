using UnityEngine;

public class LobbyChest : MonoBehaviour
{
    public static LobbyChest Instance;

    [SerializeField] private string minigameName; // Nombre del minijuego
    [SerializeField] private GameObject achievementMenu; // Referencia al menú de logros
    [SerializeField] private Transform achievementList; // Contenedor para logros
    [SerializeField] private GameObject achievementPrefab; // Prefab de cada logro
    [SerializeField] private int MAX_DISTANCE = 5; // Distancia máxima para interactuar

    private Transform _playerTransform; // Referencia dinámica al jugador
    private string[] achievements; // Logros asociados al minijuego

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
                "Duración extensa",
                "El fuego",
                "Primeros instrumentos musicales",
                "Pinturas rupestres",
                "Herramientas de piedra",
                "Domesticación de animales",
                "El descubrimiento de Ötzi",
                "Primeros asentamientos"
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
        // Limpia los logros previos en la lista antes de añadir nuevos
        foreach (Transform child in achievementList)
        {
            Destroy(child.gameObject);
        }

        // Mostrar el menú
        achievementMenu.SetActive(true);

        // Rellenar con logros
        foreach (string achievementName in achievements)
        {
            string achievementKey = $"{minigameName}_{achievementName.Replace(" ", "")}";
            bool unlocked = AchievementManager.IsAchievementUnlocked(achievementKey);

            GameObject achievementItem = Instantiate(achievementPrefab, achievementList);
            AchievementItemUI achievementUI = achievementItem.GetComponent<AchievementItemUI>();

            if (achievementUI != null)
            {
                achievementUI.SetAchievementData(achievementName, unlocked);
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

