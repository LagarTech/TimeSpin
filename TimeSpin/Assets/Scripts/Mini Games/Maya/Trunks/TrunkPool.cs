using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * FUNCIONAMIENTO EN EL SERVIDOR -> Primero se inicializa el pool, instanciando los 15 troncos y desactivándolos
 * Después, cada X segundos, en TrunkSpawner se genera la orden de obtener un tronco del pool, para ello se genera una posición aleatoria y se toma el primer inactivo de la lista
 * Cada tronco gestiona su movimiento mediante TrunkMovement, además del momento en el que se tienen que desactivar
 * FUNCIONAMIENTO EN EL CLIENTE -> Según se instancian los 15 troncos en el servidor, en red, aparecen en los clientes
 * Estos, mediante su script, desactivan su Mesh y se auto añaden al pool
 * Cada vez que el servidor necesita obtener un nuevo tronco del pool, indica cuál es su identificador y se lo pasa a los clientes, para que estos activen el Mesh apropiado para que se vea
 * Si deben desaparecer, los clientes vuelven a desactivar el Mesh
*/

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
        // En el servidor, se desactivan los objetos del pool
        if(Application.platform == RuntimePlatform.LinuxServer)
        {
            trunk.SetActive(false);
        }
        else
        {
            // En el cliente, se desactiva sólo el mesh, para no perder la referencia a ellos
            trunk.GetComponent<TrunkMovement>().mesh.enabled = false;
        }
    }

    // Función para que un tronco se añada al pool en el cliente, una vez spawnee
    public void AddTrunkToPool(GameObject trunk)
    {
        _pool.Add(trunk);
    }

    public IEnumerator ActiveTrunk(ulong networkObjectId)
    {
        // Se espera un tiempo mínimo para que los troncos se coloquen en la posición adecuada en el servidor
        yield return new WaitForSeconds(0.5f);
        // Se avisa a los clientes del id del tronco que deben activar
        ActiveTrunkClientRpc(networkObjectId);
    }

    [ClientRpc]
    private void ActiveTrunkClientRpc(ulong networkObjectId)
    {
        // Buscar el NetworkObject por su NetworkObjectId en el pool de los clientes
        foreach(var trunk in _pool)
        {
            TrunkMovement trunkMove = trunk.GetComponent<TrunkMovement>();
            if (trunkMove.trunkNetworkId == (int) networkObjectId)
            {
                // Se reactiva el modelo
                trunkMove.mesh.enabled = true;
            }
        }
    }

    public void ChangeScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Future", LoadSceneMode.Single);
    }

}
