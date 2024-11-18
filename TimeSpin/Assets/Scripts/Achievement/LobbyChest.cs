using System.Collections.Generic;
using UnityEngine;

public class LobbyChest : MonoBehaviour
{

    [SerializeField] private string minigameName; // Nombre del minijuego
    [SerializeField] private GameObject achievementMenu; // Referencia al menú de logros
    [SerializeField] private GameObject achievementPrefab; // Prefab de cada logro
    [SerializeField] private int MAX_DISTANCE = 2; // Distancia máxima para interactuar

    private Transform _playerTransform; // Referencia dinámica al jugador
    private string[] achievements; // Logros asociados al minijuego

    [SerializeField] private List <GameObject> listAchievements;


    void Start()
    {
        // Registra este baúl con el manager
        LobbyChestManager.Instance?.RegisterChest(this);

        // Define los logros para cada minijuego 
        switch (minigameName)
        {
            case "Prehistory":

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
                break;

            case "Medieval":

                achievements = new string[]
                {
                    "Caballeros y Torneos",
                    "La Peste Negra",
                    "Las Cruzadas",
                    "Las Ciudades Amuralladas",
                    "Mujeres en el Medievo",
                    "Los Castillos",
                    "El Sistema Feudal",
                    "Los Gremios"
                };
                break;

            case "Egipto":

                achievements = new string[]
                {
                    "Astrología de Pirámides",
                    "Escritura Jeroglífica",
                    "Gatos Sagrados",
                    "Diversidad en Faraones",
                    "El Maquillaje de los Ojos",
                    "Calendario Egipcio",
                    "Mujeres con Derechos",
                    "Proceso de Momificación"
                };
                break;

            case "Maya":

                achievements = new string[]
                {
                    "Calendario Avanzado",
                    "Pirámides Escalonadas",
                    "Escritura Jeroglífica",
                    "Conocimientos Astronómicos",
                    "Sacrificios Humanos",
                    "Juego de Pelota",
                    "Ingeniería Hidráulica",
                    "Abandono de las Ciudades"
                };
                break;

            case "Future":

                achievements = new string[]
                {
                    "Coches Voladores",
                    "Carreras Espaciales",
                    "Vida en otros Planetas",
                    "Alienígenas",
                    "Inteligencia Artificial y Robots",
                    "Ciudades",
                    "Automatización del Hogar",
                    "Tecnología de Comunicación"
                };
                break;

            default:
                achievements = new string[] { }; // Por si no se encuentra una etapa válida
                Debug.LogWarning($"Error");
                break;
        }
            

        // Inicializar logros como bloqueados si aún no se han registrado
        InitializeAchievements();

        // Ocultar el menú al inicio
        achievementMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
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


    public void AddPlayerReference(Transform playerTransform)
    {
        _playerTransform = playerTransform;
        Debug.Log("Referencia del jugador asignada al baúl.");
    }

    public void ShowAchievements()
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

