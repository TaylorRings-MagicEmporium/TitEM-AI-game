using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gamemanager : MonoBehaviour
{
    public float PatrolToInvest = 45.0f;
    public float InvestToChase = 80.0f;

    public Text LevelState;

    //bool HasFloorBuilt = false;
    //bool HasPlayerCaptured = false;
    public bool HasPlayerBeenChased = false;
    int treasureCount = 0;

    public bool ExitCondition = false;

    public enum Game_State {SELECT, READY, GAME, ESCAPED, CAPTURED };
    public Game_State Current_Game_State;

    int floor_num = 0;
    public float difficultyScale = 1.0f;
    public Text floor_number_text;

    public MazeGenerator MG;
    public ScreenSwitcher SS;
    public CashManager CM;

    public GameObject Player;
    public Image PowerBar;

    bool FirstLevelShow = true;
    bool SecondLevelShow = false;
    bool ThirdLevelShow = false;

    public Text FloorLevel1;
    public Text FloorLevel2;
    public Text FloorLevel3;

    public Text GameOver_Floor;

    public AudioSource AS;

    void Start()
    {
        AS = GetComponent<AudioSource>();
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
        //HasFloorBuilt = state;
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

            if(FirstLevelShow && SecondLevelShow)
            {
                ThirdLevelShow = true;
            } else if (FirstLevelShow)
            {
                SecondLevelShow = true;
            }

            //floor_num += 1;

        } else if(Current_Game_State == Game_State.GAME) // game
        {

            SS.ActivateScreen("game");

        }
        else if(Current_Game_State == Game_State.READY) // ready
        {
            AS.Play();
            SS.ActivateScreen("ready");

        }
        else if(Current_Game_State == Game_State.SELECT) //select
        {
            AS.Stop();
            CM.FinishSummary();
            MG.Reset_Floor_Level();
            SS.ActivateScreen("select");

            if(ThirdLevelShow && SecondLevelShow && FirstLevelShow)
            {
                FloorLevel3.transform.parent.gameObject.SetActive(true);
                FloorLevel3.text = "Floor " + (floor_num + 2) + "\n\nDifficulty:\n" + (difficultyScale + 0.09f);
                FloorLevel2.text = "Floor " + (floor_num + 1) + "\n\nDifficulty:\n" + (difficultyScale + 0.06f);
                FloorLevel1.text = "Floor " + floor_num + "\n\nDifficulty:\n" + (difficultyScale + 0.03f);
            } else if(SecondLevelShow && FirstLevelShow){
                FloorLevel2.transform.parent.gameObject.SetActive(true);
                FloorLevel2.text = "Floor " + (floor_num + 1) + "\n\nDifficulty:\n" + (difficultyScale + 0.06f);
                FloorLevel1.text = "Floor " + floor_num + "\n\nDifficulty:\n" + (difficultyScale + 0.03f);
            }
            else
            {
                //Debug.Log("pear: " + difficultyScale);
                FloorLevel1.text = "Floor " + floor_num + "\n\nDifficulty:\n" + (difficultyScale + 0.03f);
                FloorLevel2.transform.parent.gameObject.SetActive(false);
                FloorLevel3.transform.parent.gameObject.SetActive(false);
            }

            //floor_number_text.text = "Floor " + floor_num;
        } else if(Current_Game_State == Game_State.CAPTURED)// over
        {
            SS.ActivateScreen("over");
            GameOver_Floor.text = "Floor Captured:\n" + floor_num;
            CM.DisplayFinalValue();
        }
    }

    public void Select_Level()
    {
        //floor_number_text.text = "Floor " + floor_num.ToString();
        UpdatePlayerStatus(Game_State.SELECT);

    }

    public void Game_Over_Called()
    {
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Guard"))
        {
            g.GetComponent<Guard>().DisableGuard = true;

        }
        //HasPlayerCaptured = true;
        UpdatePlayerStatus(Game_State.CAPTURED);
        Player.GetComponent<Player_Transition>().Player_Disabled();
    }

    public void Reset_Game()
    {
        floor_num = 0;
        treasureCount = 0;
        CM.Reset_Game();
        MG.Reset_Floor_Level();

        SecondLevelShow = false;
        ThirdLevelShow = false;
        FloorLevel1.transform.parent.gameObject.SetActive(true);
        FloorLevel2.transform.parent.gameObject.SetActive(false);
        FloorLevel3.transform.parent.gameObject.SetActive(false);
        difficultyScale = 1;
        UpdatePlayerStatus(Game_State.SELECT);

        //Debug.Log("apple: " + difficultyScale);
    }

    public void AddDifficulty(float value)
    {
        if(value == 0.09f)
        {
            floor_num += 3;
        } else if(value == 0.06f)
        {
            floor_num += 2;
        }
        else
        {
            floor_num++;
        }

        difficultyScale += value;
        UpdatePlayerStatus(Game_State.READY);
        treasureCount = 0;
        MG.MinTreasureAmount = (int)(20.0f * difficultyScale);
        MG.MaxTreasureAmount = (int)(50.0f * difficultyScale);
        int numOfRooms = (int)(10.0f * difficultyScale * 1.5);
        if (numOfRooms > (MG.GridSizeX * MG.GridSizeY) - 6)
        {
            numOfRooms = (MG.GridSizeX * MG.GridSizeY) - 6;
        }
        if(MG.MinTreasureAmount > 450)
        {
            MG.MinTreasureAmount = 450;
        }
        if(MG.MaxTreasureAmount > 500)
        {
            MG.MaxTreasureAmount = 500;
        }
        MG.RoomsInFloor = numOfRooms;

        int temp = Mathf.FloorToInt((difficultyScale - 1) / 0.12f);
        if(temp > 10)
        {
            temp = 10;
        }
        MG.GuardsToWalk = temp;
        //Debug.Log("hi " + MG.GuardsToWalk);


        MG.Create_Floor_Level();
        Player.GetComponent<Player_Powers>().Reset_Level();
    }

    public void EndLevelConditions()
    {
        if(Player.GetComponent<Player_Powers>().currentPowerLevel > Player.GetComponent<Player_Powers>().MaxPowerLevel * 0.9f)
        {
            CM.AddMoney("Little power used!", (int)(25 * difficultyScale));
        }
        if(treasureCount == MG.TreasureRooms)
        {
            CM.AddMoney("Collect All Treasure!", (int)(40 * difficultyScale));
        }
        if (!HasPlayerBeenChased)
        {
            Debug.Log("called");
            CM.AddMoney("Haven't Been Chased!", (int)(50 * difficultyScale));
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
