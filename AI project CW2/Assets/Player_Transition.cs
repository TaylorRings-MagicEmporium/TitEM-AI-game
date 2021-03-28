using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Transition : MonoBehaviour
{
    public void Player_Disabled()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<TransformMove>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<ItemCollecter>().enabled = false;
        GetComponent<Player_Powers>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    public void Player_Enabled()
    {
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<TransformMove>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<ItemCollecter>().enabled = true;
        GetComponent<Player_Powers>().enabled = true;
        GetComponent<Collider>().enabled = true;


    }
}
