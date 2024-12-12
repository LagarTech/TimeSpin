using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class LobbyChestManager : MonoBehaviour
{
    public static LobbyChestManager Instance;

    private List<LobbyChest> chests = new List<LobbyChest>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Evita duplicados
        }
    }

    public void RegisterChest(LobbyChest chest)
    {
        if (!chests.Contains(chest))
        {
            chests.Add(chest);
            Debug.Log($"Baúl registrado: {chest.name}");
        }
    }

    public void OcultarPanelLogros()
    {
        GameObject.FindGameObjectWithTag("Logros").SetActive(false);

        SelectionTable.Instance.runningGame = true;
        UI_Controller.instance.joystick.SetActive(true);
    }
}
