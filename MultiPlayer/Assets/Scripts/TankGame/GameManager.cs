using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MBSingleton<GameManager> {

    [System.Serializable]
    public struct Timer
    {
        public float minutes;
        public float seconds;
        public float miliseconds;
    }

    public Timer turnTime;
    private Tank player;

    [HideInInspector] public bool gameOver = false;

    private Timer auxTimer;

    public void SetLocalPlayer(Tank _player)
    {
        player = _player;
    }

    // Use this for initialization
    void Start () {
        auxTimer = turnTime;
        if (NetworkManager.Instance.isServer)
        {
            player.SetTurn();
        }
        player.SetTurn();
    }
	
	// Update is called once per frame
	void Update () {
        if (!gameOver)
        {
            UI_Canvas.Instance.SetTimeText(UpdateTime());
            UI_Canvas.Instance.SetPlayerUIData(player);
        }
    }

    private string UpdateTime()
    {
        if (turnTime.minutes < 0)
        {
            OnEndTurn();
            return "0:00:00";
        }
        if (turnTime.miliseconds <= 0)
        {
            if (turnTime.seconds <= 0)
            {
                turnTime.minutes--;
                turnTime.seconds = 59;
            }
            else if (turnTime.seconds >= 0)
            {
                turnTime.seconds--;
            }

            turnTime.miliseconds = 100;
        }

        turnTime.miliseconds -= Time.deltaTime * 100;
        if (turnTime.miliseconds < 0)
            turnTime.miliseconds = 0;
        return string.Format("{0}:{1}:{2}", turnTime.minutes.ToString("0"), turnTime.seconds.ToString("00"), ((int)(turnTime.miliseconds)).ToString("00"));
    }

    public void OnEndTurn()
    {
        player.SetTurn();
        turnTime = auxTimer;
        UI_Canvas.Instance.UpdateCurrentPlayerTurn();
    }

    public void OnGameOver()
    {
        UI_Canvas.Instance.OnGameOver();
        gameOver = true;
    }

}
