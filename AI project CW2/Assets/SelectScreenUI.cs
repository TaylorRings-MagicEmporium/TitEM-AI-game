using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectScreenUI : MonoBehaviour
{
    public List<Button> levelButtons = new List<Button>();

    public TMP_Text CurrentFloorNum;
    public TMP_Text CurrentCashNum;

    public void ShowLevelButtons(int floorNum, float difficultyScale, float difficultyScaleIncrease = 0.03f)
    {
        for(int i = 0; i < Mathf.Min(levelButtons.Count,floorNum); i++)
        {
            levelButtons[i].gameObject.SetActive(true);
            levelButtons[i].transform.GetComponentInChildren<TMP_Text>().text = "Floor " + (floorNum + i) + "\nDifficulty: " + (difficultyScale + difficultyScaleIncrease*(i+1));
        }
    }

    public void ResetButtons()
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            levelButtons[i].gameObject.SetActive(false);
        }
    }

    public void ShowCurrentStats(int floorNum, int cashAmount)
    {
        CurrentFloorNum.text = "Floor: " + floorNum.ToString();
        CurrentCashNum.text = "Cash: $" + cashAmount.ToString();
    }
}
