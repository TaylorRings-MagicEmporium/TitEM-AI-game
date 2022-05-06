using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   

public class SelectScreenUI : MonoBehaviour
{
    public List<Button> levelButtons = new List<Button>();

    public void ShowLevelButtons(int floorNum, float difficultyScale, float difficultyScaleIncrease = 0.03f)
    {
        for(int i = 0; i < Mathf.Min(levelButtons.Count,floorNum); i++)
        {
            levelButtons[i].gameObject.SetActive(true);
            levelButtons[i].transform.GetComponentInChildren<Text>().text = "Floor " + (floorNum + i) + "\n\nDifficulty:\n" + (difficultyScale + difficultyScaleIncrease*(i+1));
        }
    }

    public void ResetButtons()
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            levelButtons[i].gameObject.SetActive(false);
        }
    }
}
