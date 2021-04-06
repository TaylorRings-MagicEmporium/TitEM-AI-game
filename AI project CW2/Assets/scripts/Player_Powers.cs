using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Powers : MonoBehaviour
{
    public float currentPowerLevel = 0;
    public float MaxPowerLevel = 100;
    public float PowerDropMultiplyer = 5;

    public bool InvisibilityOn = false;
    public ParticleSystem ps;

    private void Start()
    {
        currentPowerLevel = MaxPowerLevel;
        //mat = GetComponent<Renderer>().material;
    }

    public void Reset_Level()
    {
        currentPowerLevel = MaxPowerLevel;
    }

    private void Update()
    {
        InvisibilityOn = false;
        if (Input.GetKey(KeyCode.Space))
        {
            if(currentPowerLevel > 0)
            {
                InvisibilityOn = true;
                currentPowerLevel -= Time.deltaTime * PowerDropMultiplyer;
            }
        }


        if (InvisibilityOn)
        {
            ps.Play();
        }
        else
        {
            ps.Clear();
            ps.Stop();

        }
    }

}
