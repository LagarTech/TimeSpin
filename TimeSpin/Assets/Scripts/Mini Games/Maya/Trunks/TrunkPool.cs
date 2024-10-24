using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * FUNCIONAMIENTO EN EL SERVIDOR -> Primero se inicializa el pool, instanciando los 15 troncos y desactiv�ndolos
 * Despu�s, cada X segundos, en TrunkSpawner se genera la orden de obtener un tronco del pool, para ello se genera una posici�n aleatoria y se toma el primer inactivo de la lista
 * Cada tronco gestiona su movimiento mediante TrunkMovement, adem�s del momento en el que se tienen que desactivar
 * FUNCIONAMIENTO EN EL CLIENTE -> Seg�n se instancian los 15 troncos en el servidor, en red, aparecen en los clientes
 * Estos, mediante su script, desactivan su Mesh y se auto a�aden al pool
 * Cada vez que el servidor necesita obtener un nuevo tronco del pool, indica cu�l es su identificador y se lo pasa a los clientes, para que estos activen el Mesh apropiado para que se vea
 * Si deben desaparecer, los clientes vuelven a desactivar el Mesh
*/

public class TrunkPool: NetworkBehaviour
{
    public static TrunkPool instance;

    [SerializeField] private GameObject _trunkPrefab; // Prefab del tronco
    [SerializeField] private int _poolSize = 15; // Tama�o m�ximo del pool (10 troncos)
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
        // Inicializar el pool con troncos inactivos, s�lo en el servidor
        if (Application.platform != RuntimePlatform.LinuxServer) return;

        for (int i = 0; i < _poolSize; i++)
        {
            GameObject trunk = Instantiate(_trunkPrefab);

            // Aseg�rate de que el tronco tiene un NetworkObject
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
        // En el servidor, se desactivan los objetos del pool
        if(Application.platform == RuntimePlatform.LinuxServer)
        {
            trunk.SetActive(false);
        }
        else
        {
            // En el cliente, se desactiva s�lo el mesh, para no perder la referencia a ellos
            trunk.GetComponent<TrunkMovement>().mesh.enabled = false;
        }
    }

    // Funci�n para que un tronco se a�ada al pool en el cliente, una vez spawnee
    public void AddTrunkToPool(GameObject trunk)
    {
        _pool.Add(trunk);
    }

    public IEnumerator ActiveTrunk(ulong networkObjectId)
    {
        // Se espera un tiempo m�nimo para que los troncos se coloquen en la posici�n adecuada en el servidor
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
