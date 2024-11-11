using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DinosaurPool : MonoBehaviour
{
    public static DinosaurPool Instance;

    [SerializeField] private GameObject[] dinosaurPrefabs; // Array con los prefabs de los 3 tipos de dinosaurios
    private List<GameObject> pool;
    private const int POOL_SIZE = 15; // Tamaño total del pool

    private void Awake()
    {
        if(Instance == null)
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
        pool = new List<GameObject>();

        // Crear el pool inicial de dinosaurios
        for (int i = 0; i < POOL_SIZE; i++)
        {
            // Seleccionar aleatoriamente un prefab de dinosaurio
            GameObject dinosaurPrefab = dinosaurPrefabs[Random.Range(0, dinosaurPrefabs.Length)];
            GameObject dinosaur = Instantiate(dinosaurPrefab);
            dinosaur.SetActive(false); // Inicia desactivado
            pool.Add(dinosaur);
        }
    }

    // Método para obtener un dinosaurio del pool
    public GameObject GetDinosaur()
    {
        // Filtrar los dinosaurios que están desactivados
        List<GameObject> inactiveDinosaurs = pool.FindAll(dino => !dino.activeInHierarchy);

        if (inactiveDinosaurs.Count > 0)
        {
            // Escoger un dinosaurio aleatorio de los desactivados
            GameObject selectedDinosaur = inactiveDinosaurs[Random.Range(0, inactiveDinosaurs.Count)];
            selectedDinosaur.SetActive(true);
            return selectedDinosaur;
        }

        return null;
    }

    // Método para devolver un dinosaurio al pool
    public void ReturnDinosaur(GameObject dinosaur)
    {
        dinosaur.SetActive(false);
    }
}
