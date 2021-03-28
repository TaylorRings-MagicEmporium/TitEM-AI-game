using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
public class Guard : MonoBehaviour
{

    //public Vector2 GridPos;

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

    public float FirstZoneLimit = 7.5f;
    public float SecondZoneLimit = 12.0f;
    public float ThirdZoneLimit = 15.0f;


    Vector3 Canvas_diff;

    public LineRenderer lr;

    public float MinimumLimit = 0.0f;
    public float MaximumLimit = 100.0f;

    public enum BehaviourStates {PATROL, INVESTIGATE, CHASE };
    public BehaviourStates currentStates;
    BehaviourStates prevStates;

    protected Coroutine Current_Behaviour_Enum;

    bool StartBehaviour = false;
    public GameObject Suspision_Meter;

    gamemanager gm;

    public bool TreasureStolen = false;

    public bool DisableGuard = false;
    public bool Debug_lines = true;

    public void Start()
    {
        gm = GameObject.FindGameObjectWithTag("Manager").GetComponent<gamemanager>();


        agent = GetComponent<NavMeshAgent>();
        Player = GameObject.FindGameObjectWithTag("Player");
        Suspision_Meter = GetComponent<Access_Points>().Suspision_Meter;

        Canvas_diff = GetComponent<Access_Points>().canvas.transform.position - transform.position;
        currentStates = BehaviourStates.PATROL;
        prevStates = BehaviourStates.PATROL;
        StartCoroutine(Behaviour_State_Update());

        if (Debug_lines)
        {
            lr = GetComponent<Access_Points>().Line_Renderer.GetComponent<LineRenderer>();
            lr.SetPosition(1, Quaternion.Euler(0, AngleLimit, 0) * transform.forward * FirstZoneLimit); //positive extreme
            lr.SetPosition(2, Quaternion.Euler(0, -AngleLimit, 0) * transform.forward * FirstZoneLimit); //negative extreme
            lr.SetPosition(4, Quaternion.Euler(0, AngleLimit, 0) * transform.forward * SecondZoneLimit); //positive extreme
            lr.SetPosition(5, Quaternion.Euler(0, -AngleLimit, 0) * transform.forward * SecondZoneLimit); //negative extreme
            lr.SetPosition(7, Quaternion.Euler(0, AngleLimit, 0) * transform.forward * ThirdZoneLimit); //positive extreme
            lr.SetPosition(8, Quaternion.Euler(0, -AngleLimit, 0) * transform.forward * ThirdZoneLimit); //negative extreme
        }
        else
        {
            GetComponent<Access_Points>().Line_Renderer.SetActive(false);
        }


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

        Behaviour_Response();
    }

    public void StartSuspision()
    {
        StartCoroutine(SuspisionManager());
    }

