using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gamemanager : MonoBehaviour
{
    // guard level regions which can depict the behaviour
    public float PatrolToInvest = 45.0f;
    public float InvestToChase = 80.0f;

    // visual description of the highest behaviour level
    public Text LevelState;

    // achievement condition if the player never made a guard chase them
    public bool HasPlayerBeenChased = false;

    // achievement condition if the player captures all of the treasures
    int treasureCount = 0;

    // have the current floor's exit conditions been met?
    public bool ExitCondition = false;

    // game states depict what the game is currently doing.
    public enum Game_State {SELECT, READY, GAME, ESCAPED, CAPTURED };
    public Game_State Current_Game_State;

    // the number of floors the player has passed
    int floor_num = 0;
    
    // the difficultyScale depicts how difficult the game should be
    public float difficultyScale = 1.0f;

    public MazeGenerator MG; // used for generating floors
    public ScreenSwitcher SS; // used to switch screens easily
    public CashManager CM; // used to manage the result screen and how the cash is collected

    public GameObject Player; // the player object
    public Image PowerBar; // power bar to show the power level

    bool FirstLevelShow = true; // show the first floor option?
    bool SecondLevelShow = false; // show the second floor option?
    bool ThirdLevelShow = false; // show the third floor option?

    public Text FloorLevel1; // text representation of level 1 showing
    public Text FloorLevel2; // ""
    public Text FloorLevel3; // ""

    public Text GameOver_Floor; // tells how many floors the player got passed

    public AudioSource AS; // overall audio system

    // plays at the beginning of the scene.
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
        if(Current_Game_State == Game_State.GAME) // updates the power bar
        {
            PowerBar.fillAmount = (float)Player.GetComponent<Player_Powers>().currentPowerLevel / (float)Player.GetComponent<Player_Powers>().MaxPowerLevel;
        }

        if (!ExitCondition && Current_Game_State == Game_State.GAME) // if the exit condition hasn't been changed, check again
        {
            CheckExitCondition();
        }
    }

    // provides the cash manager with the treasure name and amount
    public void AddTreasureValue(string tresName, int amount)
    { 
        CM.AddMoney(tresName, amount);
        treasureCount++;
    }

    // finds the highest guard suspicion level on the floor
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

    // button function for player to go back to select screen
    public void Select_Level()
    {
        //floor_number_text.text = "Floor " + floor_num.ToString();
        UpdatePlayerStatus(Game_State.SELECT);

    }

    // if a guard spotted a treasure missing, then all guards are alerted of this disappearence
    public void TreasureTakenRiseAlerts()
    {
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Guard"))
        {
            g.GetComponent<Guard>().TreasureStolenAlert();
        }
    }

    // this function changes how the game is played currently and is able to set screens up
    public void UpdatePlayerStatus(Game_State state)
    {
        Current_Game_State = state;
        if(Current_Game_State == Game_State.ESCAPED) //result screen
        {
            Debug.Log("PLAY STATE TO ESCAPED");
            EndLevelConditions();
            SS.ActivateScreen("result");
            CM.DisplayMoneySummary();
            treasureCount = 0;
            ExitCondition = false;
            HasPlayerBeenChased = false;

            if(FirstLevelShow && SecondLevelShow) // gradually release more floor options to the player
            {
                ThirdLevelShow = true;
            } else if (FirstLevelShow)
            {
                SecondLevelShow = true;
            }

        } else if(Current_Game_State == Game_State.GAME) // game screen
        {
            Debug.Log("PLAY STATE TO GAME");
            SS.ActivateScreen("game");

        }
        else if(Current_Game_State == Game_State.READY) // ready screen
        {
            Debug.Log("PLAY STATE TO READY");
            AS.Play();
            SS.ActivateScreen("ready");

        }
        else if(Current_Game_State == Game_State.SELECT) //select screen
        {
            Debug.Log("PLAY STATE TO SELECT");
            AS.Stop();
            CM.FinishSummary();
            MG.Reset_Floor_Level();
            SS.ActivateScreen("select");

            // the if statements check that the right floor options are shown
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
                FloorLevel1.text = "Floor " + floor_num + "\n\nDifficulty:\n" + (difficultyScale + 0.03f);
                FloorLevel2.transform.parent.gameObject.SetActive(false);
                FloorLevel3.transform.parent.gameObject.SetActive(false);
            }

        } else if(Current_Game_State == Game_State.CAPTURED)// over
        {
            Debug.Log("PLAY STATE TO CAPTURED");
            SS.ActivateScreen("over");
            GameOver_Floor.text = "Floor Captured:\n" + floor_num;
            CM.DisplayFinalValue();
        }
    }

    // if the player has been captured that tell the game-manager
    public void Game_Over_Called()
    {
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Guard")) // tell all guards to disable their behaviours
        {
            g.GetComponent<Guard>().DisableGuard = true;

        }
        UpdatePlayerStatus(Game_State.CAPTURED);
        Player.GetComponent<Player_Transition>().Player_Disabled(); // disable the player by removing controls
    }

    // resets the whole game for another run
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
    }

    // based on the floor option chosen, the difficultly scale increases and assigns new values for the maze generator
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


        MG.Create_Floor_Level();
        Player.GetComponent<Player_Powers>().Reset_Level();
    }

    // if the player escapes, then the end level conditions are checked
    public void EndLevelConditions()
    {
        if(Player.GetComponent<Player_Powers>().currentPowerLevel > Player.GetComponent<Player_Powers>().MaxPowerLevel * 0.9f)
        {
            CM.AddMoney("Little power used!", (int)(25 * difficultyScale)); // if only 10% of power was used, then they earn this bonus
        }
        if(treasureCount == MG.TreasureRooms)
        {
            CM.AddMoney("Collect All Treasure!", (int)(40 * difficultyScale)); // if they collect all of the treasure on the floor, then they earn this bonus
        }
        if (!HasPlayerBeenChased)
        {
            CM.AddMoney("Haven't Been Chased!", (int)(50 * difficultyScale)); // if the player has not been chased on the floor, then they earn this bonus.
        }
    }

    // checks if the player has reached the conditions to escape
    public void CheckExitCondition()
    {
        if(treasureCount >= 1) // the player can only connect if they collect one treasure
        {
            ExitCondition = true;
        }
    }
}
