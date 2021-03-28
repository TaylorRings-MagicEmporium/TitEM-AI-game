using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grate : MonoBehaviour
{

    gamemanager gm;
    //bool Entering = true;
    bool PlayerOnGrate = false;

    Animator ani;
   // Animation ani_clip;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("Manager").GetComponent<gamemanager>();
        ani = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && gm.Current_Game_State == gamemanager.Game_State.READY)
        {
            //play entering animation
            StartCoroutine(Animation_entering_play());
        }
        else if (Input.GetKeyDown(KeyCode.Space) && gm.Current_Game_State == gamemanager.Game_State.GAME && PlayerOnGrate && gm.ExitCondition)
        {
            //play exit animation
            StartCoroutine(Animation_exiting_play());
        }
        else if(Input.GetKeyDown(KeyCode.Space) && gm.Current_Game_State == gamemanager.Game_State.GAME && PlayerOnGrate)
        {
            Debug.Log("Collect one more treasure to exit!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerOnGrate = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerOnGrate = false;
        }
    }

    IEnumerator Animation_entering_play()
    {
        //GameObject.FindGameObjectWithTag("Player").GetComponent<Player_Transition>().Player_Disabled();
        ani.SetTrigger("changeState");
        yield return new WaitForSeconds(1);
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player_Transition>().Player_Enabled();
        GameObject.FindGameObjectWithTag("Manager").GetComponent<gamemanager>().UpdatePlayerStatus(gamemanager.Game_State.GAME);
    }

    IEnumerator Animation_exiting_play()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player_Transition>().Player_Disabled();

        ani.SetTrigger("changeState");
        yield return new WaitForSeconds(1);
        GameObject.FindGameObjectWithTag("Manager").GetComponent<gamemanager>().UpdatePlayerStatus(gamemanager.Game_State.ESCAPED);

    }
}

