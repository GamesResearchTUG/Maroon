﻿using System;
using System.Collections;
using System.Collections.Generic;
using GEAR.Localization;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkControl : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI inControlText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Button controlButton;
    
    private enum Mode
    {
        InControl,
        InControlRequest,
        NotInControl,
        NotInControlMyRequest,
        NotInControlOtherRequest,
        NotInControlCooldown
    }

    private Mode _mode;
    private string _clientWithRequest;

    private const int CountdownDuration = 5;
    private int _countdownTime;
    private bool _countdownActive;
    
    private const int CooldownDuration = 15;
    private int _cooldownTime;
    private bool _cooldownActive;
    
    // Start is called before the first frame update
    void Start()
    {
        controlButton.onClick.AddListener(OnClickCommandButton);
        MaroonNetworkManager.Instance.newClientInControlEvent.AddListener(OnNewClientInControl);
        if (isServer)
        {
            if (MaroonNetworkManager.Instance.ClientInControl == null)
            {
                MaroonNetworkManager.Instance.ServerSetClientInControl(MaroonNetworkManager.Instance.PlayerName);
            }
        }

        if (MaroonNetworkManager.Instance.IsInControl)
        {
            _mode = Mode.InControl;
        }
        else
        {
            _mode = Mode.NotInControl;
        }
        UpdateAppearance(); //init
    }

    private void OnNewClientInControl()
    {
        if (MaroonNetworkManager.Instance.IsInControl)
        {
            _mode = Mode.InControl;
        }
        else if (_cooldownActive)
        {
            _mode = Mode.NotInControlCooldown;
        }
        else
        {
            _mode = Mode.NotInControl;
        }
        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        switch (_mode)
        {
            case Mode.InControl:
                inControlText.text = LanguageManager.Instance.GetString("InControl");
                statusText.gameObject.SetActive(false);
                countdownText.gameObject.SetActive(false);
                controlButton.interactable = false;
                controlButton.GetComponent<Image>().color = new Color(0.5f,1, 0.5f, 0.6f);
                break;
            
            case Mode.InControlRequest:
                inControlText.text = _clientWithRequest + " " + LanguageManager.Instance.GetString("ControlRequest");
                statusText.text = LanguageManager.Instance.GetString("ClickToDeny");
                statusText.gameObject.SetActive(true);
                countdownText.gameObject.SetActive(true);
                controlButton.interactable = true;
                controlButton.GetComponent<Image>().color = new Color(1,1, 0.5f, 0.6f);
                break;
            
            case Mode.NotInControl:
                inControlText.text = MaroonNetworkManager.Instance.ClientInControl + " " + LanguageManager.Instance.GetString("IsInControl");
                statusText.text = LanguageManager.Instance.GetString("ClickRequestControl");
                statusText.gameObject.SetActive(true);
                countdownText.gameObject.SetActive(false);
                controlButton.interactable = true;
                controlButton.GetComponent<Image>().color = new Color(1,0.5f, 0.5f, 0.6f);
                break;
            
            case Mode.NotInControlMyRequest:
                inControlText.text = LanguageManager.Instance.GetString("ControlRequested");
                statusText.text = LanguageManager.Instance.GetString("ClickToCancel");
                statusText.gameObject.SetActive(true);
                countdownText.gameObject.SetActive(true);
                controlButton.interactable = true;
                controlButton.GetComponent<Image>().color = new Color(1,1, 0.5f, 0.6f);
                break;
            
            case Mode.NotInControlOtherRequest:
                inControlText.text = _clientWithRequest + " " + LanguageManager.Instance.GetString("ControlRequest");
                statusText.gameObject.SetActive(false);
                countdownText.gameObject.SetActive(true);
                controlButton.interactable = false;
                controlButton.GetComponent<Image>().color = new Color(0.5f,0.5f, 0.5f, 0.6f);
                break;
            
            case Mode.NotInControlCooldown:
                inControlText.text = MaroonNetworkManager.Instance.ClientInControl + " " + LanguageManager.Instance.GetString("IsInControl");
                statusText.text = LanguageManager.Instance.GetString("WaitCooldown");
                statusText.gameObject.SetActive(true);
                countdownText.gameObject.SetActive(true);
                countdownText.text = _cooldownTime.ToString();
                controlButton.interactable = false;
                controlButton.GetComponent<Image>().color = new Color(1,0.5f, 0.5f, 0.6f);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnClickCommandButton()
    {
        switch (_mode)
        {
            case Mode.InControl:
                // cannot happen
                break;
            case Mode.InControlRequest:
                CancelCountdown();
                break;
            case Mode.NotInControl:
                RequestControl();
                break;
            case Mode.NotInControlMyRequest:
                CancelCountdown();
                break;
            case Mode.NotInControlOtherRequest:
                // cannot happen
                break;
            case Mode.NotInControlCooldown:
                // cannot happen
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void RequestControl()
    {
        CmdClientRequestsControl(MaroonNetworkManager.Instance.PlayerName);
    }
    
    [Command(ignoreAuthority = true)]
    private void CmdClientRequestsControl(string client)
    {
        if (_countdownActive)
            return;
        RpcClientRequestsControl(client);
    }

    [ClientRpc]
    private void RpcClientRequestsControl(string client)
    {
        _clientWithRequest = client;
        if (client == MaroonNetworkManager.Instance.PlayerName)
        {
            _mode = Mode.NotInControlMyRequest;
        } 
        else if (MaroonNetworkManager.Instance.IsInControl)
        {
            _mode = Mode.InControlRequest;
        }
        else
        {
            _mode = Mode.NotInControlOtherRequest;
        }
        UpdateAppearance();
        StartCountdown();
    }

    private void StartCountdown()
    {
        _countdownTime = CountdownDuration;
        countdownText.text = _countdownTime.ToString();
        _countdownActive = true;
        InvokeRepeating(nameof(DecreaseCountdown), 1, 1);
    }

    private void DecreaseCountdown()
    {
        _countdownTime--;
        if (_countdownTime <= 0)
        {
            StopCountdown();
            if (isServer)
            {
                MaroonNetworkManager.Instance.ServerSetClientInControl(_clientWithRequest);
            }
        }
        countdownText.text = _countdownTime.ToString();
    }

    private void StopCountdown()
    {
        _countdownTime = 0;
        _countdownActive = false;
        CancelInvoke(nameof(DecreaseCountdown));
    }

    private void CancelCountdown()
    {
        if (!_countdownActive)
            return;
        
        CmdCancelCountdown();
    }

    [Command(ignoreAuthority = true)]
    private void CmdCancelCountdown()
    {
        RpcCancelCountdown();
    }
    
    [ClientRpc]
    private void RpcCancelCountdown()
    {
        StopCountdown();
        if (MaroonNetworkManager.Instance.IsInControl)
        {
            _mode = Mode.InControl;
        }
        else if (_cooldownActive)
        {
            _mode = Mode.NotInControlCooldown;
        }
        else
        {
            _mode = Mode.NotInControl;
        }

        if (_clientWithRequest == MaroonNetworkManager.Instance.PlayerName)
        {
            StartCooldown();
        }
        UpdateAppearance();
    }
    
    private void StartCooldown()
    {
        _mode = Mode.NotInControlCooldown;
        _cooldownTime = CooldownDuration;
        countdownText.text = _cooldownTime.ToString();
        _cooldownActive = true;
        InvokeRepeating(nameof(DecreaseCooldown), 1, 1);
    }

    private void DecreaseCooldown()
    {
        _cooldownTime--;
        if (_cooldownTime <= 0)
        {
            StopCooldown();
        }
        if(!_countdownActive)
            countdownText.text = _cooldownTime.ToString();
    }

    private void StopCooldown()
    {
        _cooldownTime = 0;
        _cooldownActive = false;
        _mode = Mode.NotInControl;
        UpdateAppearance();
        CancelInvoke(nameof(DecreaseCooldown));
    }
}