using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Canvas : MonoBehaviour {

    [System.Serializable]
    public struct PlayerUIData
    {
        public Text playerHealt;
        public Text bulletSpeed;
        public Text cannonAngle;
    }


    public static UI_Canvas instance;
    public Text timeText;
    public Text playerTurnText;

    public PlayerUIData[] playersUIData;
    public GameObject gameOverScreen;
    private int currentPlayerTurn = 0;
    private void Awake()
    {
        instance = this;
    }

    void Start ()
    {
        UpdateCurrentPlayerTurn();

    }

    public void SetTimeText(string _timeText)
    {
        timeText.text = _timeText;
    }

    public void SetPlayerUIData(Tank[] tanks)
    {
        for (int i = 0; i < playersUIData.Length; i++)
        {
            playersUIData[i].playerHealt.text = "Lifes " + tanks[i].lifes.ToString();
            playersUIData[i].bulletSpeed.text = "Bullet Speed " + tanks[i].bulletVelocity.ToString("00.00");
            playersUIData[i].cannonAngle.text = "Cannon Angle " + Mathf.Abs(tanks[i].AuxAngle).ToString("00.00");
        }
    }

    public void UpdateCurrentPlayerTurn()
    {
        currentPlayerTurn++;
        if (currentPlayerTurn > GameManager.instance.players.Length)
        {
            currentPlayerTurn = 1;
        }
        playerTurnText.text = "Player's " + currentPlayerTurn.ToString() + " turn";
    }

    public void OnGameOver()
    {
        gameOverScreen.gameObject.SetActive(true);
        
    }
}

