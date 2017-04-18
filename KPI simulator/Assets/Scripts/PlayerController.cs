using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    public float speed;
    Rigidbody2D myBody;
    AnimatorController myAnim;
    float hInput;
    float vInput;
    public bool canMove = true;
    
	void Start ()
    {
        myBody = this.GetComponent<Rigidbody2D>();
        myAnim = AnimatorController.instance;
	}
	
	void FixedUpdate ()
    {

        hInput = Input.GetAxisRaw("Horizontal");
        vInput = Input.GetAxisRaw("Vertical");

        if ((hInput == 0 && vInput == 0) || !canMove)
        {
            Move(0, 0);
            myAnim.UpdateSpeed(0, 0);
            return;
        }

        if (hInput != 0 && vInput == 0)
        {
            Move(hInput, 0);
            myAnim.UpdateSpeed(hInput, 0);
            return;
        }

        if (vInput != 0 && hInput == 0)
        {
            Move(0, vInput);
            myAnim.UpdateSpeed(0, vInput);
            return;
        }

        
    }

    void OnTriggerStay2D(Collider2D otherObject)
    {
        if (otherObject.gameObject.tag == "Door" && Input.GetKeyDown(KeyCode.E))
        {
            //otherObject.gameObject.GetComponent<DoorBehaviour>().Interact();
        }

        if (otherObject.gameObject.tag == "Interactable" && Input.GetKeyDown(KeyCode.E))
        {
            canMove = false;
            otherObject.gameObject.GetComponent<InteractionScript>().Interact();
        }
    }


    public void Move(float horizontalInput, float verticalInput)
    {
        Vector2 moveVel = myBody.velocity;
        moveVel.x = horizontalInput * speed;
        moveVel.y = verticalInput * speed;
        myBody.velocity = moveVel;
    }


    
}
