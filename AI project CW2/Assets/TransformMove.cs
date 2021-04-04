using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformMove : MonoBehaviour
{ 
    public Rigidbody rb;

    public Animator ani;

    public float AccMultiplyer = 1;
    public float VelocityLimit = 1;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //ani = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Vector3 tempForce = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            tempForce += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            tempForce += new Vector3(-1, 0, 0);

        }
        if (Input.GetKey(KeyCode.S))
        {
            tempForce += new Vector3(0, 0, -1);

        }
        if (Input.GetKey(KeyCode.D))
        {
            tempForce += new Vector3(1, 0, 0);

        }


        if(tempForce == Vector3.zero)
        {
            rb.AddForce(-rb.velocity * AccMultiplyer);
            ani.SetBool("IsMoving", false);
        }
        else
        {
            if(rb.velocity.magnitude <= VelocityLimit)
            {
                rb.AddForce(tempForce * AccMultiplyer);
                if(rb.velocity != Vector3.zero)
                {
                    ani.transform.rotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
                }

                ani.SetBool("IsMoving", true);
            }
        }
    }
}
