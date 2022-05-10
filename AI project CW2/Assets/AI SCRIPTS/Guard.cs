using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// An AI agent that acts as a Mueseum guard. they can Patrol, Investigate and Chase.
/// </summary>
public class Guard : MonoBehaviour
{
    private NavMeshAgent agent;
    private GameObject Player;

    private bool turning = false;

    private Quaternion current;
    private Quaternion target;

    // incrementer for rotation
    private float rotateCounter = 0;

    //[SerializeField]
    public float GuardSuspicionLevel = 0.0f;

    GuardUI guardUI;
    public SOGuardData guardData;

    //used for debugging
    public LineRenderer debugLr;

    [HideInInspector]
    public enum BehaviourStates {PATROL, INVESTIGATE, CHASE };
    public BehaviourStates currentStates;
    BehaviourStates prevStates;

    // active corountine based on current state
    protected Coroutine Current_Behaviour_Enum;

    gamemanager gm;
    GameEventListener eventListener;

    [Space]
    public bool IsTreasureStolen = false;
    public bool IsDisableGuard = false;
    public bool ShowDebug_lines;

    [Space]
    public GameEvent OnTreasureStolen;
    public GameEvent OnPlayerCaptured;
    public Animator Guard_animation;

    [Space]
    public List<Vector3> waypoints = new List<Vector3>();
    int waypoint_index = 0;

    /// <summary>
    /// Setups and starts the guard AI with behaviours. Debug lines are also shown here.
    /// </summary>
    public void Start_Guard()
    {
        gm = GameObject.FindGameObjectWithTag("Manager").GetComponent<gamemanager>();
        agent = GetComponent<NavMeshAgent>();
        Player = GameObject.FindGameObjectWithTag("Player");
        guardUI = GetComponent<GuardUI>();

        agent.enabled = false;
        transform.position = waypoints[0];
        transform.rotation = Quaternion.identity;
        agent.enabled = true;


        currentStates = BehaviourStates.PATROL;
        prevStates = BehaviourStates.PATROL;

        eventListener = GetComponent<GameEventListener>();
        eventListener.Response.AddListener(TreasureStolenAlert);

        StartCoroutine(Behaviour_State_Update());
        StartCoroutine(Behaviour_thoughts());
        Behaviour_Response();
        StartSuspicion();

        if (ShowDebug_lines)
        {
            debugLr.SetPosition(1, Quaternion.Euler(0, guardData.AngleLimit, 0) * transform.forward * guardData.FirstZoneLimit); //positive extreme
            debugLr.SetPosition(2, Quaternion.Euler(0, -guardData.AngleLimit, 0) * transform.forward * guardData.FirstZoneLimit); //negative extreme
            debugLr.SetPosition(4, Quaternion.Euler(0, guardData.AngleLimit, 0) * transform.forward * guardData.SecondZoneLimit); //positive extreme
            debugLr.SetPosition(5, Quaternion.Euler(0, -guardData.AngleLimit, 0) * transform.forward * guardData.SecondZoneLimit); //negative extreme
            debugLr.SetPosition(7, Quaternion.Euler(0, guardData.AngleLimit, 0) * transform.forward * guardData.ThirdZoneLimit); //positive extreme
            debugLr.SetPosition(8, Quaternion.Euler(0, -guardData.AngleLimit, 0) * transform.forward * guardData.ThirdZoneLimit); //negative extreme
        }
        else
        {
            debugLr.enabled = false;
        }
    }

