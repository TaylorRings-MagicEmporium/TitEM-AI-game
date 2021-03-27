using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gamemanager : MonoBehaviour
{

    public Text treasureValues;
    public int TotalTreasureValue;

    public float PatrolToInvest = 45.0f;
    public float InvestToChase = 80.0f;

    public Text LevelState;
    // Start is called before the first frame update

    bool HasPlayerBegun = false;
    bool HasFloorBuilt = false;
    bool HasPlayerEscaped = false;
    bool HasPlayerCaptured = false;

    public enum Game_State {SELECT, READY, GAME, ESCAPED, CAPTURED };
    public Game_State Current_Game_State;

    int floor_num = 0;
    public Text floor_number_text;

    public MazeGenerator MG;
    public ScreenSwitcher SS;
    public GameObject Player;
    public Image PowerBar;

    void Start()
    {
        treasureValues.text = "treasures:\n$000";
        MG = GetComponent<MazeGenerator>();
        SS = GetComponent<ScreenSwitcher>();
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
    }

    public void AddTreasureValue(int amount)
    {
        Debug.Log(amount);
        TotalTreasureValue += amount;
        treasureValues.text = "treasures:\n$" + TotalTreasureValue.ToString();
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
            SS.ActivateScreen("result");
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
        TotalTreasureValue = 0;
        UpdatePlayerStatus(Game_State.SELECT);

    }
}
