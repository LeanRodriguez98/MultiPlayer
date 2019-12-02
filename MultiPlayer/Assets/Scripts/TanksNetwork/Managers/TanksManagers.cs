using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using UnityEngine;

public class TanksManagers : MBSingleton<TanksManagers>
{

    public bool isServer = true;
    public GameObject redTank;
    public GameObject blueTank;
    public GameObject redBullet;
    public GameObject blueBullet;
    public GameObject networkMenu;
    public GameObject game;
    public GameManager gameManager;
    public UI_Canvas canvas;

    public bool interpolateTanks;
    public bool interpolateBullets;
    private float clientClock;
    public void InitGame()
    {
        isServer = ConnectionManager.Instance.isServer;
        networkMenu.SetActive(false);
        game.SetActive(true);
        redTank.SetActive(true);
        blueTank.SetActive(true);
        gameManager.enabled = true;
        canvas.enabled = true;
        SetPlayers();
        clientClock = Time.realtimeSinceStartup;
    }
    public float GetClientTime()
    {
        return Mathf.Abs(Time.realtimeSinceStartup - clientClock);
    }
    protected override void Awake()
    {
        base.Awake();
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
            GameManager.Instance.SetLocalPlayer(redTank.GetComponent<Tank>());
        }
        else
        {
            blueBullet.AddComponent<Bullet>();
            redBullet.AddComponent<ClientBullet>();
            Destroy(redTank.GetComponent<Tank>());
            redTank.AddComponent<ClientTank>();
            GameManager.Instance.SetLocalPlayer(blueTank.GetComponent<Tank>());
        }
    }
}
