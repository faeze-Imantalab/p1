using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loading : MonoBehaviour
{

    public GameObject loadObject;

    public static Loading instance;

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public void Set(bool state)
    {
        loadObject.SetActive(state);
    }
}
