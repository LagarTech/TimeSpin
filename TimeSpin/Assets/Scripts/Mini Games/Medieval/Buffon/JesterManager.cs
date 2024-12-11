using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JesterManager : MonoBehaviour
{
    [SerializeField] private List<JesterController> _jesters; // Lista de bufones en escena

    private void Update()
    {
        // Obtener las espadas en la escena
        GameObject[] swords = GameObject.FindGameObjectsWithTag("Sword");

        foreach (var sword in swords)
        {
            // Asignar espadas a bufones que no tengan objetivo
            if (!_jesters.Any(j => j.IsTargeting(sword)))
            {
                JesterController availableJester = _jesters.FirstOrDefault(j => !j.HasTarget());
                if (availableJester != null)
                {
                    availableJester.ActivateJester(sword);
                }
            }
        }
    }
}