using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;
using System.Linq;


public class StudentData : CharacterData
{
    // Use this for initialization
    void Start ()
    {
        if (GetComponent<AudioSource>() != null)
            voice = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    IEnumerator OnTriggerStay2D(Collider2D otherObject)
    {
        PhoneController phone = FindObjectOfType<PhoneController>();

        if (!dialogueEnabled)
            ChooseActiveDialogue();

        if (otherObject.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.E) && !dialogueEnabled && activeDialogue != null && !phone.phoneActive)
        {
            dialogueEnabled = true;
            DialogueSystem dialogue = GetComponent<DialogueSystem>();
            yield return StartCoroutine(dialogue.NPD_Dialogue(this));
            dialogueEnabled = false;
        }
    }
    
}
