using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour {

    public GameObject dBox;
    public Text dText;
    public bool dialogActive;
    private int lineIndex = 0;
    private string[] DialogLines;
    private bool dBoxIsMoving = false;
    private Animator dBoxAnim;

    // Use this for initialization
    void Start ()
    {
        dBoxAnim = GetComponent<Animator>();
        dBox.SetActive(false);
        dialogActive = false;

    }
	
	// Update is called once per frame
	void Update ()
    {
        if (dialogActive && Input.GetKeyDown(KeyCode.Space) && !dBoxIsMoving)
        {
            lineIndex++;

            if (lineIndex + 1 > DialogLines.Length)
            {
                dBoxAnim.SetTrigger("BoxOut");
            }
            else
            {
                Type(DialogLines[lineIndex]);
            }

        }
    }

    public IEnumerator StartDialogue(string[] dialogLines)
    {
        if (!dialogActive)
        {
            PlayerController PC = FindObjectOfType<PlayerController>();
            PC.canMove = false;
            DialogLines = dialogLines;
            lineIndex = 0;
            dBox.SetActive(true);
            dialogActive = true;
            dText.text = string.Empty;

            yield return StartCoroutine(MoveOnScreen());

            Type(DialogLines[lineIndex]);
        }
    }



    private void Type(string textToType)
    {
        dText.text = textToType;
    }

    void AnimationComplete()
    {
        dBoxIsMoving = false;
    }

    public IEnumerator MoveOnScreen()
    {
        dBoxIsMoving = true;
        dBoxAnim.SetTrigger("BoxIn");

        while (dBoxIsMoving)
            yield return null;
    }

    public void HideDialogBox()
    {
        dBox.SetActive(false);
        dialogActive = false;
        PlayerController PC = FindObjectOfType<PlayerController>();
        PC.canMove = true;
    }

}
