using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Grate : MonoBehaviour
{

    gamemanager gm;
    //bool Entering = true;
    bool PlayerOnGrate = false;
    bool StillGoing = false;

    public Image lock_image;

    Animator ani_grate;
    public Animator ani_player;
   // Animation ani_clip;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("Manager").GetComponent<gamemanager>();
        ani_grate = GetComponent<Animator>();
        lock_image.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && gm.Current_Game_State == gamemanager.Game_State.READY && !StillGoing)
        {
            //play entering animation
            StartCoroutine(Animation_entering_play());
        }
        else if (Input.GetKeyDown(KeyCode.Space) && gm.Current_Game_State == gamemanager.Game_State.GAME && PlayerOnGrate && gm.ExitCondition && !StillGoing)
        {
            //play exit animation
            StartCoroutine(Animation_exiting_play());
        }
        else if(Input.GetKeyDown(KeyCode.Space) && gm.Current_Game_State == gamemanager.Game_State.GAME && PlayerOnGrate)
        {
            ani_grate.SetTrigger("no entry");
        }

        if (gm.ExitCondition)
        {
            lock_image.enabled = false;
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
        StillGoing = true;
        ani_grate.SetTrigger("changeState");
        ani_player.SetTrigger("Do_Jump");
        yield return new WaitForSeconds(1);
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player_Transition>().Player_Enabled();
        GameObject.FindGameObjectWithTag("Manager").GetComponent<gamemanager>().UpdatePlayerStatus(gamemanager.Game_State.GAME);
        lock_image.enabled = true;
        StillGoing = false;
    }

    IEnumerator Animation_exiting_play()
    {
        StillGoing = true;
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player_Transition>().Player_Disabled();
        ani_player.SetTrigger("Do_Jump");
        ani_grate.SetTrigger("changeState");
        lock_image.enabled = false;
        yield return new WaitForSeconds(1);
        GameObject.FindGameObjectWithTag("Manager").GetComponent<gamemanager>().UpdatePlayerStatus(gamemanager.Game_State.ESCAPED);

        StillGoing = false;
    }
}

