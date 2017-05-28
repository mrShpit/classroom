using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;
using System.Linq;

public class NPC_StudentBehavior : NPC_Character
{
    IEnumerator OnTriggerStay2D(Collider2D otherObject)
    {
        if (!dialogueSystem.dialogueEnabled)
            dialogueSystem.ChooseActiveDialogue();

        PhoneController phone = FindObjectOfType<PhoneController>();

        if (otherObject.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.E) && !dialogueSystem.dialogueEnabled 
            && dialogueSystem.activeDialogue != null && !phone.phoneActive)
        {
            dialogueSystem.dialogueEnabled = true;
            yield return StartCoroutine(GetComponent<DialogueSystem>().NPC_Dialogue());
            dialogueSystem.dialogueEnabled = false;
        }
    }
}