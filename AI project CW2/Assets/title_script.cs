using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class title_script : MonoBehaviour
{

    Animator ani;
    public GameObject instruct_panel;

    private void Start()
    {
        instruct_panel.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        ani = GetComponent<Animator>();
    }

    public void Begin_Game()
    {
        StartCoroutine(Game_procedure());
    }

    IEnumerator Game_procedure()
    {
        ani.SetTrigger("continue");
        yield return new WaitForSeconds(4f);
        Debug.Log("done!");
        SceneManager.LoadScene("game_scene");
    }

    public void Get_Instructions()
    {
        instruct_panel.SetActive(true);
    }

    public void Get_Back()
    {
        instruct_panel.SetActive(false);
    }
}
