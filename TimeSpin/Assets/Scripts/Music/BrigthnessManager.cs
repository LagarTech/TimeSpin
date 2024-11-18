using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrigthnessManager : MonoBehaviour
{
    private Image _brillo;
    private void Awake()
    {
        _brillo = GetComponent<Image>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update()
    {
        _brillo.color = new Color(_brillo.color.r, _brillo.color.g, _brillo.color.b, 1 - GameSceneManager.instance.Brigthness);
    }
}
