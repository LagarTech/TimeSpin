using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrunkPool: NetworkBehaviour
{
    public static TrunkPool instance;

    [SerializeField] private GameObject _trunkPrefab; // Prefab del tronco
    [SerializeField] private int _poolSize = 15; // Tamaño máximo del pool (10 troncos)
    [SerializeField] private List<GameObject> _pool = new List<GameObject>();

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
        // Inicializar el pool con troncos inactivos, sólo en el servidor
        if (Application.platform != RuntimePlatform.LinuxServer) return;

        for (int i = 0; i < _poolSize; i++)
        {
            GameObject trunk = Instantiate(_trunkPrefab);
            // Se asigna un identificador al tronco
            trunk.GetComponent<TrunkMovement>().idTrunk = i;

            // Asegúrate de que el tronco tiene un NetworkObject
            NetworkObject networkObject = trunk.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                // Spawnea el objeto en la red
                networkObject.Spawn();
            }

            trunk.SetActive(false); // Mantenerlo inactivo hasta que se necesite
            _pool.Add(trunk);
        }

        // Una vez instanciados todos los troncos, se hace el pool en los clientes
        CreatePoolClientRpc();

    }

    // Método para obtener un tronco del pool
    public GameObject GetTrunkFromPool()
    {
        foreach (var trunk in _pool)
        {
            if (!trunk.activeInHierarchy)
            {
                trunk.SetActive(true);
                // Se avisa a los clientes del id del tronco que deben activar
                ActiveTrunkClientRpc(trunk.GetComponent<TrunkMovement>().idTrunk);
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

    // Mediante esta función, todos los clientes tendrán un pool con la referencia a los troncos, para poder activarlos y desactivarlos
    [ClientRpc]
    private void CreatePoolClientRpc()
    {
        // Se buscan todos los objetos spawneados
        GameObject[] poolList = GameObject.FindGameObjectsWithTag("Tronco");
        // Se recorre la lista de troncos
        foreach(var tronco in poolList)
        {
            // Se desactiva el tronco y se añade al pool
            tronco.SetActive(false);
            _pool.Add(tronco);
        }
    }

    [ClientRpc]
    private void ActiveTrunkClientRpc(int trunkId)
    {
        foreach (var trunk in _pool)
        {
            // Se activa el tronco que se haya obtenido del pool en el servidor, caracterizado por el id
            if(trunk.GetComponent<TrunkMovement>().idTrunk == trunkId)
            {
                trunk.SetActive(true);
            }
        }
    }

    public void ChangeScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Future", LoadSceneMode.Single);
    }

}
