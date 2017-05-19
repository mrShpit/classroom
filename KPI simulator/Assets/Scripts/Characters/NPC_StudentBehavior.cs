using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;
using System.Linq;

public class NPC_StudentBehavior : NPC_Character
{
    IEnumerator OnTriggerStay2D(Collider2D otherObject)
    {
        PhoneController phone = FindObjectOfType<PhoneController>();

        if (!dialogueSystem.dialogueEnabled)
            dialogueSystem.ChooseActiveDialogue();

        if (otherObject.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.E) && !dialogueSystem.dialogueEnabled 
            && dialogueSystem.activeDialogue != null && !phone.phoneActive)
        {
            dialogueSystem.dialogueEnabled = true;
            yield return StartCoroutine(GetComponent<DialogueSystem>().NPD_Dialogue(this));
            dialogueSystem.dialogueEnabled = false;
        }
    }
}
