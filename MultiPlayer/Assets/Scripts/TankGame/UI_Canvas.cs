using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Canvas : MBSingleton<UI_Canvas>
{
    [System.Serializable]
    public struct PlayerUIData
    {
        public Text playerHealt;
        public Text bulletSpeed;
        public Text cannonAngle;
    }
    public Text timeText;
    public Text playerTurnText;
    public PlayerUIData redTankUI;
    public PlayerUIData blueTankUI;
    private PlayerUIData playerUIData;
    public GameObject gameOverScreen;
    private bool serverPlayerTurn = true;

    public string yourTurnText = "Your Turn";
    public string oponentTurnText = "Oponent Turn";
    void Start()
    {
        ClientPlayerUIData clientUIData;
        if (NetworkManager.Instance.isServer)
        {
            playerUIData = redTankUI;
            clientUIData = gameObject.AddComponent<ClientPlayerUIData>();
            clientUIData.SetUIToUpdate(blueTankUI);
            //UpdatePlayerTurn();
        }
        else
        {
            playerUIData = blueTankUI;
            clientUIData = gameObject.AddComponent<ClientPlayerUIData>();
            clientUIData.SetUIToUpdate(redTankUI);
           // ClientTurnSign clientTurnSign;
           // clientTurnSign = gameObject.AddComponent<ClientTurnSign>();
           // clientTurnSign.SetTurnSign(playerTurnText);
           // clientTurnSign.SetClockSign(timeText);
           // clientTurnSign.Init();
           // playerTurnText.text = oponentTurnText;
        }
        clientUIData.Init();
    }

    public void SetTimeText(string _timeText)
    {
        timeText.text = _timeText;
        MessageManager.Instance.SendClockSing(_timeText, ObjectsID.clockSignObjectID);
    }

    public void SetPlayerUIData(Tank tank)
    {
        playerUIData.playerHealt.text = "Lifes " + tank.lifes.ToString();
        playerUIData.bulletSpeed.text = "Bullet Speed " + tank.bulletVelocity.ToString("00.00");
        playerUIData.cannonAngle.text = "Cannon Angle " + Mathf.Abs(tank.AuxAngle).ToString("00.00");
        string dataBuffer = playerUIData.playerHealt.text + '語' + playerUIData.bulletSpeed.text + '語' + playerUIData.cannonAngle.text;
        MessageManager.Instance.SendPlayerUI(dataBuffer, ObjectsID.playerUIObjectID);
    }

    public void UpdatePlayerTurn()
    {
        
        if (serverPlayerTurn)
        {
            playerTurnText.text = yourTurnText;
            MessageManager.Instance.SendTurnSing(oponentTurnText, ObjectsID.turnSignObjectID);
        }
        else
        {
            playerTurnText.text = oponentTurnText;
            MessageManager.Instance.SendTurnSing(yourTurnText, ObjectsID.turnSignObjectID);
        } 
        serverPlayerTurn = !serverPlayerTurn;
    }

    public void OnGameOver()
    {
        gameOverScreen.gameObject.SetActive(true);
    }
}