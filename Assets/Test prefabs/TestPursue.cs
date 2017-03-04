using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestPursue : MonoBehaviour
{
	const float VISION_LENGTH = 15f;
	private float speed;
	public Vector3 targetDirection;
	private GameObject right;
	private GameObject left;
	public GameObject asteroid;
	private int pursueTurnspeed = 4;
	private float maxTravelDistance;
	private GameController gameController;
	private Rigidbody rbody;


	/****************************************************************************************************
     * Description: This is called once at the start of the game
     * Syntax: ---
     ****************************************************************************************************/
	void Start()
	{
		speed = Random.Range(5f, 5.9f);
		gameController = GameObject.Find("Global Controller").GetComponent<GameController>();
		maxTravelDistance = Mathf.Sqrt(3) * gameController.area.x;
		rbody = GetComponent<Rigidbody> ();

	}


	/****************************************************************************************************
     * Description: Controls the behavior of an invader that is following a target
     * Syntax: followTarget(rbody, target);
     *              target = the gameobject being followed
     ****************************************************************************************************/
	public void followTarget (GameObject target)
	{
		Vector3 lookDirection = Vector3.zero;
		targetDirection = Vector3.zero;

		targetDirection = ((target.transform.position + (target.transform.forward)) - transform.position).normalized;
		lookDirection = ((target.transform.position + (target.transform.forward)) - transform.position);

		//Calculates the move direction of invader and to avoid obstacles
		right = transform.FindChild("Right").gameObject;
		left = transform.FindChild("Left").gameObject;
		RaycastHit hit = new RaycastHit();
		targetDirection += castRay (left.transform.position, lookDirection, hit, VISION_LENGTH);
		targetDirection += castRay (right.transform.position, lookDirection, hit, VISION_LENGTH);

		//Rotates the invader
		Quaternion lookRotation = Quaternion.identity;
		if (targetDirection != Vector3.zero)
			lookRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
		transform.rotation = Quaternion.Slerp (transform.rotation, lookRotation, Time.deltaTime * pursueTurnspeed);

		rbody.velocity = targetDirection * (speed - 1);
	}


	/****************************************************************************************************
     * Description: Controls the behavior of an invader that is running away from a target
     * Syntax: runAway(rbody, target);
     *              follower = the gameobject chasing this
     *              changeDirection = lets this know when to change direction
     ****************************************************************************************************/
	public void runAway(GameObject follower, bool changeDirection)
	{
		right = transform.FindChild ("Right").gameObject;
		left = transform.FindChild ("Left").gameObject;

		targetDirection = -(follower.transform.position - transform.position).normalized;

		//Avoid asteroids
		RaycastHit hit = new RaycastHit ();
		targetDirection += castRay (transform.position, transform.forward, hit, VISION_LENGTH);
		targetDirection += castRay (left.transform.position, left.transform.forward, hit, VISION_LENGTH);
		targetDirection += castRay (right.transform.position, right.transform.forward, hit, VISION_LENGTH);
		/*
		if (changeDirection) 
		{
            //Avoid out-of-bounds
            do {
            targetDirection = new Vector3(Random.Range(Vector3.zero.x - 90, Vector3.zero.x + 90),
                                                Random.Range(Vector3.zero.y - 90, Vector3.zero.y + 90),
                                                Random.Range(Vector3.zero.z - 90, Vector3.zero.z + 90));
            } while (Vector3.Distance(targetDirection, Vector3.zero) > maxTravelDistance);

            targetDirection = targetDirection.normalized;
		}*/

		//Rotates the invader
		//Quaternion lookRotation = Quaternion.identity;
		//if (targetDirection != Vector3.zero)
		// lookRotation = Quaternion.LookRotation (targetDirection);
		transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation(targetDirection), Time.deltaTime);
		rbody.velocity = transform.forward * speed;
		Debug.Log(Vector3.Distance(transform.position, asteroid.transform.position));

		//return targetDirection;
	}


	/****************************************************************************************************
     * Description: Calculates the normal of an incoming asteroid
     * Syntax: castRay(origin, direction, hit, distance);
     *              origin = origin of the ray
     *              direction = direction of the ray
     *              hit = RaycastHit
     *              distance = how far to raycast
     ****************************************************************************************************/
	private Vector3 castRay(Vector3 origin, Vector3 direction, RaycastHit hit, float distance)
	{
		Vector3 normal = Vector3.zero;
		if (Physics.Raycast(origin, direction, out hit, distance))
		{
			if (hit.transform.root.gameObject.tag == "Asteroid")
				normal = hit.normal * 4;
		}
		return normal;
	}
}
