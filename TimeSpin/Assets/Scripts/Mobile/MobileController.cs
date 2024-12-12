using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class MobileController : MonoBehaviour
{
    public static MobileController instance;

    public Joystick Joystick;
    public Button interactuar;

    public GameObject panel;


    #region WebGL is on mobile check

    [DllImport("__Internal")]
    private static extern bool IsMobile();

    public bool isMobile()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
                return IsMobile();  
#endif
        return false;
    }

    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (isMobile())
        {
            panel.SetActive(true);
        }
    }


}
