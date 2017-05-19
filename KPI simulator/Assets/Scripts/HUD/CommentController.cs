using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CommentController : MonoBehaviour
{
    public List<string> MonologueLines = new List<string>();
    private bool dialogueEnabled;

	// Use this for initialization
	void Start ()
    {
        
    }

    IEnumerator OnTriggerStay2D(Collider2D otherObject)
    {
        if (otherObject.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.E) && !dialogueEnabled)
        {
            dialogueEnabled = true;
            DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();
            yield return StartCoroutine(DM.ShowDialogBox());
            yield return StartCoroutine(DM.Talk(string.Empty, MonologueLines, null));
            DM.HideDialogBox();
            dialogueEnabled = false;
        }
    }

}
