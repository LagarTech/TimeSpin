using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    void Update()
    {
        transform.localPosition = new Vector3(0, -1.0f, 0);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
    }
}
