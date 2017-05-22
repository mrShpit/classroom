using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;
using System.Linq;

public class NPC_TeacherBehavior : NPC_Character
{
    IEnumerator OnTriggerStay2D(Collider2D otherObject)
    {
        if (!dialogueSystem.dialogueEnabled)
            dialogueSystem.ChooseActiveDialogue();

        PhoneController phone = FindObjectOfType<PhoneController>();
        if (otherObject.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.E) && !dialogueSystem.dialogueEnabled
            && dialogueSystem.activeDialogue != null && !phone.phoneActive)
        {
            if (FindObjectOfType<ExamProcess>().ExamIsRunning)
                yield break;

            dialogueSystem.dialogueEnabled = true;
            yield return StartCoroutine(GetComponent<DialogueSystem>().NPC_Dialogue()); //Запустить активный диалог персонажа
            dialogueSystem.dialogueEnabled = false;

            if (Flag.FlagCheck(FindObjectOfType<DirectorController>().WorldFlags, new Flag("StartExamMode", 1)))
            {
                StartCoroutine(FindObjectOfType<ExamProcess>().StartExam(this.GetComponent<TeacherData>(), 
                    new List<StudentData>() { FindObjectOfType<PlayerController>().GetComponent<StudentData>()})); //Пока-что без напарников
            }

        }
    }
}