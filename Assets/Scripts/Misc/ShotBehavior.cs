using UnityEngine;
using System.Collections;

public class ShotBehavior : MonoBehaviour
{
    private float timeTillVoid;


    /****************************************************************************************************
     * Description: This is called once at the start of the game
     * Syntax: ---
     ****************************************************************************************************/
    void Start()
    {
        timeTillVoid = Time.time + 0.25f;
    }


    /****************************************************************************************************
     * Description: This is called once every frame
     * Syntax: ---
     ****************************************************************************************************/
    void Update ()
    {
        if (Time.time > timeTillVoid)
            gameObject.GetComponentInChildren<BoxCollider>().enabled = false;
		transform.position += transform.forward * Time.deltaTime * 200f;
	}


    void OnTriggerEnter(Collider col)
    {
        GameObject.Destroy(this);
    }
}