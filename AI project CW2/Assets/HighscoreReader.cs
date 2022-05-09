using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HighscoreReader : MonoBehaviour
{
    [SerializeField]
    private string FileName;

    public List<ScoreStat> ReadFromFile()
    {
        List<ScoreStat> list = new List<ScoreStat>();
        string path = Application.persistentDataPath +"/" + FileName + ".txt";
        try
        {

            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                while((line = sr.ReadLine()) != null)
                {
                    var temp = JsonUtility.FromJson<ScoreStat>(line);
                    list.Add(temp);
                }
            }
            return list;
        }
        catch
        {
            Debug.LogWarning("Could not read from file located at: " + path +". Returning nothing");
            return null;
        }
    }
}
