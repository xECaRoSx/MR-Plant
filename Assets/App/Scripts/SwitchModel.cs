using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchModel : MonoBehaviour
{
    public GameObject diorama_New;
    public GameObject diorama_Old;

    private void Start()
    {
        diorama_New.SetActive(true);
        diorama_Old.SetActive(false);
    }

    public void SwitchModelVersion()
    {
        if (diorama_New.activeSelf)
        {
            diorama_New.SetActive(false);
            diorama_Old.SetActive(true);
        }
        else
        {
            diorama_New.SetActive(true);
            diorama_Old.SetActive(false);
        }
    }
}
