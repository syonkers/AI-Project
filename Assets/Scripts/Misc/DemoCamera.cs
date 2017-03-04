using UnityEngine;
using System.Collections;

public class DemoCamera : MonoBehaviour
{
    private GameController gameController;
    private GameObject hud;
    private float lastSwitch = 0.0f;
    private float waitTime = 0.0f;
    private int currentCamera = 0;
    public Transform target;


    /****************************************************************************************************
     * Description: This is called once at the start of the game
     * Syntax: ---
     ****************************************************************************************************/
    void Start()
    {
        gameController = GameObject.Find("Global Controller").GetComponent<GameController>();
        hud = GameObject.Find("Canvas");

        currentCamera = Random.Range(1, 4);
    }


    /****************************************************************************************************
     * Description: This is called once every frame
     * Syntax: ---
     ****************************************************************************************************/
    void Update()
    {
        //Switch to a different camera angle
        if (lastSwitch + waitTime < Time.time)
        {
            currentCamera = (currentCamera == 3) ? Random.Range(1, 3) : Random.Range(1, 4);
            lastSwitch = Time.time;
            waitTime = Random.Range(10f, 15f);
            
            transform.localRotation = Quaternion.identity;
            hud.SetActive(false);
            switch (currentCamera)
            {
                case 1:
                    target = NewTarget();
                    break;

                case 2:
                    target = NewTarget();
                    transform.localPosition = target.position + -target.forward * 5;
                    break;

                case 3:
                    transform.localPosition = new Vector3(25, 25, -100);
                    break;
            }
        }
        
        //Camera behaviours
        if (target != null || currentCamera == 3)
        { 
            switch (currentCamera)
            {
                case 1:
                    transform.localPosition = target.position + (target.up * 0.5f) - target.forward;
                    transform.localRotation = target.rotation;
                    hud.SetActive(true);
                    break;

                case 2:
                    if (Vector3.Distance(transform.position, target.position) > 5)
                        transform.position = Vector3.Lerp(transform.position, target.position + (target.up * 2), Time.deltaTime);
                    transform.LookAt(target.position + (target.up * 1));
                    break;

                case 3:
                    transform.position += transform.right * Time.deltaTime * 7;
                    transform.LookAt(Vector3.zero);
                    break;
            }
        }

        //If the target dies
        if (currentCamera != 3 && target != null)
        {
            if (target.gameObject.GetComponent<AI_Main>().health <= 0)
            {
                hud.SetActive(false);
                transform.LookAt(target);
            }
        }
    }


    /****************************************************************************************************
     * Description: Grabs a new random target from either team
     * Syntax: Transform = NewTarget();
     ****************************************************************************************************/
    private Transform NewTarget()
    {
        Transform target;
        do
        {
            int team = Random.Range(1, 3);
            int index = Random.Range(0, (team == 1) ? gameController.team1.Count - 1 : gameController.team2.Count - 1);
            target = (team == 1) ? gameController.team1[index].transform : gameController.team2[index].transform;
        } while (target.gameObject.GetComponent<AI_Main>().health <= 0);

        return target;
    }
}