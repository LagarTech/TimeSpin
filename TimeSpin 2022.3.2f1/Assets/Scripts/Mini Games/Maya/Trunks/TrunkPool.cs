using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrunkPool: MonoBehaviour
{
    public static TrunkPool instance;

    [SerializeField] private GameObject _trunkPrefab; // Prefab del tronco
    [SerializeField] private int _poolSize = 15; // Tamaño máximo del pool (10 troncos)
    private List<GameObject> _pool = new List<GameObject>();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        // Inicializar el pool con troncos inactivos
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject trunk = Instantiate(_trunkPrefab);
            trunk.SetActive(false); // Lo mantenemos inactivo hasta que se necesite
            _pool.Add(trunk);
        }
    }

    // Método para obtener un tronco del pool
    public GameObject GetTrunkFromPool()
    {
        foreach (var trunk in _pool)
        {
            if (!trunk.activeInHierarchy)
            {
                trunk.SetActive(true);
                return trunk;
            }
        }
        return null; // Si todos los troncos están en uso, retorna null
    }

    // Método para devolver el tronco al pool
    public void ReturnTrunkToPool(GameObject trunk)
    {
        trunk.SetActive(false);
    }
}
