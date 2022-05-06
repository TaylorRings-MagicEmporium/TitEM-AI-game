using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gamemanager : MonoBehaviour
{
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
    int floor_num = 1;
    
    // the difficultyScale depicts how difficult the game should be
    public float difficultyScale = 1.0f;

    public MazeGenerator MazeGen; // used for generating floors
    public TreasureRoomGenerator TreasureGen;
    public GuardGenerator GuardGen;
    public NavMeshBuilder navMeshGen;
    public ScreenSwitcher SS; // used to switch screens easily
    public CashManager CM; // used to manage the result screen and how the cash is collected

    public GameObject Player; // the player object
    public Image PowerBar; // power bar to show the power level

    public Text GameOver_Floor; // tells how many floors the player got passed

    public AudioSource AS; // overall audio system

    public SelectScreenUI selectScreenUI;

    // plays at the beginning of the scene.
    void Start()
    {
        MazeGen = GetComponent<MazeGenerator>();
        TreasureGen = GetComponent<TreasureRoomGenerator>();
        GuardGen = GetComponent<GuardGenerator>();
        AS = GetComponent<AudioSource>();
        SS = GetComponent<ScreenSwitcher>();
        CM = GetComponent<CashManager>();


        SS.Self_Start();
        selectScreenUI.ResetButtons();
        selectScreenUI.ShowLevelButtons(floor_num, difficultyScale, 0.03f);
        StartCoroutine(CheckTotalSusLevel());
        UpdatePlayerStatus(Game_State.SELECT);
        Player = GameObject.FindGameObjectWithTag("Player");

    }

    public void SetupMaze()
    {
        MazeGen.PlacePlayer();
        MazeGen.Create_Floor_Level();
        navMeshGen.UpdateNavMesh();
        TreasureGen.AddTreasureRooms();
        GuardGen.PlaceGuards();
    }

    public void ResetMaze()
    {
        MazeGen.Reset_Floor_Level();
        TreasureGen.ResetTreasureRooms();
        GuardGen.ResetGuards();
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
                if (gg.GuardSuspicionLevel > HighestLevel)
                {
                    HighestLevel = gg.GuardSuspicionLevel;
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
                LevelState.text = "suspicious!";

            }
            else if (HighestLevel > 25.0f)
            {
                LevelState.text = "noticed!";

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
            ResetMaze();
            SS.ActivateScreen("select");

            selectScreenUI.ShowLevelButtons(floor_num, difficultyScale, 0.03f);

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
            g.GetComponent<Guard>().IsDisableGuard = true;

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
        ResetMaze();
        selectScreenUI.ResetButtons();
        difficultyScale = 1;
        UpdatePlayerStatus(Game_State.SELECT);
    }

    // based on the floor option chosen, the difficultly scale increases and assigns new values for the maze generator
    public void AddDifficulty(float value)
    {
        if (value == 0.09f)
        {
            floor_num += 3;
        }
        else if (value == 0.06f)
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

        int numOfRooms = (int)(10.0f * difficultyScale * 1.5);
        if (numOfRooms > (MazeGen.GridSizeX * MazeGen.GridSizeY) - 6)
        {
            numOfRooms = (MazeGen.GridSizeX * MazeGen.GridSizeY) - 6;
        }

        MazeGen.RoomsInFloor = numOfRooms;

        int temp = Mathf.FloorToInt((difficultyScale - 1) / 0.12f);
        if (temp > 10)
        {
            temp = 10;
        }
        GuardGen.GuardsToWalk = temp;

        TreasureGen.SetTreasureRange(difficultyScale);

        SetupMaze();
        Player.GetComponent<Player_Powers>().Reset_Level();
    }

     //if the player escapes, then the end level conditions are checked
    public void EndLevelConditions()
    {
        if (Player.GetComponent<Player_Powers>().currentPowerLevel > Player.GetComponent<Player_Powers>().MaxPowerLevel * 0.9f)
        {
            CM.AddMoney("Little power used!", (int)(25 * difficultyScale)); // if only 10% of power was used, then they earn this bonus
        }
        if (treasureCount == TreasureGen.ActiveTreasureRooms())
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
