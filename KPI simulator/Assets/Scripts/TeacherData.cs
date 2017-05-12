using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;
using System.Linq;

public class TeacherData : CharacterData
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
        PhoneController phone = FindObjectOfType<PhoneController>();

        if(!dialogueEnabled)
            ChooseActiveDialogue();

        if (otherObject.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.E) && !dialogueEnabled && activeDialogue != null && !phone.phoneActive)
        {
            if (FindObjectOfType<ExamProcess>().ExamIsRunning)
                yield break;

            dialogueEnabled = true;
            DialogueSystem dialogue = GetComponent<DialogueSystem>();
            yield return StartCoroutine(dialogue.NPD_Dialogue(this)); //Запустить активный диалог персонажа
            dialogueEnabled = false;

            if (Flag.FlagCheck(this.Character_Flags, new Flag("StartExam", 1)))
            {
                StartCoroutine(FindObjectOfType<ExamProcess>().StartExam(this, new List<CharacterData>())); //Пока-что без напарников
            }

        }
    }
}