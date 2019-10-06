﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using UnityEngine;

public class TanksManagers : MBSingleton<TanksManagers> {

    public ClientText playerPointsText;
    public ClientText playerUDPPointsText;



    public bool isServer = true;
    public GameObject redTank;
    public GameObject blueTank;
    public GameObject redBullet;
    public GameObject blueBullet;

    public GameObject networkMenu;
    public GameObject game;


    public void InitGame()
    {
        isServer = NetworkManager.Instance.isServer;
        networkMenu.SetActive(false);
        game.SetActive(true);
        redTank.SetActive(true);
        blueTank.SetActive(true);
        SetPlayers();
        AddListener();
    }

    private void Awake()
    {
        networkMenu.SetActive(true);
        game.SetActive(false);
        redTank.SetActive(false);
        blueTank.SetActive(false);
    }

    public void SetPlayers()
    {
        if (isServer)
        {
            redBullet.AddComponent<Bullet>();
            blueBullet.AddComponent<ClientBullet>();
            Destroy(blueTank.GetComponent<Tank>());
            blueTank.AddComponent<ClientTank>();
        }
        else
        {
            blueBullet.AddComponent<Bullet>();
            redBullet.AddComponent<ClientBullet>();
            Destroy(redTank.GetComponent<Tank>());
            redTank.AddComponent<ClientTank>();
        }
    }

    void AddListener()
    {
        if (!isServer)
        {
            PacketManager.Instance.Awake();
        }
    }

 
}