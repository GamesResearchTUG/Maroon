﻿using System.Collections;
using System.Collections.Generic;
using GEAR.Localization;
using Maroon.UI;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSpawnMessage : MessageBase
{
    public Vector3 CharacterPosition;
    public Quaternion CharacterRotation;
}

public class MaroonNetworkManager : NetworkManager
{
    [HideInInspector]
    public static MaroonNetworkManager Instance = null;
    
    private ListServer _listServer;
    private MaroonNetworkDiscovery _networkDiscovery;
    private PortForwarding _upnp;
    private GameManager _gameManager;
    private DialogueManager _dialogueManager;

    private bool _isStarted;
    private bool _activePortMapping;
    private bool _tryClientConnect = true;

    public override void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
        _listServer = GetComponent<ListServer>();
        _networkDiscovery = GetComponent<MaroonNetworkDiscovery>();
        _upnp = GetComponent<PortForwarding>();
        _gameManager = FindObjectOfType<GameManager>();
    }

    public void StartMultiUser()
    {
        if(_isStarted)
            return;
        _listServer.ConnectToListServer();
        _networkDiscovery.StartDiscovery();
        _isStarted = true;
    }

    public bool IsActive()
    {
        return _isStarted;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _networkDiscovery.AdvertiseServer();
        _upnp.SetupPortForwarding();
        NetworkServer.RegisterHandler<CharacterSpawnMessage>(OnCreateCharacter);
    }

    private void OnCreateCharacter(NetworkConnection conn, CharacterSpawnMessage message)
    {
        GameObject playerObject = Instantiate(playerPrefab);
        playerObject.transform.position = message.CharacterPosition;
        playerObject.transform.rotation = message.CharacterRotation;

        NetworkServer.AddPlayerForConnection(conn, playerObject);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (mode == NetworkManagerMode.ClientOnly)
        {
            _networkDiscovery.StopDiscovery();
            _tryClientConnect = true;
        }
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        if (_tryClientConnect)
        {
            //Connection attempt failed!
            _tryClientConnect = false;
            DisplayMessage("ClientConnectFail");
        }
        else
        {
            //Disconnected from host
            DisplayMessage("ClientDisconnect");
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        _networkDiscovery.StartDiscovery();
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        _tryClientConnect = false;
        
        if(SceneManager.GetActiveScene().name.Contains("Laboratory"))
            SendCreatePlayerMessage(conn);
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
        
        if(SceneManager.GetActiveScene().name.Contains("Laboratory"))
            SendCreatePlayerMessage(conn);
    }

    private void SendCreatePlayerMessage(NetworkConnection conn)
    {
        // you can send the message here, or wherever else you want
        CharacterSpawnMessage characterMessage = new CharacterSpawnMessage
        {
            CharacterPosition = _gameManager.GetPlayerPosition(),
            CharacterRotation = _gameManager.GetPlayerRotation()
        };

        conn.Send(characterMessage);
    }

    public void PortsMapped()
    {
        //TODO: Manual Port Mapping
        _activePortMapping = true;
        _listServer.PortMappingSuccessfull();
    }
    
    public override void OnApplicationQuit()
    {
        if (_activePortMapping)
        {
            _upnp.DeletePortMapping();
        }
        base.OnApplicationQuit();
    }
    
    private void DisplayMessage(string messageKey)
    {
        if (_dialogueManager == null)
            _dialogueManager = FindObjectOfType<DialogueManager>();

        if (_dialogueManager == null)
            return;

        var message = LanguageManager.Instance.GetString(messageKey);
        _dialogueManager.ShowMessage(message);
    }
}
