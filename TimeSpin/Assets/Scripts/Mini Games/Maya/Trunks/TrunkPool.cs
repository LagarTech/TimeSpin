using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TrunkPool : MonoBehaviour
{
    public static TrunkPool instance;

    [SerializeField] private GameObject _trunkPrefab; // Prefab del tronco
    [SerializeField] private int _poolSize = 15; // Tama�o m�ximo del pool (15 troncos)
    [SerializeField] private List<GameObject> _pool = new List<GameObject>();

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
    }

    private void Start()
    {
        // Inicializar el pool con troncos inactivos

        for (int i = 0; i < _poolSize; i++)
        {
            GameObject trunk = Instantiate(_trunkPrefab);

            trunk.SetActive(false); // Mantenerlo inactivo hasta que se necesite
            _pool.Add(trunk);
        }
    }

    // M�todo para obtener un tronco del pool
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
        return null; // Si todos los troncos est�n en uso, retorna null
    }

    // M�todo para devolver el tronco al pool
    public void ReturnTrunkToPool(GameObject trunk)
    {
        // Se desactivan los objetos del pool
        trunk.SetActive(false);
    }

    // Funci�n para que un tronco se a�ada al pool en el cliente, una vez spawnee
    public void AddTrunkToPool(GameObject trunk)
    {
        _pool.Add(trunk);
    }

    public IEnumerator ActiveTrunk()
    {
        // Se espera un tiempo m�nimo para que los troncos se coloquen en la posici�n adecuada en el servidor
        yield return new WaitForSeconds(0.5f);
        // Se avisa a los clientes del id del tronco que deben activar
        ActiveTrunk();
    }

}
