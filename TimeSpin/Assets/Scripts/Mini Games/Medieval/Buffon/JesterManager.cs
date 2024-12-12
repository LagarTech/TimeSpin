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

        // Mantener un conjunto de espadas ya asignadas para evitar duplicados
        HashSet<GameObject> assignedSwords = new HashSet<GameObject>();

        // Recorremos cada buf�n y le asignamos una espada disponible
        foreach (var jester in _jesters)
        {
            // Si el buf�n no tiene un objetivo actual
            if (!jester.HasTarget())
            {
                // Buscar una espada que a�n no haya sido asignada, no est� siendo objetivo de otros bufones, y su posici�n Y sea menor a 1
                GameObject availableSword = swords
                    .FirstOrDefault(sword =>
                        !assignedSwords.Contains(sword) &&
                        !_jesters.Any(j => j.IsTargeting(sword)) &&
                        sword.transform.position.y < 0.2
                    );

                if (availableSword != null)
                {
                    jester.ActivateJester(availableSword);
                    assignedSwords.Add(availableSword); // Marcar esta espada como asignada
                }
            }
        }
    }
}