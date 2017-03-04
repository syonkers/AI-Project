using UnityEngine;
using System.Collections;

public class AI_Main : MonoBehaviour
{
	enum AI_State {Dead, Dying, Swarming, Pursuing, Fleeing, Respawning};

	private const int MAX_HEALTH = 3;
	private const float FLEE_FACTOR = 0.6f;
	private const float VISION_LENGTH = 25f;

    [Header("Invader Variables")]
    public int health;
    private float shoot;
    public float respawnTime;
	private Rigidbody rbody;
	SphereCollider sphereCollider;
	private Vector3 velocity;
	private AI_State currentState;
	public bool isBeingChased;
	public bool changingDirection;
	private bool activated;

	[Header("GameObjects")]
	public GameObject target;
	public GameObject follower;
	GameObject middle, left, right;

	[Header("Death Variables")]
	public Vector3 deathVelocity = Vector3.zero;

    [Header("Other")]
    GameController gameController;
	ReverseGrade distanceGrade;
	Grade healthGrade;
	Pursue pursuitController;
	SwarmController swarmController;


    /****************************************************************************************************
     * Description: This is called once at the start of the game
     * Syntax: ---
     ****************************************************************************************************/
    void Start ()
    {
        middle = transform.FindChild("Middle").gameObject;
        left = transform.FindChild("Left").gameObject;
        right = transform.FindChild("Right").gameObject;

        pursuitController = GetComponent<Pursue>();
		swarmController = GetComponent<SwarmController>();
		gameController = GameObject.Find("Global Controller").GetComponent<GameController>();
		sphereCollider = GetComponent<SphereCollider>();
        rbody = GetComponent<Rigidbody>();

		healthGrade = new Grade ("Health", 0, MAX_HEALTH);
		velocity = Vector3.zero;
		changingDirection = false;
		activated = false;
		health = MAX_HEALTH;
        currentState = AI_State.Swarming;
    }


    /****************************************************************************************************
     * Description: This is called once every frame
     * Syntax: ---
     ****************************************************************************************************/
    void Update ()
	{
		CheckAIStatus ();
		switch (currentState) 
		{
            case AI_State.Dead:
                if (Time.time > respawnTime)
                    currentState = AI_State.Respawning;    
                break;

            case AI_State.Respawning:
			    sphereCollider.enabled = true;
                //Reform invader cubes
			    foreach (CubeController cube in GetComponentsInChildren<CubeController>())
				    cube.Respawn ();
			    FleeReset ();
			    health = MAX_HEALTH;
                Vector3 deltaSpawn = new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), Random.Range(-5, 5));
                transform.position = (tag == "Team1") ? gameController.team1Spawn + deltaSpawn : gameController.team2Spawn + deltaSpawn;
			    currentState = AI_State.Swarming;
			    break;

			case AI_State.Dying:
				rbody.velocity = Vector3.zero;
				sphereCollider.enabled = false;
				swarmController.DeathReset ();
			 	  //Send invader cubes exploding out
				foreach (CubeController cube in GetComponentsInChildren<CubeController>())
					cube.Death (deathVelocity);
				respawnTime = Time.time + 3;
				target = null;
				follower = null;
				currentState = AI_State.Dead;
			    break;

		    case AI_State.Swarming:
			    swarmController.Swarm ();
                //Give a chance to shoot only if the invader is facing roughly the right direction
			    float dot = Vector3.Dot (transform.forward, (Vector3.zero - transform.position).normalized);
                float distance = Vector3.Distance(transform.position, Vector3.zero);
			    if (dot > 0.99f) 
			    {
				    shoot = Random.Range (0f, distance);
				    if (shoot >= 1 && shoot < 2)
					    Shoot ();
			    }
			    TargetCheck ();
			    break;
		
		    case AI_State.Pursuing:
			    pursuitController.followTarget (target);
			    if (isBeingChased && follower) 
			    {
				    float choice = FightOrFlight ();
				    //If we decide we want to flee instead of continue chasing
				    if (choice < FLEE_FACTOR) 
				    {
					    target.GetComponent<AI_Main> ().follower = null;
					    target.GetComponent<AI_Main> ().isBeingChased = false;
					    target = null;
				    }
			    }
                
                shoot = Random.Range(0, 30);
                if (shoot == 1)
                    Shoot ();

                if (target != null)
                {
                    if (target.GetComponent<AI_Main>().health <= 0)
                        target = null;
                }
			    break;

		case AI_State.Fleeing:
			    //Make sure to call invoker only once while we are being followed
			if (!activated) {
				InvokeRepeating ("ChangeDirection", 0f, 4f);
				activated = true;
			}
			pursuitController.runAway (follower, changingDirection);

