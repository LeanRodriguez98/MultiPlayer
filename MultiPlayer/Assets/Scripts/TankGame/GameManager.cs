using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [System.Serializable]
    public struct Timer
    {
        public float minutes;
        public float seconds;
        public float miliseconds;
    }

    public static GameManager instance;
    public Timer turnTime;
    public Tank[] players;

    [HideInInspector] public bool gameOver = false;

    private Timer auxTimer;
    private UI_Canvas canvasInstance;
    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start () {
        auxTimer = turnTime;
       // players[0].SetTurn();
        canvasInstance = UI_Canvas.instance;
    }
	
	// Update is called once per frame
	void Update () {
        if (!gameOver)
        {
            canvasInstance.SetTimeText(UpdateTime());
            //canvasInstance.SetPlayerUIData(players);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Utilities.ReloadScene();
        }
    }

    private string UpdateTime()
    {
        if (turnTime.minutes < 0)
        {
            //OnEndTurn();
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
        /*for (int i = 0; i < players.Length; i++)
        {
            players[i].SetTurn();
        }
        turnTime = auxTimer;
        canvasInstance.UpdateCurrentPlayerTurn();*/
    }


    public void OnGameOver()
    {
        UI_Canvas.instance.OnGameOver();
        gameOver = true;
    }

    public Tank GetEnemyTank()
    {
        foreach (Tank tank in players)
        {
            if (!tank.isYourTurn)
            {
                return tank;
            }
        }
        return null;
    }

}
