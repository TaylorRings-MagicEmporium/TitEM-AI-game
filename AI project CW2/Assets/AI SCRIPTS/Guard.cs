using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
public class Guard : MonoBehaviour
{
    // bools based on type of guard
    public bool Stand = false;
    public bool Waypoint = false;

    // gets the agent for the guard and gets the player object
    protected NavMeshAgent agent;
    protected GameObject Player;

    // deduces whether the enemy should be turning or not.
    protected bool turning = false;
    
    // used for lerping between previous rotation and new rotation.
    protected Quaternion current;
    protected Quaternion target;
    
    // incrementer for rotation
    protected float rotateCounter = 0;

    // a value that depicts how suspicious the guard is
    public float GuardSuspicionLevel = 0.0f;


    // depicts how "wide" the guard can see
    public float AngleLimit = 45.0f;
    //public float val;

    // depicts the zones that the guard can see the player
    public float FirstZoneLimit = 7.5f;
    public float SecondZoneLimit = 12.0f;
    public float ThirdZoneLimit = 15.0f;

    public GuardUI guardUI;
    public SOGuardData guardData;

    //used for debugging
    public LineRenderer lr;

    //behaviour of a guard
    public enum BehaviourStates {PATROL, INVESTIGATE, CHASE };
    public BehaviourStates currentStates;
    BehaviourStates prevStates;

    // active corountine that specifies the behaviour actions of the guard currently.
    protected Coroutine Current_Behaviour_Enum;

    // is this a new behaviour for the guard (changed recently)
    bool StartBehaviour = false;

    gamemanager gm;

    GameEventListener eventListener;

    // has a treasure been stolen?
    public bool TreasureStolen = false;

    // is the guard active based on game-manager's state?
    public bool DisableGuard = false;

    // should debug_lines be present?
    public bool Debug_lines;

    // class containing gameobjects related to displaying guard info.
    protected Access_Points AP;

    // initialises the guard to begin patrolling.
    public virtual void Start_Guard()
    {
        // attaches the Guard to the gamemanager
        gm = GameObject.FindGameObjectWithTag("Manager").GetComponent<gamemanager>();
        AP = GetComponent<Access_Points>();
        Debug_lines = AP.Debug;
        agent = GetComponent<NavMeshAgent>();
        Player = GameObject.FindGameObjectWithTag("Player");
        guardUI = GetComponent<GuardUI>();

        currentStates = BehaviourStates.PATROL;
        prevStates = BehaviourStates.PATROL;
        StartCoroutine(Behaviour_State_Update()); // starts behaviour change
        StartCoroutine(Behaviour_thoughts()); // start visually showing guard's thoughts
        
        eventListener = GetComponent<GameEventListener>();
        eventListener.Response.AddListener(TreasureStolenAlert);

        // if true, then output regions where the player is seen via Debug
        if (Debug_lines)
        {
            lr = AP.Line_Renderer.GetComponent<LineRenderer>();
            lr.SetPosition(1, Quaternion.Euler(0, AngleLimit, 0) * transform.forward * FirstZoneLimit); //positive extreme
            lr.SetPosition(2, Quaternion.Euler(0, -AngleLimit, 0) * transform.forward * FirstZoneLimit); //negative extreme
            lr.SetPosition(4, Quaternion.Euler(0, AngleLimit, 0) * transform.forward * SecondZoneLimit); //positive extreme
            lr.SetPosition(5, Quaternion.Euler(0, -AngleLimit, 0) * transform.forward * SecondZoneLimit); //negative extreme
            lr.SetPosition(7, Quaternion.Euler(0, AngleLimit, 0) * transform.forward * ThirdZoneLimit); //positive extreme
            lr.SetPosition(8, Quaternion.Euler(0, -AngleLimit, 0) * transform.forward * ThirdZoneLimit); //negative extreme
        }
        else
        {
            AP.Line_Renderer.SetActive(false);
        }
    }

    public void ResetGuard()
    {
        StopAllCoroutines();
    }

    // runs every frame
    private void Update()
    {
        // if turning is true, then perform a smooth transition from one quaternion to another based on an incrementer.
        if (turning) 
        {
            transform.rotation = Quaternion.Slerp(current, target, rotateCounter);
            rotateCounter += 0.05f;
        }

        //Behaviour_Response();
    }

