using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwarmController : MonoBehaviour
{
    static float ENEMY_COLLISION_RADIUS = 10.0f;

    [Header("Boids")]
    public List<GameObject> allyBoids;
    public List<GameObject> asteroidBoids;

    [Header("Death Variables")]
    public bool deadTrigger = false;
    public Vector3 deathVelocity = Vector3.zero;
    
    private Rigidbody rbody;
    private SphereCollider sphereCollider;
    private Vector3 oldVelocity;
    private Vector3 newVelocity;
    private GameController gameController;
    private AI_Main aiMain;

    private float distanceToAllyCenter = 0.0f;
    private float maxDistanceToAllyCenter = 0.0f;

    private float allyCohesionFactor = 0.0f;
    private float allySeparationFactor = 0.0f;
    private float asteroidSeparationFactor = 15.0f;
    private int weightFactor = 10;
    

    /****************************************************************************************************
     * Description: This is called once at the start of the game
     * Syntax: ---
     ****************************************************************************************************/
    void Awake()
    {
        allyBoids = new List<GameObject>();
        asteroidBoids = new List<GameObject>();
        rbody = GetComponent<Rigidbody>();
        aiMain = GetComponent<AI_Main>();
        gameController = GameObject.Find("Global Controller").GetComponent<GameController>();
        sphereCollider = GetComponent<SphereCollider>();
        maxDistanceToAllyCenter = GetComponent<SphereCollider>().radius * 2;
    }

    /****************************************************************************************************
     * Description: This is called once every frame
     * Syntax: ---
     ****************************************************************************************************/
    public void Swarm()
    {
     	//Calculate the direction the invader should travel in
       	oldVelocity = rbody.velocity;
      	newVelocity = Vector3.zero;
        if (allyBoids.Count > 0)
         	newVelocity += (AllyCohesion() * allyCohesionFactor) + (AllySeparation() * allySeparationFactor);
        if (Time.time > aiMain.respawnTime + 5)
      	    newVelocity += (CenterCohesion() * (weightFactor / 2));
      	if (asteroidBoids.Count > 0)
     		newVelocity += AsteroidSeparation() * asteroidSeparationFactor;
      	newVelocity = Vector3.Lerp(oldVelocity, newVelocity, Time.deltaTime);
      	rbody.velocity = newVelocity;
      	CalculateWeightFactors();

     	//Rotate the invader to the direction they are travelling
     	if (rbody.velocity != Vector3.zero)
           	transform.rotation = Quaternion.LookRotation(rbody.velocity, Vector3.up);
    }


    /****************************************************************************************************
     * Description: Resets invader when it dies
     * Syntax: DeathReset();
     ****************************************************************************************************/
    public void DeathReset()
	{
		asteroidBoids.Clear();
		foreach (GameObject ally in allyBoids)
			ally.GetComponent<SwarmController>().allyBoids.Remove(gameObject);
		allyBoids.Clear();
	}


    /****************************************************************************************************
     * Description: Calculates the weight of each swarm factor
     * Syntax: CalculateWeightFactors();
     ****************************************************************************************************/
    void CalculateWeightFactors()
    {
        allyCohesionFactor = (distanceToAllyCenter / maxDistanceToAllyCenter) * weightFactor;
        allySeparationFactor = weightFactor - allyCohesionFactor;
    }


    /****************************************************************************************************
     * Description: Calculates the overall alignment of all neighbors
     * Syntax: Vector3 direction = Alignment();
     * Returns: The overall velocity of the group
     ****************************************************************************************************/
    Vector3 Alignment()
    {
        Vector3 velocity = Vector3.zero;
        int neighbors = 0;
        foreach (GameObject boid in allyBoids)
        {
            velocity += boid.GetComponent<Rigidbody>().velocity;
            neighbors++;
        }
        velocity /= neighbors;
        return velocity.normalized;
    }


    /****************************************************************************************************
     * Description: Calculates the overall cohesion of all neighbors
     * Syntax: Vector3 direction = AllyCohestion();
     * Returns: The vector to the average center of allies
     ****************************************************************************************************/
    Vector3 AllyCohesion()
    {
        Vector3 position = Vector3.zero;
        int allies = 0;
        foreach (GameObject boid in allyBoids)
        {
            position += boid.transform.position;
            allies++;
        }
        position /= allies;
        Vector3 velocity = position - transform.position;
        distanceToAllyCenter = Vector3.Distance(position, transform.position);
        return velocity.normalized;
    }


    /****************************************************************************************************
     * Description: Calculates the overall separation of all neighbors
     * Syntax: Vector3 direction = AllySeparation();
     * Returns: The vector away from the average center of allies
     ****************************************************************************************************/
    Vector3 AllySeparation()
    {
        Vector3 position = Vector3.zero;
        int allies = 0;
        float weight = 0;
        foreach (GameObject boid in allyBoids)
        {
            Vector3 delta = boid.transform.position - transform.position;
            float radius = delta.magnitude / sphereCollider.radius;
            float boidWeight = (1 - radius) / (1 + radius);
            position += delta * boidWeight;
            weight += boidWeight;
            allies++;
        }
        Vector3 velocity = -position / (allies * weight);
        return velocity.normalized;
    }



    /****************************************************************************************************
     * Description: Calculates the overall cohesion to the center of the enemy spawn
     * Syntax: Vector3 direction = CenterCohesion();
     * Returns: The vector to the center of the universe
     ****************************************************************************************************/
    Vector3 CenterCohesion()
    {
        Vector3 center = (tag == "Team1") ? gameController.team2Spawn : gameController.team1Spawn;
        Vector3 velocity = (center - transform.position);
        return velocity.normalized;
    }


    /****************************************************************************************************
     * Description: Calculates the overall separation of all asteroids
     * Syntax: Vector3 direction = AsteroidSeparation();
     * Returns: The vector away from asteroids
     ****************************************************************************************************/
    Vector3 AsteroidSeparation()
    {
        Vector3 position = Vector3.zero;
        int asteroids = 0;
        float weight = 0;
        foreach (GameObject boid in asteroidBoids)
        {
            if (Vector3.Distance(transform.position, boid.transform.position) < sphereCollider.radius / 2)
            {
                Vector3 delta = boid.transform.position - transform.position;
                float radius = delta.magnitude / sphereCollider.radius;
                float boidWeight = (1 - radius) / (1 + radius);
                position += delta * boidWeight;
                weight += boidWeight;
                asteroids++;
            }
        }
        Vector3 velocity = -position / (asteroids * weight);
        return velocity.normalized;
    }


    /****************************************************************************************************
     * Description: Is called when a collision occurs between this
     * Syntax: ---
     ****************************************************************************************************/
    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Asteroid")
            asteroidBoids.Add(col.gameObject);
        else if (col.tag == this.tag)
            allyBoids.Add(col.gameObject);
    }



    /****************************************************************************************************
     * Description: Is called when a collision exits this
     * Syntax: ---
     ****************************************************************************************************/
    void OnTriggerExit(Collider col)
    {
        if (col.tag == "Asteroid")
            asteroidBoids.Remove(col.gameObject);
        else if (col.tag == this.tag)
            allyBoids.Remove(col.gameObject);
    }

    //This was used for testing purposes
    /*void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, AllyCohesion() * allyCohesionFactor * 2);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, AllySeparation() * allySeparationFactor * 2);
    }*/
}