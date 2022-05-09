using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HighscoreWriter : MonoBehaviour
{
    [SerializeField]
    private string FileName;
    private void Start()
    {
        Debug.Log(Application.persistentDataPath);
    }
    public bool WriteToFile(List<ScoreStat> scoreList)
    {
        string json = "";
        for(int i = 0; i < scoreList.Count; i++)
        {
            json += JsonUtility.ToJson(scoreList[i]);
            if(i != scoreList.Count - 1)
            {
                json += "\n";
            }
        }

        Debug.Log(json);
        try
        {
            string path = Application.persistentDataPath + "/" + FileName + ".txt";
            File.WriteAllText(path, json);
            return true;
        }
        catch
        {
            Debug.LogError("Something went wrong writing the " + FileName + " txt file. check at " + Application.persistentDataPath);
            return false;
        }
    }
}
