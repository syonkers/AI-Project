using UnityEngine;
using System.Collections;

public class AsteroidController : MonoBehaviour
{
    Vector3 velocity = Vector3.zero;
    float xVelocity;
    float yVelocity;
    float zVelocity;


    /****************************************************************************************************
     * Description: This is called once at the start of the game
     * Syntax: ---
     ****************************************************************************************************/
    void Start()
    {
        float delta = 0.001f;
        xVelocity = Random.Range(-delta, delta);
        yVelocity = Random.Range(-delta, delta);
        zVelocity = Random.Range(-delta, delta);
    }


    /****************************************************************************************************
     * Description: This is called once every frame
     * Syntax: ---
     ****************************************************************************************************/
    void Update()
    {
        transform.position += new Vector3(xVelocity, yVelocity, zVelocity);
    }
}