using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
public class Guard : MonoBehaviour
{
    public bool Stand = false;
    public bool Waypoint = false;

    protected NavMeshAgent agent;
    protected GameObject Player;

    protected bool turning = false;
    protected Quaternion current;
    protected Quaternion target;
    protected float rotateCounter = 0;

    public float GuardSuspisionLevel = 0.0f;

    public float AngleLimit = 45.0f;
    public float val;

    Vector3 Canvas_diff;

    public GameObject Suspision_Meter;
    public void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        Player = GameObject.FindGameObjectWithTag("Player");
        Suspision_Meter = GetComponent<Access_Points>().Suspision_Meter;

        Canvas_diff = GetComponent<Access_Points>().canvas.transform.position - transform.position;
    }

    public void Update()
    {
        if (turning)
        {
            transform.rotation = Quaternion.Slerp(current, target, rotateCounter);
            rotateCounter += 0.05f;
        }

        Suspision_Meter.GetComponent<Image>().fillAmount = (GuardSuspisionLevel / 100.0f);

        GetComponent<Access_Points>().canvas.transform.position = transform.position + Canvas_diff;
    
    }

    public void StartSuspision()
    {
        StartCoroutine(SuspisionManager());
    }

    public IEnumerator SuspisionManager()
    {
        while (true)
        {
            float len = (Player.transform.position - transform.position).magnitude;
            if (len <= 15.0f)
            {
                float dot = Vector3.Dot((Player.transform.position - transform.position).normalized, transform.forward);
                val = dot;
                if (dot > Mathf.Cos(AngleLimit))
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, Player.transform.position - transform.position, out hit, len))
                    {
                        if (hit.transform.CompareTag("Player"))
                        {
                            if (hit.distance <= 5.0f)
                            {
                                //increase 3x
                                GuardSuspisionLevel += 30.0f;

                            }
                            else if (hit.distance <= 10.0f)
                            {
                                // increase 2x
                                GuardSuspisionLevel += 15.0f;

                            }
                            else
                            {
                                //increase 1x
                                GuardSuspisionLevel += 7.0f;

                            }

                            if (GuardSuspisionLevel > 100.0f)
                            {
                                GuardSuspisionLevel = 100.0f;
                            }
                        }
                        else
                        {
                            DecreaseGuardLevel();
                        }
                    }
                    else
                    {
                        DecreaseGuardLevel();
                    }
                }
                else
                {
                    if(len <= 5.0)
                    {
                        GuardSuspisionLevel += 2.0f;
                    }
                    else
                    {
                        DecreaseGuardLevel();
                    }
                }


            }
            else
            {
                DecreaseGuardLevel();
            }
            yield return new WaitForSeconds(0.5f);
        }

    }

    void DecreaseGuardLevel()
    {
        GuardSuspisionLevel -= 2.0f;
        if (GuardSuspisionLevel < 0.0f)
        {
            GuardSuspisionLevel = 0.0f;
        }
    }






}
