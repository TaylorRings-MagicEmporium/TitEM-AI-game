using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class HighscoreManager : MonoBehaviour
{
    public int maxScoresRecorded = 5;

    public List<ScoreStat> scores = new List<ScoreStat>();

    public HighscoreReader reader;
    public HighscoreWriter writer;

    public TMP_Text scoreBoard;

    public void SetupHighscore()
    {
        for(int i = 0; i < maxScoresRecorded; i++)
        {
            scores.Add(new ScoreStat(0, 0));
        }
        var temp = reader.ReadFromFile();
        if(temp != null)
        {
            scores = temp;
        }
    }
    public void DisplayScores()
    {
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Highscores!");
        for(int i = 0; i < scores.Count; i++)
        {
            sb.Append(i + 1);
            sb.Append(":");
            sb.Append("$" + scores[i].cashAmount);
            sb.Append(" ");
            sb.Append("Floor ");
            sb.Append(scores[i].floorNum);
            sb.AppendLine();
        }
        scoreBoard.text = sb.ToString();
    }

    public void ReadScores()
    {
        scores = reader.ReadFromFile();
    }

    public void saveScores()
    {
        writer.WriteToFile(scores);
    }

    public void AddScore(int money,int floor)
    {
        for(int i = 0; i < scores.Count; i++)
        {
            if(money > scores[i].cashAmount)
            {
                scores.Insert(i, new ScoreStat(money, floor));
                scores.RemoveAt(scores.Count - 1);
                break;
            }
        }
    }

    public void ClearScores()
    {
        
    }
}
