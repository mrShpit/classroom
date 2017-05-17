using UnityEngine;
using System.Collections;

public class PlayerController : CharacterData
{
    public float speed;
    Rigidbody2D myBody;
    AnimatorController myAnim;
    float hInput;
    float vInput;
    public bool canMove
    {
        get
        {
            if (FindObjectOfType<DialogueBoxManager>().talkActive || FindObjectOfType<ExamProcess>().ExamIsRunning ||
                FindObjectOfType<SkillController>().skillPanel.activeInHierarchy || FindObjectOfType<DialogueBoxManager>().ChoiceMode)
                return false;
            else
                return true;
        }
    }
    private static bool ItExists;
    PlayerTriggerController triggerField;
    public int StartPoint = -1;

    void Start ()
    {
        if (!ItExists)
        {
            ItExists = true;
            DontDestroyOnLoad(transform.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        myBody = this.GetComponent<Rigidbody2D>();
        myAnim = AnimatorController.instance;

        triggerField = this.transform.Find("TriggerField").GetComponent<PlayerTriggerController>();
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
            triggerField.HorizontalMovement(hInput);
            return;
        }

        if (vInput != 0 && hInput == 0)
        {
            Move(0, vInput);
            myAnim.UpdateSpeed(0, vInput);
            triggerField.VerticalMovement(vInput);
            return;
        }
    }

    public void Move(float horizontalInput, float verticalInput)
    {
        Vector2 moveVel = new Vector2();
        moveVel.x = horizontalInput * speed;
        moveVel.y = verticalInput * speed;
        myBody.velocity = moveVel;
    }
}