    /// <summary>
    /// Resets the Guard's Behaviour and values to when it was initialised.
    /// </summary>
    public void ResetGuard()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        if (turning) 
        {
            transform.rotation = Quaternion.Slerp(current, target, rotateCounter);
            rotateCounter += 0.05f;
        }
    }

    /// <summary>
    /// Starts the Guard's suspicion meter controller.
    /// </summary>
    public void StartSuspicion()
    {

        StartCoroutine(SuspicionManager());
    }

    /// <summary>
    /// Calculates the amount of suspicion a guard have based on it's surroundings.
    /// </summary>
    /// <returns></returns>
    public IEnumerator SuspicionManager()
    {

        while (true)
        {

            if (gm.Current_Game_State == gamemanager.Game_State.GAME)
            {
                if (!Player.GetComponent<Player_Powers>().InvisibilityOn)
                {
                    float len = (Player.transform.position - transform.position).sqrMagnitude;
                    bool playerWithinLength = len <= Mathf.Pow(guardData.MaxSightDistance, 2);

                    float dot = Vector3.Dot((Player.transform.position - transform.position).normalized, transform.forward);
                    bool playerWithinSightRange = dot > Mathf.Cos(guardData.AngleLimit);

                    if (playerWithinLength && playerWithinSightRange)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(transform.position, Player.transform.position - transform.position, out hit, guardData.MaxSightDistance))
                        {
                            if (hit.transform.CompareTag("Player"))
                            {
                                if (hit.distance <= guardData.FirstZoneLimit)
                                {
                                    //increase 3x
                                    IncreaseGuardLevel(27.0f);

                                }
                                else if (hit.distance <= guardData.SecondZoneLimit)
                                {
                                    // increase 2x
                                    IncreaseGuardLevel(16.0f);

                                }
                                else
                                {
                                    //increase 1x
                                    IncreaseGuardLevel(7.0f);

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
                    else if(len <= Mathf.Pow(guardData.MaxFeelDistance,2))
                    {
                        IncreaseGuardLevel(2.5f);
                    }
                    else
                    {
                        DecreaseGuardLevel();
                    }
                }

                if (!IsTreasureStolen)
                {

                    RaycastHit hitT;
                    if (Physics.Raycast(transform.position, transform.forward, out hitT, guardData.MaxSightDistance))
                    {
                        if (hitT.transform.CompareTag("Treasure"))
                        {
                            if (hitT.transform.GetComponent<Treasure_Info>().IsTreasureTaken())
                            {
                                OnTreasureStolen.Raise();
                                Debug.Log("TREASURE TAKEN");
                            }
                        }
                    }
                }

            }
            yield return new WaitForSeconds(0.5f);

        }
    }

    /// <summary>
    /// Decreases the guard's suspicion meter based on conditions
    /// </summary>
    void DecreaseGuardLevel()
    {
        if(currentStates == BehaviourStates.CHASE)
        {
            GuardSuspicionLevel -= 2.5f;
        }
        else
        {
            GuardSuspicionLevel -= 2.0f;
        }

        GuardSuspicionLevel = Mathf.Clamp(GuardSuspicionLevel, guardData.MinimumLimit, guardData.MaximumLimit);
        guardUI.ChangeSuspicionFillAmount(GuardSuspicionLevel, guardData.MaximumLimit);
    }

    /// <summary>
    /// Increases the guard's suspicion meter based on the value given.
    /// </summary>
    /// <param name="addedValue">The amount of suspicion to add towards the meter</param>
    void IncreaseGuardLevel(float addedValue)
    {
        GuardSuspicionLevel += addedValue;
        GuardSuspicionLevel = Mathf.Clamp(GuardSuspicionLevel, guardData.MinimumLimit, guardData.MaximumLimit);
        guardUI.ChangeSuspicionFillAmount(GuardSuspicionLevel, guardData.MaximumLimit);
    }

    /// <summary>
    /// Controls how the guard's behaviour changes when certain conditions are met.
    /// </summary>
    /// <returns></returns>
    IEnumerator Behaviour_State_Update()
    {
        while (true)
        {
            prevStates = currentStates;
            switch (currentStates)
            {
                case BehaviourStates.PATROL:

                    if (GuardSuspicionLevel > guardData.LimitToPatrol) 
                    {
                        currentStates = BehaviourStates.INVESTIGATE;
                    }

                    break;
                case BehaviourStates.INVESTIGATE:

                    if (GuardSuspicionLevel > guardData.LimitToInvestigate) 
                    {
                        currentStates = BehaviourStates.CHASE;
                    }
                    else if (GuardSuspicionLevel < guardData.LimitToPatrol) 
                    {
                        currentStates = BehaviourStates.PATROL;
                    }

                    break;
                case BehaviourStates.CHASE:

                    if (GuardSuspicionLevel < guardData.LimitToInvestigate) 
                    {
                        currentStates = BehaviourStates.INVESTIGATE;
                    }

                    break;
            }

            if(prevStates != currentStates)
            {
                Behaviour_Response();
            }
            yield return new WaitForSeconds(0.5f);
            
        }
    }

    /// <summary>
    /// Begins the entry functions for the current state.
    /// </summary>
    public void Behaviour_Response()
    {
        switch (currentStates)
        {
            case BehaviourStates.PATROL:

                Begin_Patrol();
                break;

            case BehaviourStates.INVESTIGATE:

                Begin_Investigate();
                break;

            case BehaviourStates.CHASE:

                Begin_Chase();
                break;
        }

    }

    /// <summary>
    /// the Entry function for PATROL State
    /// </summary>
    protected void Begin_Patrol() 
    {
        if(Current_Behaviour_Enum != null)
        {
            StopCoroutine(Current_Behaviour_Enum);
        }
        Current_Behaviour_Enum = StartCoroutine(LookAndMove());
    }

    /// <summary>
    /// the Entry function for INVESTIGATE State
    /// </summary>
    protected void Begin_Investigate() {

        //Debug.Log("INVESTIGATING");
        turning = false;
        StopCoroutine(Current_Behaviour_Enum);
        Guard_animation.SetBool("IsMoving", false);
        Current_Behaviour_Enum = StartCoroutine(InvestigateArea());

    }

    /// <summary>
    /// the Entry function for CHASE State
    /// </summary>
    protected void Begin_Chase() {
        //Debug.Log("CHASING");
        turning = false;
        StopCoroutine(Current_Behaviour_Enum);
        Guard_animation.SetBool("IsMoving", false);
        Current_Behaviour_Enum = StartCoroutine(ChasePlayer());
    }

    /// <summary>
    /// The active update function for the CHASE state (alerts guards around them and moves to player's location)
    /// </summary>
    /// <returns></returns>
    IEnumerator ChasePlayer()
    {

        List<GameObject> listOfGuards = new List<GameObject>(GameObject.FindGameObjectsWithTag("Guard"));
        foreach (GameObject gu in listOfGuards) 
        {
            bool guardWithinRange = Vector3.Distance(gu.transform.position, transform.position) <= 15.0f;
            if (guardWithinRange)
            {
                gu.GetComponent<Guard>().AlertGuard();

            }
        }

        Guard_animation.SetBool("IsMoving", true);
        while (true && gm.Current_Game_State != gamemanager.Game_State.CAPTURED)
        {
            agent.SetDestination(Player.transform.position);
            agent.updateRotation = true;

            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// The active update function for the INVESTIGATE state (moves to a random location that the player has been before)
    /// </summary>
    /// <returns></returns>
    IEnumerator InvestigateArea()
    {
        Position_Status ac = Player.GetComponent<Position_Status>();
        
        while (true)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 5.0f;
            randomDirection += ac.GetPos();
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, 5.0f,-1);

            Guard_animation.SetBool("IsMoving", true);
            agent.SetDestination(navHit.position);

            yield return new WaitForSeconds(1.5f);
            Guard_animation.SetBool("IsMoving", false);
        }
    }

    /// <summary>
    /// The active update function for the PATROL state (goes through it's waypoints and rotates around)
    /// </summary>
    /// <returns></returns>
    IEnumerator LookAndMove()
    {
        bool reverse = false;
        while (true)
        {
            turning = true;

            for (int i = 0; i < 3; i++)
            {
                rotateCounter = 0.0f;
                current = transform.rotation;
                target = current * Quaternion.Euler(0, 90f, 0);
                yield return new WaitForSeconds(guardData.waitingTime);
            }

            turning = false;

            if (waypoints.Count > 1)
            {
                if (waypoint_index == waypoints.Count - 1 && !reverse)
                {
                    reverse = true;
                }
                if (waypoint_index == 0 && reverse)
                {
                    reverse = false;
                }

                if (reverse)
                {
                    waypoint_index--;
                }
                else
                {
                    waypoint_index++;
                }

                agent.SetDestination(waypoints[waypoint_index]);

                bool closeEnough = false;
                Guard_animation.SetBool("IsMoving", true);
                while (!closeEnough)
                {
                    if ((transform.position - waypoints[waypoint_index]).magnitude < 1.0f)
                    {
                        closeEnough = true;
                    }
                    yield return new WaitForSeconds(0.5f);
                }
                Guard_animation.SetBool("IsMoving", false);
            }

        }
    }


    /// <summary>
    /// alerts the guard via a CHASE guard to increase their suspicion meter.
    /// </summary>
    public void AlertGuard()
    {
        //Debug.Log("ALERTED");
        GuardSuspicionLevel += 60.0f;
    }

    /// <summary>
    /// alert from an external guard that treasure is stolen, and increase the minimum limit.
    /// </summary>
    public void TreasureStolenAlert()
    {
        IsTreasureStolen = true;
        guardData.MinimumLimit += 40.0f;
        GuardSuspicionLevel = Mathf.Clamp(GuardSuspicionLevel + 5.0f, guardData.MinimumLimit, guardData.MaximumLimit);
    }

    /// <summary>
    /// Controls how the state is shown via UI thought images.
    /// </summary>
    /// <returns></returns>
    IEnumerator Behaviour_thoughts()
    {
        int wait;
        guardUI.ToggleThoughtImage(false);
        while (true)
        {
            wait = 2;
            if (guardUI.GetThoughtImageActiveState())
            {
                guardUI.ToggleThoughtImage(false);
                wait = 3;
            }
            else
            {
                if (Random.Range(0, 10) > 5)
                {
                    guardUI.ChangeThoughtImage((int)currentStates);
                    guardUI.ToggleThoughtImage(true);
                    wait = 4;
                }
                else
                {
                    wait = 2;
                }
            }
            yield return new WaitForSeconds(wait);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            bool playerCanBeCaptured = currentStates == BehaviourStates.CHASE && !IsDisableGuard;
            if (playerCanBeCaptured)
            {
                OnPlayerCaptured.Raise();
            }
            else
            {
                GuardSuspicionLevel += 60.0f;
            }

        }
    }

    public void OnDestroy()
    {
        StopAllCoroutines();
    }

}
