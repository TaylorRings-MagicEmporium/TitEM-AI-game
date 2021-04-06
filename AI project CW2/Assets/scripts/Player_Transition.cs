using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Transition : MonoBehaviour
{
    public GameObject Model;

    public void Player_Disabled()
    {
        Model.SetActive(false);
        GetComponent<TransformMove>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<ItemCollecter>().enabled = false;
        GetComponent<Player_Powers>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    public void Player_Enabled()
    {
        Model.SetActive(true);
        GetComponent<TransformMove>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<ItemCollecter>().enabled = true;
        GetComponent<Player_Powers>().enabled = true;
        GetComponent<Collider>().enabled = true;


    }
}
