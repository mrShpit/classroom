using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;
using System.Linq;

public class TeacherData : NPC_CharacterData
{
    public Vector3[] ExamSitPlaces;
    public int xp, level;
    public int currPatience, currSatisfaction;
    public string subjectName;

    void Start()
    {
        if (GetComponent<AudioSource>() != null)
            voice = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator OnTriggerStay2D(Collider2D otherObject)
    {
        if(!dialogueEnabled)
            ChooseActiveDialogue();

        PhoneController phone = FindObjectOfType<PhoneController>();
        if (otherObject.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.E) && !dialogueEnabled && activeDialogue != null && !phone.phoneActive)
        {
            if (FindObjectOfType<ExamProcess>().ExamIsRunning)
                yield break;

            dialogueEnabled = true;
            yield return StartCoroutine(GetComponent<DialogueSystem>().NPD_Dialogue(this)); //Запустить активный диалог персонажа
            dialogueEnabled = false;

            if (Flag.FlagCheck(this.Character_Flags, new Flag("StartExam", 1)))
            {
                StartCoroutine(FindObjectOfType<ExamProcess>().StartExam(this, new List<NPC_CharacterData>())); //Список пуст, пока-что без напарников
            }

        }
    }
}