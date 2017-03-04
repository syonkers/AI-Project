using UnityEngine;
using System.Collections;

public class CubeController : MonoBehaviour
{
    public Vector3 startPosition = Vector3.zero;
    private bool isDead = false;
    private float deathVelocity = 1;
    float xVelocity;
    float yVelocity;
    float zVelocity;


    /****************************************************************************************************
     * Description: This is called once at the start of the game
     * Syntax: ---
     ****************************************************************************************************/
    void Start()
    {
        startPosition = transform.localPosition;
    }


    /****************************************************************************************************
     * Description: This is called once every frame
     * Syntax: ---
     ****************************************************************************************************/
    void Update()
    {
        if (isDead)
        {
            transform.position += new Vector3(xVelocity, yVelocity, zVelocity) * Time.deltaTime;
            transform.Rotate(new Vector3(xVelocity, yVelocity, zVelocity));
        }
    }


    /****************************************************************************************************
     * Description: Controls cube velocity when invader dies
     * Syntax: Death(velocity);
     *          velocity = the velocity from the impacting collider
     ****************************************************************************************************/
    public void Death(Vector3 velocity)
    {
        isDead = true;
        xVelocity = Random.Range(-deathVelocity, deathVelocity);
        yVelocity = Random.Range(-deathVelocity, deathVelocity);
        zVelocity = Random.Range(-deathVelocity, deathVelocity);
    }


    /****************************************************************************************************
     * Description: Realigns the cube back to its original state
     * Syntax: ---
     ****************************************************************************************************/
    public void Respawn()
    {
        isDead = false;
        transform.localPosition = startPosition;
        transform.localRotation = Quaternion.identity;
    }
}
