using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;
using System.Linq;

public class Student_NPC : NPC_CharacterData
{
    // Use this for initialization
    void Start ()
    {
        if (GetComponent<AudioSource>() != null)
            voice = GetComponent<AudioSource>();
    }
	
    IEnumerator OnTriggerStay2D(Collider2D otherObject)
    {
        PhoneController phone = FindObjectOfType<PhoneController>();

        if (!dialogueEnabled)
            ChooseActiveDialogue();

        if (otherObject.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.E) && !dialogueEnabled && activeDialogue != null && !phone.phoneActive)
        {
            dialogueEnabled = true;
            yield return StartCoroutine(GetComponent<DialogueSystem>().NPD_Dialogue(this));
            dialogueEnabled = false;
        }
    }
}
