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

    public PlayerUIData playerUIServerData;
    public PlayerUIData playerUIClientData;
    private PlayerUIData playerUIData;

    public GameObject gameOverScreen;
    private int currentPlayerTurn = 0;
    private ClientPlayerUIData clientPlayerUIData;

    void Start()
    {
        ClientPlayerUIData clientUIData;
        UpdateCurrentPlayerTurn();
        if (NetworkManager.Instance.isServer)
        {
            playerUIData = playerUIServerData;
            clientUIData = gameObject.AddComponent<ClientPlayerUIData>();
            clientUIData.uiToUpdate = playerUIClientData;
        }
        else
        {
            playerUIData = playerUIClientData;
            clientUIData = gameObject.AddComponent<ClientPlayerUIData>();
            clientUIData.uiToUpdate = playerUIServerData;
        }
        clientUIData.Init();
    }

    public void SetTimeText(string _timeText)
    {
        timeText.text = _timeText;
    }

    public void SetPlayerUIData(Tank tank)
    {
        playerUIData.playerHealt.text = "Lifes " + tank.lifes.ToString();
        playerUIData.bulletSpeed.text = "Bullet Speed " + tank.bulletVelocity.ToString("00.00");
        playerUIData.cannonAngle.text = "Cannon Angle " + Mathf.Abs(tank.AuxAngle).ToString("00.00");
        string dataBuffer = playerUIData.playerHealt.text + '?' + playerUIData.bulletSpeed.text + '?' + playerUIData.cannonAngle.text;
        MessageManager.Instance.SendPlayerUI(dataBuffer, ObjectsID.playerUIObjectID);
    }

    public void UpdateCurrentPlayerTurn()
    {
        /*currentPlayerTurn++;
        if (currentPlayerTurn > GameManager.instance.players.Length)
        {
            currentPlayerTurn = 1;
        }*/
        playerTurnText.text = "Player's " + currentPlayerTurn.ToString() + " turn";
    }

    public void OnGameOver()
    {
        gameOverScreen.gameObject.SetActive(true);
    }


}

