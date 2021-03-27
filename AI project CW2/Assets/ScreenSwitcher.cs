using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSwitcher : MonoBehaviour
{
    Dictionary<string, CanvasGroup> AllScreens = new Dictionary<string, CanvasGroup>();

    string prevScreen = "";
    //string currScreen = "";

    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Screen"))
        {
            AllScreens[g.GetComponent<ScreenSwitchObject>().screenName] = g.GetComponent<CanvasGroup>();
            AllScreens[g.GetComponent<ScreenSwitchObject>().screenName].alpha = 0;
            AllScreens[g.GetComponent<ScreenSwitchObject>().screenName].interactable = false;
            AllScreens[g.GetComponent<ScreenSwitchObject>().screenName].blocksRaycasts = false;
        }
    }

    public void ActivateScreen(string n)
    {
        Debug.Log(n);
        Debug.Log(AllScreens.Count);
        if(prevScreen != "")
        {
            AllScreens[prevScreen].alpha = 0;
            AllScreens[prevScreen].interactable = false;
            AllScreens[prevScreen].blocksRaycasts = false;
        }
        AllScreens[n].alpha = 1;
        AllScreens[n].interactable = true;
        AllScreens[n].blocksRaycasts = true;
        prevScreen = n;
    }
}
