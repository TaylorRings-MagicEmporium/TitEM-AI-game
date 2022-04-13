using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject PausePanel;

    public bool IsPaused;

    private void Start()
    {
        PausePanel.SetActive(false);
    }

    public void PauseEnabled()
    {
        Time.timeScale = 0;
        PausePanel.SetActive(true);
    }

    public void PauseDisabled()
    {
        Time.timeScale = 1;
        PausePanel.SetActive(false);
    }

    public void TogglePause(bool state)
    {
        if(IsPaused != state)
        {
            IsPaused = state;
            if (state) PauseEnabled(); else PauseDisabled();
        }
    }

}