    // begins the AI suspicion manager. 
    public void StartSuspicion()
    {
        StartCoroutine(SuspicionManager());
    }

    // calculates and determines the suspicion level based on numerious conditions.
    public IEnumerator SuspicionManager()
    {

        while (true)
        {
            if(gm.Current_Game_State == gamemanager.Game_State.GAME) // if the game-manager state is in gameplay...
            {
                if (!Player.GetComponent<Player_Powers>().InvisibilityOn) // if invisiblity is not on...
                {
                    float len = (Player.transform.position - transform.position).sqrMagnitude;
                    bool playerWithinLength = len <= Mathf.Pow(guardData.MaxSightDistance, 2);

                    float dot = Vector3.Dot((Player.transform.position - transform.position).normalized, transform.forward);
                    bool playerWithinSightRange = dot > Mathf.Cos(AngleLimit);

                    if (playerWithinLength && playerWithinSightRange)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(transform.position, Player.transform.position - transform.position, out hit, guardData.MaxSightDistance)) // if the guard can see something...
                        {
                            if (hit.transform.CompareTag("Player")) // if that object is the player itself, then add an amount of the guard suspicion level
                            {
                                if (hit.distance <= FirstZoneLimit) // the closest zone
                                {
                                    //increase 3x
                                    IncreaseGuardLevel(27.0f);

                                }
                                else if (hit.distance <= SecondZoneLimit) // the midway zone
                                {
                                    // increase 2x
                                    IncreaseGuardLevel(16.0f);

                                }
                                else // the fartherest zone
                                {
                                    //increase 1x
                                    IncreaseGuardLevel(7.0f);

                                }
                            }
                            else
                            {
                                DecreaseGuardLevel(); // decrease the guard level
                            }
                        }
                        else
                        {
                            DecreaseGuardLevel(); // decrease the guard level
                        }
                    }
                    else if(len <= Mathf.Pow(guardData.MaxFeelDistance,2))
                    {
                        IncreaseGuardLevel(2.5f);
                    }
                    else
                    {
                        DecreaseGuardLevel(); // decrease the guard level
                    }
                }

                if (!TreasureStolen) // if a treasure has not been detected to be stolen yet...
                {

                    RaycastHit hitT;
                    if (Physics.Raycast(transform.position, transform.forward, out hitT, guardData.MaxSightDistance))
                    {
                        if (hitT.transform.CompareTag("Treasure")) // if the hit object was the treasure collider...
                        {
                            if (hitT.transform.GetComponent<Treasure_Info>().IsTreasureTaken()) // if the treasure has been stolen, then alert all guards
                            {
                                // the treasure is taken! alert all guards!
                                AP.OnTreasureStolen.Raise();
                                Debug.Log("TREASURE TAKEN");
                            }
                        }
                    }
                }

            }
            yield return new WaitForSeconds(0.5f);

        }
    }

    // decreases the suspicion level based on the current behaviour state
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

    void IncreaseGuardLevel(float addedValue)
    {
        GuardSuspicionLevel += addedValue;
        GuardSuspicionLevel = Mathf.Clamp(GuardSuspicionLevel, guardData.MinimumLimit, guardData.MaximumLimit);
        guardUI.ChangeSuspicionFillAmount(GuardSuspicionLevel, guardData.MaximumLimit);
    }

    // determines what the behaviour should be based on the suspicion levels
    IEnumerator Behaviour_State_Update()
    {
        while (true)
        {
            prevStates = currentStates;
            switch (currentStates)
            {
                case BehaviourStates.PATROL:

                    if (GuardSuspicionLevel > guardData.LimitToPatrol) // if the level has risen to the investigation stage, then update behaviour
                    {
                        currentStates = BehaviourStates.INVESTIGATE;
                    }

                    break;
                case BehaviourStates.INVESTIGATE:

                    if (GuardSuspicionLevel > guardData.LimitToInvestigate) // if the level has risen to the chase stage, then update behaviour
                    {
                        currentStates = BehaviourStates.CHASE;
                    }
                    else if (GuardSuspicionLevel < guardData.LimitToPatrol) // if the level has lowered to the patrol stage, then update behaviour
                    {
                        currentStates = BehaviourStates.PATROL;
                    }

                    break;
                case BehaviourStates.CHASE:

                    if (GuardSuspicionLevel < guardData.LimitToInvestigate) // if the level has lowered to the investigation stage, then update behaviour
                    {
                        currentStates = BehaviourStates.INVESTIGATE;
                    }

                    break;
            }

            if(prevStates != currentStates) // if the behaviour has changed, then set the bool for appropriate action
            {
                Behaviour_Response();
            }
            yield return new WaitForSeconds(0.5f);
            
        }
    }

    // begins certain functions to setup the behaviour actions
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

    // virtual function for beginning patrols (based on type of guard)
    protected virtual void Begin_Patrol() { }

    // virtual function for beginning investigations
    protected virtual void Begin_Investigate() {

        //Debug.Log("INVESTIGATING");
        turning = false;
        StopCoroutine(Current_Behaviour_Enum);
        AP.Guard_ani.SetBool("IsMoving", false);
        Current_Behaviour_Enum = StartCoroutine(InvestigateArea());

    }

    // virtual function for beginning chasing
    protected virtual void Begin_Chase() {
        //Debug.Log("CHASING");
        turning = false;
        StopCoroutine(Current_Behaviour_Enum);
        AP.Guard_ani.SetBool("IsMoving", false);
        Current_Behaviour_Enum = StartCoroutine(ChasePlayer());
    }

    // chases the player to try and catch them
    IEnumerator ChasePlayer()
    {

        List<GameObject> listOfGuards = new List<GameObject>(GameObject.FindGameObjectsWithTag("Guard"));
        foreach (GameObject gu in listOfGuards) 
        {
            bool guardWithinRange = Vector3.Distance(gu.transform.position, transform.position) <= 15.0f;
            if (guardWithinRange)
            {
                gu.GetComponent<Guard>().AlertGuard(); // alerts all guards within range about chasing the player.

            }
        }

        AP.Guard_ani.SetBool("IsMoving", true); // set animation to walk
        while (true)
        {
            agent.SetDestination(Player.transform.position); // sets the position to the player's position.
            agent.updateRotation = true;

            yield return new WaitForSeconds(0.5f);
        }
    }

    // investigate the area where the player might be (encourages the player to always move)
    IEnumerator InvestigateArea()
    {
        Position_Status ac = Player.GetComponent<Position_Status>();
        
        while (true)
        {
            // this part chooses a random position based on a sphere radius around a deplayed position of the player
            Vector3 randomDirection = Random.insideUnitSphere * 5.0f;
            randomDirection += ac.GetPos();
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, 5.0f,-1);

            AP.Guard_ani.SetBool("IsMoving", true);
            agent.SetDestination(navHit.position);

            yield return new WaitForSeconds(1.5f);
            AP.Guard_ani.SetBool("IsMoving", false);
        }
    }

    // when a collision is detected...
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player")) // if it was the player...
        {
            if(currentStates == BehaviourStates.CHASE && !DisableGuard) // and the guard is in chase mode, then game over is called as the guard has captured the player
            {
                AP.OnPlayerCaptured.Raise();
            }
            else // if not, alert the guard that it just been touched
            {
                GuardSuspicionLevel += 60.0f;
            }

        }
    }

    // when called by a nearby chasing guard, increase the sispicion level
    public void AlertGuard()
    {
        //Debug.Log("ALERTED");
        GuardSuspicionLevel += 60.0f;
    }

    // if a treasure has been stolen and has been detected by a guard, then alert the guard
    public void TreasureStolenAlert()
    {
        TreasureStolen = true;
        guardData.MinimumLimit += 40.0f; // the minimum limit of the suspicion will be increased as there is missing treasure.
        GuardSuspicionLevel = Mathf.Clamp(GuardSuspicionLevel + 5.0f, guardData.MinimumLimit, guardData.MaximumLimit);
    }

    // for visual representation of the current behaviour, a thought bubble will appear
    IEnumerator Behaviour_thoughts()
    {


        int wait;
        guardUI.ToggleThoughtImage(false);
        // randomly waits for time to make the thoughts more authentic
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

}
