﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gamemanager : MonoBehaviour
{
    public float PatrolToInvest = 45.0f;
    public float InvestToChase = 80.0f;

    public Text LevelState;

    bool HasFloorBuilt = false;
    bool HasPlayerCaptured = false;
    public bool HasPlayerBeenChased = false;
    int treasureCount = 0;

    public bool ExitCondition = false;

    public enum Game_State {SELECT, READY, GAME, ESCAPED, CAPTURED };
    public Game_State Current_Game_State;

    int floor_num = 0;
    float difficultyScale = 1.0f;
    public Text floor_number_text;

    public MazeGenerator MG;
    public ScreenSwitcher SS;
    public CashManager CM;

    public GameObject Player;
    public Image PowerBar;

    void Start()
    {
        MG = GetComponent<MazeGenerator>();
        SS = GetComponent<ScreenSwitcher>();
        SS.Self_Start();
        CM = GetComponent<CashManager>();
        StartCoroutine(CheckTotalSusLevel());
        UpdatePlayerStatus(Game_State.SELECT);
        Player = GameObject.FindGameObjectWithTag("Player");

    }

    // Update is called once per frame
    void Update()
    {
        if(Current_Game_State == Game_State.GAME)
        {
            PowerBar.fillAmount = (float)Player.GetComponent<Player_Powers>().currentPowerLevel / (float)Player.GetComponent<Player_Powers>().MaxPowerLevel;
        }

        if (!ExitCondition && Current_Game_State == Game_State.GAME)
        {
            CheckExitCondition();
        }
    }

    public void AddTreasureValue(string tresName, int amount)
    { 
        CM.AddMoney(tresName, amount);
        treasureCount++;
    }

    IEnumerator CheckTotalSusLevel()
    {
        while (true)
        {
            float HighestLevel = -1;

            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Guard"))
            {
                Guard gg = g.GetComponent<Guard>();
                if (gg.GuardSuspisionLevel > HighestLevel)
                {
                    HighestLevel = gg.GuardSuspisionLevel;
                }
                if(gg.currentStates == Guard.BehaviourStates.CHASE)
                {
                    HasPlayerBeenChased = true;
                }
            }

            if (HighestLevel > 80.0f)
            {
                LevelState.text = "criminal!";

            }
            else if (HighestLevel > 50.0f)
            {
                LevelState.text = "suspision";

            }
            else if (HighestLevel > 25.0f)
            {
                LevelState.text = "slight";

            }
            else
            {
                LevelState.text = "clear";
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void TreasureTakenRiseAlerts()
    {
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Guard"))
        {
            g.GetComponent<Guard>().TreasureStolenAlert();
        }
    }

    public void UpdateFloorStatus(bool state)
    {
        HasFloorBuilt = state;
    }

    public void UpdatePlayerStatus(Game_State state)
    {
        Current_Game_State = state;
        if(Current_Game_State == Game_State.ESCAPED) //result
        {
            EndLevelConditions();
            SS.ActivateScreen("result");
            CM.DisplayMoneySummary();
            treasureCount = 0;
            ExitCondition = false;
            HasPlayerBeenChased = false;
            floor_num += 1;

        } else if(Current_Game_State == Game_State.GAME) // game
        {
            SS.ActivateScreen("game");

        }
        else if(Current_Game_State == Game_State.READY) // ready
        {
            SS.ActivateScreen("ready");

        }
        else if(Current_Game_State == Game_State.SELECT) //select
        {
            CM.FinishSummary();
            SS.ActivateScreen("select");

            floor_number_text.text = "Floor " + floor_num;
        } else if(Current_Game_State == Game_State.CAPTURED)// over
        {
            SS.ActivateScreen("over");
        }
    }

    public void Start_Level()
    {
        UpdatePlayerStatus(Game_State.READY);
        treasureCount = 0;
        MG.MinTreasureAmount = (int)(30.0f * difficultyScale);
        MG.MaxTreasureAmount = (int)(60.0f * difficultyScale);
        int numOfRooms = (int)(10.0f * difficultyScale * 1.2);
        if(numOfRooms > (MG.GridSizeX * MG.GridSizeY)-7)
        {
            numOfRooms = (MG.GridSizeX * MG.GridSizeY) - 7;
        }
        MG.RoomsInFloor = numOfRooms;
        MG.Create_Floor_Level();
        Player.GetComponent<Player_Powers>().Reset_Level();
    }

    public void Select_Level()
    {
        floor_number_text.text = "Floor " + floor_num.ToString();
        UpdatePlayerStatus(Game_State.SELECT);

    }

    public void Game_Over_Called()
    {
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Guard"))
        {
            g.GetComponent<Guard>().DisableGuard = true;

        }
        HasPlayerCaptured = true;
        UpdatePlayerStatus(Game_State.CAPTURED);
        Player.GetComponent<Player_Transition>().Player_Disabled();
    }

    public void Reset_Game()
    {
        floor_num = 0;
        treasureCount = 0;
        UpdatePlayerStatus(Game_State.SELECT);

    }

    public void EndLevelConditions()
    {
        if(Player.GetComponent<Player_Powers>().currentPowerLevel > Player.GetComponent<Player_Powers>().MaxPowerLevel * 0.9f)
        {
            CM.AddMoney("Little power used!", (int)(70 * difficultyScale));
        }
        if(treasureCount == MG.TreasureRooms)
        {
            CM.AddMoney("Collect All Treasure!", (int)(40 * difficultyScale));
        }
        if (!HasPlayerBeenChased)
        {
            Debug.Log("called");
            CM.AddMoney("Haven't Been Chased!", (int)(55 * difficultyScale));
        }
    }

    public void CheckExitCondition()
    {
        if(treasureCount >= 1)
        {
            ExitCondition = true;
        }
    }
}
