using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderManager : MonoBehaviour
{
    void Start()
    {
        if(MobileController.instance.isMobile())
        {
            ChangeMaterialShading(0);
        }
        else
        {
            ChangeMaterialShading(1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (MobileController.instance.isMobile())
        {
            ChangeMaterialShading(0);
        }
        else
        {
            ChangeMaterialShading(1);
        }
    }

    private void ChangeMaterialShading(int shaderIndex)
    {
        Renderer[] renderers = FindObjectsOfType<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer.sharedMaterials.Length > 1 && renderer.gameObject.tag != "Material doble")
            {
                // Crear un nuevo array de materiales con el material deseado
                Material[] newMaterials = new Material[1];
                newMaterials[0] = renderer.sharedMaterials[shaderIndex];
                renderer.sharedMaterials = newMaterials; // Asignar el nuevo material
            }
        }
    }
}
