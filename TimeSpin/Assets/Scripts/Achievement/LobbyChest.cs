using UnityEngine;

public class LobbyChest : MonoBehaviour
{
    public static LobbyChest Instance;

    [SerializeField] private string minigameName; // Nombre del minijuego
    [SerializeField] private GameObject achievementMenu; // Referencia al men� de logros
    [SerializeField] private Transform achievementList; // Contenedor para logros
    [SerializeField] private GameObject achievementPrefab; // Prefab de cada logro
    [SerializeField] private int MAX_DISTANCE = 5; // Distancia m�xima para interactuar

    private Transform _playerTransform; // Referencia din�mica al jugador
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
                "Duraci�n extensa",
                "El fuego",
                "Primeros instrumentos musicales",
                "Pinturas rupestres",
                "Herramientas de piedra",
                "Domesticaci�n de animales",
                "El descubrimiento de �tzi",
                "Primeros asentamientos"
            };
        }

        // Inicializar logros como bloqueados si a�n no se han registrado
        InitializeAchievements();

        // Ocultar el men� al inicio
        achievementMenu.SetActive(false);
    }

    void Update()
    {
        // Verificar si se presiona la tecla Espacio
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Verificar que haya un jugador y que est� cerca
            if (_playerTransform != null && Vector3.Distance(transform.position, _playerTransform.position) < MAX_DISTANCE)
            {
                ShowAchievements();
            }
        }
    }

    public void AddPlayerReference(Transform playerTransform)
    {
        _playerTransform = playerTransform;
        Debug.Log("Referencia del jugador asignada al ba�l.");
    }

    private void ShowAchievements()
    {
        // Limpia los logros previos en la lista antes de a�adir nuevos
        foreach (Transform child in achievementList)
        {
            Destroy(child.gameObject);
        }

        // Mostrar el men�
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

