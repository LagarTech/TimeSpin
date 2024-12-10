using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JesterManager : MonoBehaviour
{
    [SerializeField] private List<JesterController> _jesters; // Lista de bufones en escena

    private void Update()
    {
        // Contar las espadas en la escena
        GameObject[] swords = GameObject.FindGameObjectsWithTag("Sword");

        if (swords.Length >= 3)
        {
            ActivateJester(swords);
        }
    }

    private void ActivateJester(GameObject[] swords)
    {
        // Ordenar espadas por prioridad: Oro, Plata, Bronce
        GameObject targetSword = swords
            .OrderByDescending(sword => GetSwordPriority(sword))
            .FirstOrDefault();

        // Buscar un bufón disponible
        JesterController availableJester = _jesters.FirstOrDefault(j => !j.IsActive);
        if (availableJester != null && targetSword != null)
        {
            availableJester.ActivateJester(targetSword);
        }
    }

    private int GetSwordPriority(GameObject sword)
    {
        if (sword.name.Contains("Espada dorada")) return 3;    // Oro tiene prioridad 3
        if (sword.name.Contains("Espada plateada")) return 2;  // Plata tiene prioridad 2
        if (sword.name.Contains("Espada bronce")) return 1;    // Bronce tiene prioridad 1
        return 0;                                               // Por defecto, menor prioridad
    }
}