			    //Switch the bool back so runAway only changes direction when the invoker calls ChangeDirection function
			if (changingDirection) {
				changingDirection = false;
			}
			OutOfBoundsCheck ();
			    break;
		}
	}


    /****************************************************************************************************
     * Description: Creates two bullets for the invader
     * Syntax: Shoot();
     ****************************************************************************************************/
    void Shoot()
	{
        //Create the left laser
		GameObject leftLaser = GameObject.Instantiate(gameController.laserGameObject, left.transform.position, transform.rotation) as GameObject;
		VolumetricLines.VolumetricLineBehavior script = leftLaser.GetComponent<VolumetricLines.VolumetricLineBehavior>();
		script.LineWidth = 2;
		script.LineColor = (tag == "Team1") ? Color.red : Color.green;
        leftLaser.GetComponentInChildren<Light>().color = script.LineColor;
		GameObject.Destroy(leftLaser, 2);

        //Create the right laser
        GameObject rightLaser = GameObject.Instantiate(gameController.laserGameObject, right.transform.position, transform.rotation) as GameObject;
        script = rightLaser.GetComponent<VolumetricLines.VolumetricLineBehavior>();
        script.LineWidth = 2;
        script.LineColor = (tag == "Team1") ? Color.red : Color.green;
        rightLaser.GetComponentInChildren<Light>().color = script.LineColor;
        GameObject.Destroy(rightLaser, 2);
    }


    /****************************************************************************************************
     * Description: Determine whether the AI should continue chasing or should run away
     * Syntax: FightOrFlight();
     ****************************************************************************************************/
    float FightOrFlight()
	{
		float totalDistance = Vector3.Distance (target.transform.position, follower.transform.position);
		float myDistance = Vector3.Distance (target.transform.position, transform.position);
        
		distanceGrade = new ReverseGrade ("Distance", 0f, totalDistance);
		myDistance = distanceGrade.Evaluate (myDistance);
		float healthPercentage = healthGrade.Evaluate ((float)health);
		return Fuzzy.And (healthPercentage, myDistance);
	}


    /****************************************************************************************************
     * Description: Helper function needed for InvokeRepeating within Fleeing state 
     * Syntax: ChangeDirection();
     ****************************************************************************************************/
    void ChangeDirection()
	{
		changingDirection = true;
	}


    /****************************************************************************************************
     * Description: Called when we are done fleeing to reset to parameters for next time we are being
     *              chased
     * Syntax: FleeReset();
     ****************************************************************************************************/
    void FleeReset()
	{
		CancelInvoke ();
		changingDirection = false;
		activated = false;
	}


    /****************************************************************************************************
     * Description: Used to determine if any enemy has crossed the AIs line of sight and if they should
     *              target it
     * Syntax: TargetCheck();
     ****************************************************************************************************/
    void TargetCheck()
	{
        //Choose a random enemy
        GameObject potentialTarget;
        int team = (tag == "Team1") ? 1 : 2;
        if (team == 1)
            potentialTarget = gameController.team2[Random.Range(0, gameController.team2.Count)];
        else
            potentialTarget = gameController.team1[Random.Range(0, gameController.team1.Count)];

        //Make sure the enemy is alive, does not have a follower, is not targeting this and is within range
        bool withinDistance = Vector3.Distance(transform.position, potentialTarget.transform.position) < VISION_LENGTH;
        AI_Main enemyAI = potentialTarget.GetComponent<AI_Main>();
        if (enemyAI.health > 0 && enemyAI.target != gameObject && enemyAI.follower == null && withinDistance)
        {
            //Should we target them?
            int ShouldTarget = Random.Range (0, 50);
            if (ShouldTarget == 1)
            {
                target = potentialTarget;
                AI_Main enemy_ai = potentialTarget.GetComponent<AI_Main>();
                enemy_ai.follower = gameObject;
                enemy_ai.isBeingChased = true;
            }
        }
	}

    
    /****************************************************************************************************
     * Description: Check the AI status in order to switch to the correct state based on whether the AI
     *              has a target and/or follower
     * Syntax: CheckAIStatus();
     ****************************************************************************************************/
    void CheckAIStatus()
	{
		if ((currentState != AI_State.Dying) && (currentState != AI_State.Dead) && (currentState != AI_State.Respawning)) 
		{
			if (target == null) 
			{
				if (follower == null) 
				{
					currentState = AI_State.Swarming;
					FleeReset ();
				} else
					currentState = AI_State.Fleeing;
			} else
				currentState = AI_State.Pursuing;
		}
	}

	void OutOfBoundsCheck()
	{
		if (Mathf.Abs (transform.position.x) > 120 || Mathf.Abs (transform.position.y) > 120 || Mathf.Abs (transform.position.z) > 120)
			currentState = AI_State.Dying;
	}

    /****************************************************************************************************
     * Description: This is called every frame an collision is happening
     * Syntax: ---
     ****************************************************************************************************/
    void OnTriggerStay(Collider col)
    {
        if (col.tag == "Laser")
        {
            if (Vector3.Distance(transform.position, col.transform.position) < 0.75f)
                health--;
            if (health <= 0)
                currentState = AI_State.Dying;
        }
    }
}