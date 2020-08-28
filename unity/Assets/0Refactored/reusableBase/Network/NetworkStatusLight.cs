﻿using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NetworkStatusLight : MonoBehaviour
{
    public GameObject lightObject;

    private Light _light;

    private ListServer _ls;
    private MaroonNetworkManager _nm;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Scene newly loaded!");
        _light = lightObject.GetComponent<Light>();
        _ls = FindObjectOfType<ListServer>();
        _nm = FindObjectOfType<MaroonNetworkManager>();
        
        InvokeRepeating(nameof(UpdateLightStatus), 0, 1);
    }

    private void UpdateLightStatus()
    {
        lightObject.SetActive(_nm.IsActive());
        if (NetworkClient.active && ClientScene.ready)
        {
            _light.color = Color.green;
        }
        else if (_ls.GetListServerStatus())
        {
            _light.color = Color.yellow;
        }
        else
        {
            _light.color = Color.red;
        }
    }
}
