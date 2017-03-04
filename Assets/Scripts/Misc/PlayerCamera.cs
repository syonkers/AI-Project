using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerCamera : MonoBehaviour
{
    public Image cursor;
    public float maxSpeed = 1.0f;
    private Vector2 rotateDirection = Vector2.zero;
    private float sensitivityY = 0.5f;
    private float sensitivityX;
    private CursorLockMode cursorMode;
    private float currentVelocity;
    private Rigidbody rbody;

    /****************************************************************************************************
     * Description: This is called once at the start of the game
     * Syntax: ---
     ****************************************************************************************************/
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        sensitivityX = sensitivityY / 4;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /****************************************************************************************************
     * Description: This is called once every frame.
     * Syntax: ---
     ****************************************************************************************************/
    void Update()
    {
        ShipControl();
        CursorControl();
    }


    /****************************************************************************************************
     * Description: Helper function used to control the ship rotation
     * Syntax: ShipControl();
     ****************************************************************************************************/
    private void ShipControl()
    {
        //Move the ship forward based on vertical input
        currentVelocity += Input.GetAxis("Vertical") * Time.deltaTime;
        if (currentVelocity > maxSpeed) currentVelocity = maxSpeed;
        if (currentVelocity < -maxSpeed) currentVelocity = -maxSpeed;
        rbody.velocity = currentVelocity * transform.forward;

        //Rotate the ship based on horizontal input
        transform.eulerAngles += new Vector3(0, 0, -Input.GetAxis("Horizontal"));

        //Rotate the ship according to mouse movements
        rotateDirection += new Vector2(Input.GetAxis("Mouse X") * Time.deltaTime, Input.GetAxis("Mouse Y") * Time.deltaTime * 2);
        if (rotateDirection.x > sensitivityX) rotateDirection.x = sensitivityX;
        if (rotateDirection.x < -sensitivityX) rotateDirection.x = -sensitivityX;
        if (rotateDirection.y > sensitivityY) rotateDirection.y = sensitivityY;
        if (rotateDirection.y < -sensitivityY) rotateDirection.y = -sensitivityY;
        transform.Rotate(-rotateDirection.y, rotateDirection.x, 0);

        //Zero the ship rotation
        if (Input.GetButtonDown("Reset"))
            rotateDirection = Vector2.zero;
    }


    /****************************************************************************************************
     * Description: Helper function used to control the cursor UI
     * Syntax: CursorControl();
     ****************************************************************************************************/
    private void CursorControl()
    {
        //Lock and unlock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        if (Input.GetButtonDown("Cancel"))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        //Rotate the cursor UI based on the ships rotation
        cursor.transform.eulerAngles = new Vector3(0.0f, 0.0f, Mathf.Atan2(rotateDirection.y, rotateDirection.x * 2) * Mathf.Rad2Deg);

        //Controls the alpha and position of the cursor based on rotation
        Vector2 alphaValues = new Vector2(rotateDirection.x / sensitivityX, rotateDirection.y / sensitivityY);
        cursor.rectTransform.anchoredPosition = new Vector3(alphaValues.x * 50, alphaValues.y * 50, 0);
        alphaValues = new Vector2(Mathf.Abs(alphaValues.x), Mathf.Abs(alphaValues.y));
        float alpha = (alphaValues.x > alphaValues.y) ? alphaValues.x : alphaValues.y;
        cursor.color = new Color(255, 255, 255, alpha);
    }


    void OnCollisionEnter()
    {
        currentVelocity = 0.0f;
    }
}