    public IEnumerator SuspisionManager()
    {

        while (true)
        {
            if(gm.Current_Game_State == gamemanager.Game_State.GAME)
            {
                if (!Player.GetComponent<Player_Powers>().InvisibilityOn)
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
                                    if (hit.distance <= FirstZoneLimit)
                                    {
                                        //increase 3x
                                        GuardSuspisionLevel += 27.0f;

                                    }
                                    else if (hit.distance <= SecondZoneLimit)
                                    {
                                        // increase 2x
                                        GuardSuspisionLevel += 16.0f;

                                    }
                                    else
                                    {
                                        //increase 1x
                                        GuardSuspisionLevel += 7.0f;

                                    }

                                    if (GuardSuspisionLevel > MaximumLimit)
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
                            if (len <= 3.0)
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
                }
                if (!TreasureStolen)
                {

                    RaycastHit hitT;
                    if (Physics.Raycast(transform.position, transform.forward, out hitT, 15.0f))
                    {
                        if (hitT.transform.CompareTag("Treasure"))
                        {
                            float dot = Vector3.Dot((hitT.transform.position - transform.position).normalized, transform.forward);
                            if (dot > Mathf.Cos(AngleLimit))
                            {
                                if (hitT.transform.GetComponent<Treasure_Info>().IsTreasureTaken())
                                {
                                    // the treasure is taken! alert all guards!
                                    gm.TreasureTakenRiseAlerts();
                                    Debug.Log("TREASURE TAKEN");
                                }
                            }

                        }
                    }
                }

            }
            yield return new WaitForSeconds(0.5f);

        }

    }

    void DecreaseGuardLevel()
    {
        if(currentStates == BehaviourStates.CHASE)
        {
            GuardSuspisionLevel -= 4.0f;
        }
        else
        {
            GuardSuspisionLevel -= 2.0f;
            if (GuardSuspisionLevel < MinimumLimit)
            {
                GuardSuspisionLevel = MinimumLimit;
            }
        }

    }

    IEnumerator Behaviour_State_Update()
    {
        while (true)
        {
            prevStates = currentStates;
            switch (currentStates)
            {
                case BehaviourStates.PATROL:

                    if (GuardSuspisionLevel > gm.PatrolToInvest)
                    {
                        currentStates = BehaviourStates.INVESTIGATE;
                    }

                    break;
                case BehaviourStates.INVESTIGATE:

                    if (GuardSuspisionLevel > gm.InvestToChase)
                    {
                        currentStates = BehaviourStates.CHASE;
                    }
                    else if (GuardSuspisionLevel < gm.PatrolToInvest)
                    {
                        currentStates = BehaviourStates.PATROL;
                    }

                    break;
                case BehaviourStates.CHASE:

                    if (GuardSuspisionLevel < gm.InvestToChase)
                    {
                        currentStates = BehaviourStates.INVESTIGATE;
                    }

                    break;
            }

            if(prevStates != currentStates)
            {
                StartBehaviour = true;
            }
            yield return new WaitForSeconds(0.5f);
            
        }
    }

    public void Behaviour_Response()
    {
        switch (currentStates)
        {
            case BehaviourStates.PATROL:

                if (StartBehaviour)
                {
                    Begin_Patrol();
                    StartBehaviour = false;
                }

                break;
            case BehaviourStates.INVESTIGATE:

                if (StartBehaviour)
                {
                    Begin_Investigate();
                    StartBehaviour = false;

                }

                break;
            case BehaviourStates.CHASE:

                if (StartBehaviour)
                {
                    Begin_Chase();
                    StartBehaviour = false;

                }

                break;
        }
    }

    protected virtual void Begin_Patrol() { }
    protected virtual void Begin_Investigate() {

        Debug.Log("INVESTIGATING");
        StopCoroutine(Current_Behaviour_Enum);
        Current_Behaviour_Enum = StartCoroutine(InvestigateArea());

    }


    protected virtual void Begin_Chase() {
        Debug.Log("CHASING");
        StopCoroutine(Current_Behaviour_Enum);
        Current_Behaviour_Enum = StartCoroutine(ChasePlayer());
    }

    IEnumerator ChasePlayer()
    {

        GameObject g = GameObject.FindGameObjectWithTag("Player");

        List<GameObject> listOfGuards = new List<GameObject>(GameObject.FindGameObjectsWithTag("Guard"));
        foreach (GameObject gu in listOfGuards)
        {
            if (Vector3.Distance(gu.transform.position, transform.position) <= 15.0f)
            {
                gu.GetComponent<Guard>().AlertGuard();

            }
        }

        while (true)
        {
            agent.SetDestination(g.transform.position);


            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator InvestigateArea()
    {
        Position_Status ac = GameObject.FindGameObjectWithTag("Player").GetComponent<Position_Status>();
        while (true)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 7.0f;
            randomDirection += ac.GetPos();
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, 7.0f,-1);

            agent.SetDestination(navHit.position);

            yield return new WaitForSeconds(2.5f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if(currentStates == BehaviourStates.CHASE && !DisableGuard)
            {
                gm.Game_Over_Called();
                //DisableGuard = true;
            }
            else
            {
                GuardSuspisionLevel += 60.0f;
            }

        }
    }

    public void AlertGuard()
    {
        Debug.Log("ALERTED");
        GuardSuspisionLevel += 60.0f;
    }

    public void TreasureStolenAlert()
    {
        TreasureStolen = true;
        MinimumLimit += 40.0f;
        if(GuardSuspisionLevel < MinimumLimit)
        {
            GuardSuspisionLevel = 45.0f;
        }
    }
}
