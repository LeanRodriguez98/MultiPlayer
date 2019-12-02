using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;

public class NetworkScreen : MBSingleton<NetworkScreen>
{
    public Button connectBtn;
    public Button startServerBtn;
    public InputField portInputField;
    public InputField addressInputField;

    protected override void Initialize()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        startServerBtn.onClick.AddListener(OnStartServerBtnClick);
    }

    void OnConnectBtnClick()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = System.Convert.ToInt32("1111");

        ConnectionManager.Instance.StartClient(ipAddress, port);
        
        SwitchToChatScreen();
    }

    void OnStartServerBtnClick()
    {
        int port = System.Convert.ToInt32("1111");
        ConnectionManager.Instance.StartServer(port);
        SwitchToChatScreen();
    }

    void SwitchToChatScreen()
    {
        TanksManagers.Instance.InitGame();
    }
}
