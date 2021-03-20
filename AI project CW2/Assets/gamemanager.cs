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

    public enum Game_State {READY, GAME, ESCAPED, CAPTURED };
    public Game_State Current_Game_State;

    public GameObject ready_screen;
    public GameObject game_screen;
    public GameObject result_screen;

    void Start()
    {
        treasureValues.text = "treasures:\n$000";
        StartCoroutine(CheckTotalSusLevel());
        UpdatePlayerStatus(Game_State.READY);
        Current_Game_State = Game_State.READY;

    }

    // Update is called once per frame
    void Update()
    {
        
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
        if(Current_Game_State == Game_State.ESCAPED)
        {
            ready_screen.SetActive(false);
            result_screen.SetActive(true);
            game_screen.SetActive(false);
        } else if(Current_Game_State == Game_State.GAME)
        {
            ready_screen.SetActive(false);
            result_screen.SetActive(false);
            game_screen.SetActive(true);
        } else if(Current_Game_State == Game_State.READY)
        {
            ready_screen.SetActive(true);
            result_screen.SetActive(false);
            game_screen.SetActive(false);
        }
    }
}
